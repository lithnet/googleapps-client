using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Group = Google.Apis.Admin.Directory.directory_v1.Data.Group;

namespace Lithnet.GoogleApps
{
    public class GroupRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public GroupMemberRequestFactory MemberFactory { get; private set; }

        public GroupSettingsRequestFactory SettingsFactory { get; private set; }

        public GroupRequestFactory(GoogleServiceCredentials creds, string[] groupScopes, string[] groupSettingsScopes, int groupPoolSize, int settingsPoolSize)
        {
            this.directoryServicePool = new BaseClientServicePool<DirectoryService>(groupPoolSize, () =>
            {
                DirectoryService x = new DirectoryService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new ServiceAccountCredential(creds.GetInitializer(groupScopes)),
                    ApplicationName = "LithnetGoogleAppsLibrary",
                    GZipEnabled = !Settings.DisableGzip,
                    Serializer = new GoogleJsonSerializer(),
                    DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                });

                x.HttpClient.Timeout = Settings.DefaultTimeout;
                return x;
            });

            this.MemberFactory = new GroupMemberRequestFactory(this.directoryServicePool);
            this.SettingsFactory = new GroupSettingsRequestFactory(creds, groupSettingsScopes, settingsPoolSize);
        }

        public IEnumerable<GoogleGroup> GetUserGroups(string userKey, bool getMembers, bool getSettings, string groupFields, int memberThreads, int settingsThreads, bool excludeUserCreated = false, Regex regexFilter = null)
        {
            BlockingCollection<GoogleGroup> completedGroups = new BlockingCollection<GoogleGroup>();

            Task t = new Task(() => this.PopulateGroups(null, userKey, groupFields, getMembers, getSettings, completedGroups, excludeUserCreated, regexFilter, settingsThreads, memberThreads));
            t.Start();

            foreach (GoogleGroup group in completedGroups.GetConsumingEnumerable())
            {
                Debug.WriteLine($"Group enumeration completed: {group.Group.Email}");
                yield return group;
            }
        }

        public IEnumerable<GoogleGroup> GetGroups(string customerID, bool getMembers, bool getSettings, string groupFields, int memberThreads, int settingsThreads, bool excludeUserCreated = false, Regex regexFilter = null)
        {
            BlockingCollection<GoogleGroup> completedGroups = new BlockingCollection<GoogleGroup>();

            Task t = new Task(() => this.PopulateGroups(customerID, null, groupFields, getMembers, getSettings, completedGroups, excludeUserCreated, regexFilter, settingsThreads, memberThreads));
            t.Start();

            foreach (GoogleGroup group in completedGroups.GetConsumingEnumerable())
            {
                Debug.WriteLine($"Group enumeration completed: {group.Group.Email}");
                yield return group;
            }
        }

        private void PopulateGroups(string customerID, string userKey, string groupFields, bool getMembers, bool getSettings, BlockingCollection<GoogleGroup> completedGroups, bool excludeUserCreated, Regex regexFilter, int settingsThreads, int memberThreads)
        {
            BlockingCollection<GoogleGroup> settingsQueue = new BlockingCollection<GoogleGroup>();
            BlockingCollection<GoogleGroup> membersQueue = new BlockingCollection<GoogleGroup>();
            BlockingCollection<GoogleGroup> incomingGroups = new BlockingCollection<GoogleGroup>();

            List<Task> tasks = new List<Task>();

            if (getSettings)
            {
                Task t = new Task(() => this.ConsumeGroupSettingsQueue(settingsThreads, settingsQueue, completedGroups));
                t.Start();
                tasks.Add(t);
            }

            if (getMembers)
            {
                Task t = new Task(() => this.ConsumeGroupMembershipQueue(memberThreads, membersQueue, completedGroups));
                t.Start();
                tasks.Add(t);
            }

            Task t1 = new Task(() =>
            {
                using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    string token = null;
                    GroupsResource.ListRequest request = connection.Item.Groups.List();
                    request.Customer = customerID;
                    request.UserKey = userKey;
                    request.MaxResults = 200;
                    request.Fields = groupFields;
                    request.PrettyPrint = false;

                    do
                    {
                        request.PageToken = token;

                        Groups pageResults = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout);

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

        private void ConsumeGroupMembershipQueue(int threads, BlockingCollection<GoogleGroup> groups, BlockingCollection<GoogleGroup> completedGroups)
        {
            ParallelOptions op = new ParallelOptions { MaxDegreeOfParallelism = threads };

            Parallel.ForEach(groups.GetConsumingEnumerable(), op, (group) =>
            {
                try
                {
                    lock (group)
                    {
                        try
                        {
                            group.Membership = this.MemberFactory.GetMembership(group.Group.Email);
                        }
                        catch (Exception ex)
                        {
                            group.Errors.Add(ex);
                        }
                        finally
                        {
                            group.LoadedMembers = true;
                        }

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

        private void ConsumeGroupSettingsQueue(int threads, BlockingCollection<GoogleGroup> groups, BlockingCollection<GoogleGroup> completedGroups)
        {
            ParallelOptions op = new ParallelOptions { MaxDegreeOfParallelism = threads };

            Parallel.ForEach(groups.GetConsumingEnumerable(), op, (group) =>
            {
                try
                {
                    lock (group)
                    {
                        try
                        {
                            group.Settings = this.SettingsFactory.Get(group.Group.Email);
                        }
                        catch (Exception ex)
                        {
                            group.Errors.Add(ex);
                        }
                        finally
                        {
                            group.LoadedSettings = true;
                        }

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

        public string Delete(string groupKey)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.DeleteRequest request = connection.Item.Groups.Delete(groupKey);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public Group Add(Group item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.InsertRequest request = connection.Item.Groups.Insert(item);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public Group Update(string groupKey, Group item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.UpdateRequest request = connection.Item.Groups.Update(item, groupKey);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public Group Patch(string groupKey, Group item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.PatchRequest request = connection.Item.Groups.Patch(item, groupKey);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public Group Get(string groupKey)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.GetRequest request = connection.Item.Groups.Get(groupKey);
                Group result = request.ExecuteWithRetryOnBackoff();
                return result;
            }
        }

        public void AddAlias(string id, string newAlias)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                Alias alias = new Alias { AliasValue = newAlias };

                GroupsResource.AliasesResource.InsertRequest request = connection.Item.Groups.Aliases.Insert(alias, id);
                Alias result = request.ExecuteWithRetry(RetryEvents.BackoffNotFound);
            }
        }

        public void RemoveAlias(string id, string existingAlias)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.AliasesResource.DeleteRequest request = connection.Item.Groups.Aliases.Delete(id, existingAlias);
                request.ExecuteWithRetryOnBackoff();
            }
        }

        public IEnumerable<string> GetAliases(string id)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                GroupsResource.AliasesResource.ListRequest request = connection.Item.Groups.Aliases.List(id);
                Aliases aliases = request.ExecuteWithRetryOnBackoff();
                return aliases?.AliasesValue?.Select(t => t.AliasValue) ?? new List<string>();
            }
        }
    }
}