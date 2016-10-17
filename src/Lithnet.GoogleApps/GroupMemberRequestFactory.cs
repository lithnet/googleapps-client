using System;
using System.Collections.Generic;
using System.Diagnostics;
using Google;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Requests;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps
{
    public static class GroupMemberRequestFactory
    {
        // Batch size can technically be up to 1000, but a google API error is returned with requests that seem to take more than 5.5 minutes
        // 500 works, but can timeout on the client side, unless the default HttpClient timeout is raised from the default of 100 seconds
        // @ 250 we seem to get comparible updated objects/sec as at batch sizes of 500 and 750.
        private const int BatchSize = 250;

        public static GroupMembership GetMembership(string groupKey)
        {
            Stopwatch timer = new Stopwatch();
            GroupMembership membership = new GroupMembership();

            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                MembersResource.ListRequest request = poolService.Item.Members.List(groupKey);
                request.PrettyPrint = false;

                timer.Start();

                do
                {
                    request.PageToken = token;

                    Members members = request.ExecuteWithBackoff<Members>();

                    if (members.MembersValue != null)
                    {
                        foreach (Member member in members.MembersValue)
                        {
                            if (!string.IsNullOrWhiteSpace(member.Email))
                            {
                                membership.AddMember(member);
                            }
                        }
                    }

                    token = members.NextPageToken;
                }
                while (token != null);

                timer.Stop();
            }

            return membership;
        }

        public static void AddMember(string groupID, string memberID, string role)
        {
            Member member = new Member();

            if (memberID.IndexOf('@') < 0)
            {
                member.Id = memberID;
            }
            else
            {
                member.Email = memberID;
            }

            member.Role = role;

            AddMember(groupID, member);
        }

        public static void AddMember(string groupID, Member item)
        {
            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MembersResource.InsertRequest request = poolService.Item.Members.Insert(item, groupID);
                request.ExecuteWithBackoff();
            }
        }

        public static void RemoveMember(string groupID, string memberID)
        {
            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MembersResource.DeleteRequest request = poolService.Item.Members.Delete(groupID, memberID);

                request.ExecuteWithBackoff();
            }
        }

        public static void AddMembers(string id, IList<Member> members, bool throwOnExistingMember)
        {
            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                List<MembersResource.InsertRequest> requests = new List<MembersResource.InsertRequest>();
                string poolSerivceName = poolService.Item.Name;

                foreach (Member member in members)
                {
                    requests.Add(poolService.Item.Members.Insert(member, id));
                }
                
                List<string> failedMembers = new List<string>();
                List<Exception> failures = new List<Exception>();

                foreach (IEnumerable<MembersResource.InsertRequest> batch in requests.Batch(GroupMemberRequestFactory.BatchSize))
                {
                    BatchRequest batchRequest = new BatchRequest(poolService.Item);

                    foreach (MembersResource.InsertRequest request in batch)
                    {
                        batchRequest.Queue<MembersResource>(request,
                            (content, error, i, message) =>
                            {
                                if (error == null)
                                {
                                    return;
                                }

                                Member itemKey = members[i];
                                
                                string errorString = string.Format(
                                    "{0}\nFailed member add: {1}\nGroup: {2}",
                                    error.ToString(),
                                    itemKey == null ? string.Empty : itemKey.Email,
                                    id);

                                if (!throwOnExistingMember)
                                {
                                    if (message.StatusCode == System.Net.HttpStatusCode.Conflict && errorString.ToLower().Contains("member already exists"))
                                    {
                                        return;
                                    }
                                }

                                GoogleApiException ex = new GoogleApiException(poolSerivceName, errorString);
                                ex.HttpStatusCode = message.StatusCode;

                                if (itemKey != null)
                                {
                                    failedMembers.Add(itemKey.Email);
                                }

                                failures.Add(ex);
                            });
                    }

                    batchRequest.ExecuteAsync().Wait();
                }

                if (failures.Count == 1)
                {
                    throw failures[0];
                }
                else if (failures.Count > 1)
                {
                    throw new AggregateGroupUpdateException(id, failedMembers, failures);
                }
            }
        }

        public static void RemoveMembers(string id, IList<string> members, bool throwOnMissingMember)
        {
            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                List<MembersResource.DeleteRequest> requests = new List<MembersResource.DeleteRequest>();
                string poolSerivceName = poolService.Item.Name;

                foreach (string member in members)
                {
                    requests.Add(poolService.Item.Members.Delete(id, member));
                }

                List<string> failedMembers = new List<string>();
                List<Exception> failures = new List<Exception>();

                foreach (IEnumerable<MembersResource.DeleteRequest> batch in requests.Batch(GroupMemberRequestFactory.BatchSize))
                {
                    BatchRequest batchRequest = new BatchRequest(poolService.Item);

                    foreach (MembersResource.DeleteRequest request in batch)
                    {
                        batchRequest.Queue<string>(request,
                            (content, error, i, message) =>
                            {
                                string itemKey = members[i];

                                if (error == null)
                                {
                                    return;
                                }

                                string errorString = string.Format("{0}\nFailed member delete: {1}\nGroup: {2}", error.ToString(), itemKey, id);

                                if (!throwOnMissingMember)
                                {
                                    if (message.StatusCode == System.Net.HttpStatusCode.NotFound && errorString.Contains("Resource Not Found: memberKey"))
                                    {
                                        return;
                                    }
                                }

                                GoogleApiException ex = new GoogleApiException(poolSerivceName, errorString);
                                ex.HttpStatusCode = message.StatusCode;
                                failedMembers.Add(itemKey);
                                failures.Add(ex);
                            });
                    }

                    batchRequest.ExecuteAsync().Wait();
                }

                if (failures.Count == 1)
                {
                    throw failures[0];
                }
                else if (failures.Count > 1)
                {
                    throw new AggregateGroupUpdateException(id, failedMembers, failures);
                }
            }
        }
    }
}
