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
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int totalGroups = 0;

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

            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                var request = connection.Client.Groups.List();
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

                    foreach (var myGroup in pageResults.GroupsValue)
                    {
                        GoogleGroup group = new GoogleGroup(myGroup);
                        GroupSettingsRequestFactory.AddJob(group);
                        membersGroup.Add(group);
                    }

                    totalGroups += pageResults.GroupsValue.Count;
                    token = pageResults.NextPageToken;

                } while (token != null);
            }

            TimeSpan enumerationElapsed = timer.Elapsed;
            
            totalGroups = membersGroup.Count;

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

            timer.Stop();

            //Logger.WriteLine("");
            //Logger.WriteLine("--------------------------------------------------------");
            //Logger.WriteLine("Groups enumeration completed: {0}", enumerationElapsed);
            //Logger.WriteLine("");
            //Logger.WriteLine("Groups settings completed:    {0}", GroupSettingsRequestFactory.Elapsed);
            //Logger.WriteLine("Groups membership completed:  {0}", GroupMemberRequestFactory.Elapsed);
            //Logger.WriteLine("");
            //Logger.WriteLine("Groups settings avg:          {1} threads @ {0:N3} / group", GroupSettingsRequestFactory.Elapsed.TotalSeconds / totalGroups, SettingsThreads);
            //Logger.WriteLine("Groups membership avg:        {1} threads @ {0:N3} / group", GroupMemberRequestFactory.Elapsed.TotalSeconds / totalGroups, MemberThreads);
            //Logger.WriteLine("");
            //Logger.WriteLine("Groups settings avg:          {1} threads @ {0:N3} / sec", totalGroups / GroupSettingsRequestFactory.Elapsed.TotalSeconds, SettingsThreads);
            //Logger.WriteLine("Groups membership avg:        {1} threads @ {0:N3} / sec", totalGroups / GroupMemberRequestFactory.Elapsed.TotalSeconds, MemberThreads);
            //Logger.WriteLine("");
            //Logger.WriteLine("Groups settings act avg:      {1} threads @ {0:N3} / group", GroupSettingsRequestFactory.CumlativeTime.TotalSeconds / totalGroups, SettingsThreads);
            //Logger.WriteLine("Groups membership act avg:    {1} threads @ {0:N3} / group", GroupMemberRequestFactory.CumlativeTime.TotalSeconds / totalGroups, MemberThreads);
            //Logger.WriteLine("");
            //Logger.WriteLine("Back-off retries              {0}", ApiExtensions.BackoffRetries);
            //Logger.WriteLine("");
            //Logger.WriteLine("Performance:                  {0} groups @ {1:N3} / group", totalGroups , (totalGroups/timer.Elapsed.TotalSeconds));
            //Logger.WriteLine("Total import time (groups):   {0}", timer.Elapsed);


        }

        public static string Delete(string groupKey)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    var request = connection.Client.Groups.Delete(groupKey);
//#if DEBUG
//                    Logger.WriteLine("DELETE GROUP request: {0}", groupKey);
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("DELETE GROUP response: {0}", result);
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("DELETE GROUP {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static Group Add(Group item)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    var request = connection.Client.Groups.Insert(item);
//#if DEBUG
//                    Logger.WriteLine("ADD GROUP request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("ADD GROUP response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("ADD GROUP {0} threw an exception", item.Email);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static Group Update(string groupKey, Group item)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    var request = connection.Client.Groups.Update(item, groupKey);
//#if DEBUG
//                    Logger.WriteLine("UPDATE GROUP request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("UPDATE GROUP response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("UPDATE GROUP {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static Group Patch(string groupKey, Group item)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    var request = connection.Client.Groups.Patch(item, groupKey);
//#if DEBUG
//                    Logger.WriteLine("PATCH GROUP request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("PATCH GROUP response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("PATCH GROUP {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static Group Get(string groupKey)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    var request = connection.Client.Groups.Get(groupKey);
//#if DEBUG
//                    Logger.WriteLine("GET GROUP request: {0}", groupKey);
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("GET GROUP response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("GET GROUP {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static void AddAlias(string id, string newAlias)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                Alias alias = new Alias();
                alias.AliasValue = newAlias;

                var request = connection.Client.Groups.Aliases.Insert(alias, id);

                try
                {
//#if DEBUG
//                    Logger.WriteLine("ADD ALIAS request");
//                    Logger.WriteSeparatorLine('-');
//                    Logger.WriteLine(connection.Client.Serializer.Serialize(alias));
//                    Logger.WriteSeparatorLine('-');
//#endif
                    Alias result = request.ExecuteWithBackoff();
//                    Logger.WriteLine("Added alias {0} to group {1}", newAlias, id);
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("An exception occurred adding alias {0} to group {1}", newAlias, id);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static void RemoveAlias(string id, string existingAlias)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = connection.Client.Groups.Aliases.Delete(id, existingAlias);

                try
                {
                    request.ExecuteWithBackoff();
//                    Logger.WriteLine("Removed alias {0} from group {1}", existingAlias, id);
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("An exception occurred removing alias {0} from group {1}", existingAlias, id);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static IEnumerable<string> GetAliases(string id)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = connection.Client.Groups.Aliases.List(id);

                try
                {
                    Aliases aliases = request.ExecuteWithBackoff();
                    return aliases.AliasesValue.Select(t => t.AliasValue);
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("An exception occurred getting aliases for group {0}", id);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }
    }
}