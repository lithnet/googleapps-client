using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        internal static string ServiceName = "GroupMemberRequestFactory";

        // Batch size can technically be up to 1000, but a google API error is returned with requests that seem to take more than 5.5 minutes
        // 500 works, but can timeout on the client side, unless the default HttpClient timeout is raised from the default of 100 seconds
        // @ 250 we seem to get comparable updated objects/sec as at batch sizes of 500 and 750.
        public static int BatchSize { get; set; } = 100;

        public static GroupMembership GetMembership(string groupKey)
        {
            GroupMembership membership = new GroupMembership();

            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                MembersResource.ListRequest request = poolService.Item.Members.List(groupKey);
                request.PrettyPrint = false;
                Trace.WriteLine($"Getting members from group {groupKey}");

                do
                {
                    request.PageToken = token;
                    Members members;

                    try
                    {
                        GroupMemberRequestFactory.WaitForGate();
                        members = request.ExecuteWithBackoff();
                    }
                    finally
                    {
                        GroupMemberRequestFactory.ReleaseGate();
                    }

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
                } while (token != null);
            }

            Trace.WriteLine($"Returned {membership.Count} members from group {groupKey}");

            return membership;
        }

        private static void WaitForGate()
        {
            RateLimiter.WaitForGate(GroupMemberRequestFactory.ServiceName);
        }

        private static void ReleaseGate()
        {
            RateLimiter.ReleaseGate(GroupMemberRequestFactory.ServiceName);
        }

        public static void AddMember(string groupID, string memberID, string role)
        {
            GroupMemberRequestFactory.AddMember(groupID, memberID, role, true);
        }

        public static void AddMember(string groupID, string memberID, string role, bool throwOnExistingMember)
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

            member.Role = role ?? "MEMBER";

            AddMember(groupID, member, throwOnExistingMember);
        }

        public static void AddMember(string groupID, Member item)
        {
            GroupMemberRequestFactory.AddMember(groupID, item, true);
        }

        public static void AddMember(string groupID, Member item, bool throwOnExistingMember)
        {
            try
            {
                GroupMemberRequestFactory.WaitForGate();

                using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    MembersResource.InsertRequest request = poolService.Item.Members.Insert(item, groupID);
                    Trace.WriteLine($"Adding member {item.Email ?? item.Id} as {item.Role} to group {groupID}");
                    request.ExecuteWithBackoff();
                }
            }
            catch (GoogleApiException e)
            {
                if (!throwOnExistingMember && GroupMemberRequestFactory.IsExistingMemberError(e.HttpStatusCode, e.Message))
                {
                    return;
                }

                throw;
            }
            finally
            {
                GroupMemberRequestFactory.ReleaseGate();
            }
        }

        public static void ChangeMemberRole(string groupID, Member item)
        {
            try
            {
                GroupMemberRequestFactory.WaitForGate();

                using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    MembersResource.PatchRequest request = poolService.Item.Members.Patch(item, groupID, item.Email);
                    Trace.WriteLine($"Changing member {item.Email ?? item.Id} role to {item.Role} in group {groupID}");
                    request.ExecuteWithBackoff();
                }
            }
            finally
            {
                GroupMemberRequestFactory.ReleaseGate();
            }
        }

        public static void RemoveMember(string groupID, string memberID)
        {
            GroupMemberRequestFactory.RemoveMember(groupID, memberID, true);
        }

        public static void RemoveMember(string groupID, string memberID, bool throwOnMissingMember)
        {
            try
            {
                GroupMemberRequestFactory.WaitForGate();

                using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    MembersResource.DeleteRequest request = poolService.Item.Members.Delete(groupID, memberID);
                    Trace.WriteLine($"Removing member {memberID} from group {groupID}");
                    try
                    {
                        request.ExecuteWithBackoff();
                    }
                    catch (GoogleApiException e)
                    {
                        if (!throwOnMissingMember && GroupMemberRequestFactory.IsMissingMemberError(e.HttpStatusCode, e.Message))
                        {
                            return;
                        }

                        throw;
                    }
                }
            }
            finally
            {
                GroupMemberRequestFactory.ReleaseGate();
            }
        }

        public static void AddMembers(string id, IList<Member> members, bool throwOnExistingMember)
        {
            if (GroupMemberRequestFactory.BatchSize <= 1)
            {
                foreach (Member member in members)
                {
                    GroupMemberRequestFactory.AddMember(id, member, throwOnExistingMember);
                }

                return;
            }

            try
            {
                GroupMemberRequestFactory.WaitForGate();

                using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Member>> requests = new List<ClientServiceRequest<Member>>();

                    foreach (Member member in members)
                    {
                        Trace.WriteLine($"Queuing batch member add for {member.Email ?? member.Id} as {member.Role} to group {id}");
                        requests.Add(poolService.Item.Members.Insert(member, id));
                    }

                    GroupMemberRequestFactory.ProcessBatches(id, members, !throwOnExistingMember, false, requests, poolService);
                }
            }
            finally
            {
                GroupMemberRequestFactory.ReleaseGate();
            }
        }

        private static void ProcessBatches<T>(string id, IList<T> members, bool ignoreExistingMember, bool ignoreMissingMember, IList<ClientServiceRequest<T>> requests, PoolItem<DirectoryService> poolService)
        {
            List<string> failedMembers = new List<string>();
            List<Exception> failures = new List<Exception>();
            Dictionary<string, ClientServiceRequest<T>> requestsToRetry = new Dictionary<string, ClientServiceRequest<T>>();

            int baseCount = 0;
            int batchCount = 0;

            foreach (IEnumerable<ClientServiceRequest<T>> batch in requests.Batch(GroupMemberRequestFactory.BatchSize))
            {
                BatchRequest batchRequest = new BatchRequest(poolService.Item);
                Trace.WriteLine($"Executing batch {++batchCount} for group {id}");

                foreach (ClientServiceRequest<T> request in batch)
                {
                    batchRequest.Queue<MembersResource>(request,
                        (content, error, i, message) =>
                        {
                            int index = baseCount + i;
                            GroupMemberRequestFactory.ProcessMemberResponse(id, members[index], ignoreExistingMember, ignoreMissingMember, error, message, requestsToRetry, request, failedMembers, failures);
                        });
                }

                batchRequest.ExecuteWithBackoff(poolService.Item.Name);

                baseCount += GroupMemberRequestFactory.BatchSize;
            }

            if (requestsToRetry.Count > 0)
            {
                Trace.WriteLine($"Retrying {requestsToRetry} member change requests");
            }

            foreach (KeyValuePair<string, ClientServiceRequest<T>> request in requestsToRetry)
            {
                try
                {
                    request.Value.ExecuteWithBackoff();
                }
                catch (GoogleApiException e)
                {
                    if (!(ignoreMissingMember && GroupMemberRequestFactory.IsMissingMemberError(e.HttpStatusCode, e.Message)))
                    {
                        if (!(ignoreExistingMember && GroupMemberRequestFactory.IsExistingMemberError(e.HttpStatusCode, e.Message)))
                        {
                            failures.Add(e);
                            failedMembers.Add(request.Key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    failures.Add(ex);
                    failedMembers.Add(request.Key);
                }
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

        private static void ProcessMemberResponse<T>(string id, T item, bool ignoreExistingMember, bool ignoreMissingMember, RequestError error, HttpResponseMessage message, Dictionary<string, ClientServiceRequest<T>> requestsToRetry, ClientServiceRequest<T> request, List<string> failedMembers, List<Exception> failures)
        {
            string memberKey;
            string memberRole = string.Empty;

            Member member = item as Member;
            if (member == null)
            {
                memberKey = item as string ?? "unknown";
            }
            else
            {
                memberKey = member.Email ?? member.Id;
                memberRole = member.Role;
            }

            string requestType = request.GetType().Name;

            if (error == null)
            {
                Trace.WriteLine($"{requestType}: Success: Member: {memberKey}, Role: {memberRole}, Group: {id}");
                return;
            }

            string errorString = $"{error}\nFailed {requestType}: {memberKey}\nGroup: {id}";

            Trace.WriteLine($"{requestType}: Failed: Member: {memberKey}, Role: {memberRole}, Group: {id}\n{error}");

            if (ignoreExistingMember && GroupMemberRequestFactory.IsExistingMemberError(message.StatusCode, errorString))
            {
                return;
            }

            if (ignoreMissingMember && GroupMemberRequestFactory.IsMissingMemberError(message.StatusCode, errorString))
            {
                return;
            }

            if (ApiExtensions.IsRetryableError(message.StatusCode, errorString))
            {
                Trace.WriteLine($"Queuing {requestType} of {memberKey} from group {id} for backoff/retry");
                requestsToRetry.Add(memberKey, request);
                return;
            }

            GoogleApiException ex = new GoogleApiException("admin", errorString);
            ex.HttpStatusCode = message.StatusCode;
            failedMembers.Add(memberKey);
            failures.Add(ex);
        }

        public static void RemoveMembers(string id, IList<string> members, bool throwOnMissingMember)
        {
            if (GroupMemberRequestFactory.BatchSize <= 1)
            {
                foreach (string member in members)
                {
                    GroupMemberRequestFactory.RemoveMember(id, member, throwOnMissingMember);
                }

                return;
            }

            try
            {
                GroupMemberRequestFactory.WaitForGate();

                using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<string>> requests = new List<ClientServiceRequest<string>>();

                    foreach (string member in members)
                    {
                        Trace.WriteLine($"Queuing batch member delete for {member} from group {id}");
                        requests.Add(poolService.Item.Members.Delete(id, member));
                    }

                    GroupMemberRequestFactory.ProcessBatches(id, members, false, !throwOnMissingMember, requests, poolService);
                }
            }
            finally
            {
                GroupMemberRequestFactory.ReleaseGate();
            }
        }

        public static void ChangeMemberRoles(string id, IList<Member> members)
        {
            if (GroupMemberRequestFactory.BatchSize <= 1)
            {
                foreach (Member member in members)
                {
                    GroupMemberRequestFactory.ChangeMemberRole(id, member);
                }

                return;
            }

            try
            {
                GroupMemberRequestFactory.WaitForGate();

                using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Member>> requests = new List<ClientServiceRequest<Member>>();

                    foreach (Member member in members)
                    {
                        string memberKey = member.Email ?? member.Id;

                        Trace.WriteLine($"Queuing batch member role change for {memberKey} as {member.Role} to group {id}");
                        requests.Add(poolService.Item.Members.Patch(member, id, memberKey));
                    }

                    GroupMemberRequestFactory.ProcessBatches(id, members, false, false, requests, poolService);
                }
            }
            finally
            {
                GroupMemberRequestFactory.ReleaseGate();
            }
        }

        private static bool IsExistingMemberError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.Conflict && message.IndexOf("member already exists", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsMissingMemberError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.NotFound && message.IndexOf("notFound", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            // This error code doesn't make sense, but the Google API has changed and now returns this response when a member is missing (as at 2016-12-02)
            if (statusCode == HttpStatusCode.BadRequest && message.IndexOf("Missing required field: memberKey", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }
    }
}