using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    using Google.Apis.Admin.Directory.directory_v1;

    public static class GroupRequestFactory
    {
        static GroupRequestFactory()
        {
            GroupRequestFactory.MemberThreads = 10;
            GroupRequestFactory.SettingsThreads = 30;
        }

        public static int MemberThreads { get; set; }

        public static int SettingsThreads { get; set; }

        public static void ImportGroups(string customerID, bool getMembers, bool getSettings, BlockingCollection<object> items)
        {
            ImportGroups(customerID, getMembers, getSettings, null, null, items);
        }

        public static void ImportGroups(string customerID, bool getMembers, bool getSettings, string groupFields, string settingsFields, BlockingCollection<object> items)
        {
            Task membersTask = null;
            Task settingsTask = null;

            if (getMembers)
            {
                membersTask = new Task(() => GroupMemberRequestFactory.ConsumeQueue(GroupRequestFactory.MemberThreads));
            }

            if (getSettings)
            {
                settingsTask = new Task(() => GroupSettingsRequestFactory.ConsumeQueue(GroupRequestFactory.SettingsThreads));
                settingsTask.Start();
            }

            List<GoogleGroup> membersGroup = new List<GoogleGroup>();

            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                GroupsResource.ListRequest request = connection.Item.Groups.List();
                request.Customer = customerID;
                request.MaxResults = 200;
                request.Fields = groupFields;
                request.PrettyPrint = false;
                Groups pageResults;

                do
                {
                    request.PageToken = token;

                    pageResults = request.ExecuteWithBackoff();

                    if (pageResults.GroupsValue == null)
                    {
                        break;
                    }

                    foreach (Group myGroup in pageResults.GroupsValue)
                    {
                        GoogleGroup group = new GoogleGroup(myGroup);
                        GroupSettingsRequestFactory.AddJob(group);
                        membersGroup.Add(group);
                    }
                   
                    token = pageResults.NextPageToken;

                } while (token != null);
            }

            if (getMembers)
            {
                foreach (GoogleGroup mygroup in membersGroup.OrderByDescending(t => t.Group.DirectMembersCount))
                {
                    GroupMemberRequestFactory.AddJob(mygroup);
                }

                membersTask.Start();
                GroupMemberRequestFactory.CompleteAdding();
            }

            if (getSettings)
            {
                GroupSettingsRequestFactory.CompleteAdding();
            }

            if (getSettings && getMembers)
            {
                Task.WaitAll(membersTask, settingsTask);
            }
            else if (getSettings)
            {
                settingsTask.Wait();
            }
            else if (getMembers)
            {
                membersTask.Wait();
            }

            foreach (GoogleGroup group in membersGroup)
            {
                items.Add(group);
            }
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
                return aliases.AliasesValue.Select(t => t.AliasValue);
            }
        }
    }
}