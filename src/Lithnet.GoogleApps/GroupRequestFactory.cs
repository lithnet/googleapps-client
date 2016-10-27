using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Google.Apis.Admin.Directory.directory_v1;
using Group = Google.Apis.Admin.Directory.directory_v1.Data.Group;

namespace Lithnet.GoogleApps
{

    public static class GroupRequestFactory
    {
        static GroupRequestFactory()
        {
            GroupRequestFactory.MemberThreads = 10;
            GroupRequestFactory.SettingsThreads = 30;
        }

        public static int MemberThreads { get; set; }

        public static int SettingsThreads { get; set; }
        
        public static IEnumerable<GoogleGroup> GetGroups(string customerID, bool getMembers, bool getSettings, string groupFields, string settingsFields, bool excludeUserCreated = false, Regex regexFilter = null)
        {
            BlockingCollection<GoogleGroup> completedGroups = new BlockingCollection<GoogleGroup>();

            Task t = new Task(() => GroupRequestFactory.PopulateGroups(customerID, groupFields, getSettings, getMembers, completedGroups, excludeUserCreated, regexFilter));
            t.Start();

            foreach (GoogleGroup group in completedGroups.GetConsumingEnumerable())
            {
                Debug.WriteLine($"Group enumeration completed: {group.Group.Email}");
                yield return group;
            }
        }

        private static void PopulateGroups(string customerID, string groupFields, bool getSettings, bool getMembers, BlockingCollection<GoogleGroup> completedGroups, bool excludeUserCreated, Regex regexFilter)
        {
            BlockingCollection<GoogleGroup> settingsQueue = new BlockingCollection<GoogleGroup>();
            BlockingCollection<GoogleGroup> membersQueue = new BlockingCollection<GoogleGroup>();
            BlockingCollection<GoogleGroup> incomingGroups = new BlockingCollection<GoogleGroup>();

            List<Task> tasks = new List<Task>();

            if (getSettings)
            {
                Task t = new Task(() => GroupRequestFactory.ConsumeGroupSettingsQueue(GroupRequestFactory.SettingsThreads, settingsQueue, completedGroups));
                t.Start();
                tasks.Add(t);
            }

            if (getMembers)
            {
                Task t = new Task(() => GroupRequestFactory.ConsumeGroupMembershipQueue(GroupRequestFactory.MemberThreads, membersQueue, completedGroups));
                t.Start();
                tasks.Add(t);
            }

            Task t1 = new Task(() =>
            {
                using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    string token = null;
                    GroupsResource.ListRequest request = connection.Item.Groups.List();
                    request.Customer = customerID;
                    request.MaxResults = 200;
                    request.Fields = groupFields;
                    request.PrettyPrint = false;

                    do
                    {
                        request.PageToken = token;

                        Groups pageResults = request.ExecuteWithBackoff();

                        if (pageResults.GroupsValue == null)
                        {
                            break;
                        }

                        foreach (Group group in pageResults.GroupsValue)
                        {
                            if (regexFilter != null)
                            {
                                if (!regexFilter.IsMatch(group.Email))
                                {
                                    Trace.WriteLine($"Ignoring group that doesn't match regex filter {group.Email}");
                                    continue;
                                }
                            }

                            if (excludeUserCreated)
                            {
                                if (!group.AdminCreated.HasValue || !group.AdminCreated.Value)
                                {
                                    Trace.WriteLine($"Ignoring user created group {group.Email}");
                                    continue;
                                }
                            }

                            GoogleGroup g = new GoogleGroup(group);
                            g.RequiresMembers = getMembers;
                            g.RequiresSettings = getSettings;

                            incomingGroups.Add(g);

                            if (getSettings)
                            {
                                settingsQueue.Add(g);
                            }

                            if (getMembers)
                            {
                                membersQueue.Add(g);
                            }

                            if (!getSettings && !getMembers)
                            {
                                completedGroups.Add(g);
                            }
                        }

                        token = pageResults.NextPageToken;

                    } while (token != null);

                    incomingGroups.CompleteAdding();
                    settingsQueue.CompleteAdding();
                    membersQueue.CompleteAdding();
                }
            });

            t1.Start();
            tasks.Add(t1);
            Task.WhenAll(tasks).ContinueWith((a) => completedGroups.CompleteAdding());
        }

        private static void ConsumeGroupMembershipQueue(int threads, BlockingCollection<GoogleGroup> groups, BlockingCollection<GoogleGroup> completedGroups)
        {
            ParallelOptions op = new ParallelOptions { MaxDegreeOfParallelism = threads };

            Parallel.ForEach(groups.GetConsumingEnumerable(), op, (group) =>
            {
                try
                {
                    lock (group)
                    {
                        group.GetMembership();
                        Debug.WriteLine($"Group membership completed: {group.Group.Email}");
                        
                        if (group.IsComplete)
                        {
                            completedGroups.Add(group);
                        }
                    }
                }
                catch (AggregateException ex)
                {
                    group.Errors.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    group.Errors.Add(ex);
                }
            });
        }

        private static void ConsumeGroupSettingsQueue(int threads, BlockingCollection<GoogleGroup> groups, BlockingCollection<GoogleGroup> completedGroups)
        {
            ParallelOptions op = new ParallelOptions { MaxDegreeOfParallelism = threads };

            Parallel.ForEach(groups.GetConsumingEnumerable(), op, (group) =>
            {
                try
                {
                    lock (group)
                    {
                        group.GetSettings();
                        Debug.WriteLine($"Group settings completed: {group.Group.Email}");

                        if (group.IsComplete)
                        {
                            completedGroups.Add(group);
                        }
                    }
                }
                catch (AggregateException ex)
                {
                    group.Errors.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    group.Errors.Add(ex);
                }
            });
        }

        public static string Delete(string groupKey)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.DeleteRequest request = connection.Item.Groups.Delete(groupKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static Group Add(Group item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.InsertRequest request = connection.Item.Groups.Insert(item);
                return request.ExecuteWithBackoff();
            }
        }

        public static Group Update(string groupKey, Group item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {

                GroupsResource.UpdateRequest request = connection.Item.Groups.Update(item, groupKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static Group Patch(string groupKey, Group item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.PatchRequest request = connection.Item.Groups.Patch(item, groupKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static Group Get(string groupKey)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.GetRequest request = connection.Item.Groups.Get(groupKey);
                Group result = request.ExecuteWithBackoff();
                return result;
            }
        }

        public static void AddAlias(string id, string newAlias)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                Alias alias = new Alias { AliasValue = newAlias };

                GroupsResource.AliasesResource.InsertRequest request = connection.Item.Groups.Aliases.Insert(alias, id);
                Alias result = request.ExecuteWithBackoff();
            }
        }

        public static void RemoveAlias(string id, string existingAlias)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.AliasesResource.DeleteRequest request = connection.Item.Groups.Aliases.Delete(id, existingAlias);
                request.ExecuteWithBackoff();
            }
        }

        public static IEnumerable<string> GetAliases(string id)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.AliasesResource.ListRequest request = connection.Item.Groups.Aliases.List(id);
                Aliases aliases = request.ExecuteWithBackoff();
                return aliases.AliasesValue.OfType<JObject>().Select(t => t.Value<string>("alias"));
            }
        }
    }
}