using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Google;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Requests;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps
{
    public class GroupMemberRequestFactory
    {
        private static string limiterName = "concurrent-group-member-requests";

        // Batch size can technically be up to 1000, but a google API error is returned with requests that seem to take more than 5.5 minutes
        // 500 works, but can timeout on the client side, unless the default HttpClient timeout is raised from the default of 100 seconds
        // @ 250 we seem to get comparable updated objects/sec as at batch sizes of 500 and 750.
        public static int BatchSize { get; set; } = 100;

        public static int ConcurrentOperationLimitDefault { get; set; } = 100;

        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        internal GroupMemberRequestFactory(BaseClientServicePool<DirectoryService> directoryServicePool)
        {
            this.directoryServicePool = directoryServicePool;
            RateLimiter.SetConcurrentLimit(GroupMemberRequestFactory.limiterName, GroupMemberRequestFactory.ConcurrentOperationLimitDefault);
        }

        private void WaitForGate()
        {
            RateLimiter.WaitForGate(GroupMemberRequestFactory.limiterName);
        }

        private void ReleaseGate()
        {
            RateLimiter.ReleaseGate(GroupMemberRequestFactory.limiterName);
        }

        public GroupMembership GetMembership(string groupKey)
        {
            GroupMembership membership = new GroupMembership();

            using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
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
                        this.WaitForGate();
                        members = request.ExecuteWithRetryOnBackoff();
                    }
                    finally
                    {
                        this.ReleaseGate();
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

        public void AddMember(string groupID, string memberID, string role)
        {
            this.AddMember(groupID, memberID, role, true);
        }

        public void AddMember(string groupID, string memberID, string role, bool throwOnExistingMember)
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

            this.AddMember(groupID, member, throwOnExistingMember);
        }

        public void AddMember(string groupID, Member item)
        {
            this.AddMember(groupID, item, true);
        }

        public void AddMember(string groupID, Member item, bool throwOnExistingMember)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    MembersResource.InsertRequest request = poolService.Item.Members.Insert(item, groupID);
                    Trace.WriteLine($"Adding member {item.Email ?? item.Id} as {item.Role} to group {groupID}");
                    request.ExecuteWithRetryOnBackoff();
                }
            }
            catch (GoogleApiException e)
            {
                if (!throwOnExistingMember && this.IsExistingMemberError(e.HttpStatusCode, e.Message))
                {
                    return;
                }

                throw;
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        public void ChangeMemberRole(string groupID, Member item)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    MembersResource.PatchRequest request = poolService.Item.Members.Patch(item, groupID, item.Email);
                    Trace.WriteLine($"Changing member {item.Email ?? item.Id} role to {item.Role} in group {groupID}");
                    request.ExecuteWithRetryOnBackoff();
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        public void RemoveMember(string groupID, string memberID)
        {
            this.RemoveMember(groupID, memberID, true);
        }

        public void RemoveMember(string groupID, string memberID, bool throwOnMissingMember)
        {
            try
            {
                this.WaitForGate();

                using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    MembersResource.DeleteRequest request = poolService.Item.Members.Delete(groupID, memberID);
                    Trace.WriteLine($"Removing member {memberID} from group {groupID}");
                    try
                    {
                        request.ExecuteWithRetryOnBackoff();
                    }
                    catch (GoogleApiException e)
                    {
                        if (!throwOnMissingMember && this.IsMissingMemberError(e.HttpStatusCode, e.Message))
                        {
                            return;
                        }

                        throw;
                    }
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        public void AddMembers(string id, IList<Member> members, bool throwOnExistingMember)
        {
            if (GroupMemberRequestFactory.BatchSize <= 1)
            {
                foreach (Member member in members)
                {
                    this.AddMember(id, member, throwOnExistingMember);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Member>> requests = new List<ClientServiceRequest<Member>>();

                    foreach (Member member in members)
                    {
                        Trace.WriteLine($"Queuing batch member add for {member.Email ?? member.Id} as {member.Role} to group {id}");
                        requests.Add(poolService.Item.Members.Insert(member, id));
                    }

                    this.ProcessBatches(id, members, !throwOnExistingMember, false, requests, poolService);
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        private void ProcessBatches<T>(string id, IList<T> members, bool ignoreExistingMember, bool ignoreMissingMember, IList<ClientServiceRequest<T>> requests, PoolItem<DirectoryService> poolService)
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
                            this.ProcessMemberResponse(id, members[index], ignoreExistingMember, ignoreMissingMember, error, message, requestsToRetry, request, failedMembers, failures);
                        });
                }

                batchRequest.ExecuteWithRetryOnBackoff(poolService.Item.Name);

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
                    request.Value.ExecuteWithRetryOnBackoff();
                }
                catch (GoogleApiException e)
                {
                    if (!(ignoreMissingMember && this.IsMissingMemberError(e.HttpStatusCode, e.Message)))
                    {
                        if (!(ignoreExistingMember && this.IsExistingMemberError(e.HttpStatusCode, e.Message)))
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

        private void ProcessMemberResponse<T>(string id, T item, bool ignoreExistingMember, bool ignoreMissingMember, RequestError error, HttpResponseMessage message, Dictionary<string, ClientServiceRequest<T>> requestsToRetry, ClientServiceRequest<T> request, List<string> failedMembers, List<Exception> failures)
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

            if (ignoreExistingMember && this.IsExistingMemberError(message.StatusCode, errorString))
            {
                return;
            }

            if (ignoreMissingMember && this.IsMissingMemberError(message.StatusCode, errorString))
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

        public void RemoveMembers(string id, IList<string> members, bool throwOnMissingMember)
        {
            if (GroupMemberRequestFactory.BatchSize <= 1)
            {
                foreach (string member in members)
                {
                    this.RemoveMember(id, member, throwOnMissingMember);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<string>> requests = new List<ClientServiceRequest<string>>();

                    foreach (string member in members)
                    {
                        Trace.WriteLine($"Queuing batch member delete for {member} from group {id}");
                        requests.Add(poolService.Item.Members.Delete(id, member));
                    }

                    this.ProcessBatches(id, members, false, !throwOnMissingMember, requests, poolService);
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        public void ChangeMemberRoles(string id, IList<Member> members)
        {
            if (GroupMemberRequestFactory.BatchSize <= 1)
            {
                foreach (Member member in members)
                {
                    this.ChangeMemberRole(id, member);
                }

                return;
            }

            try
            {
                this.WaitForGate();

                using (PoolItem<DirectoryService> poolService = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    List<ClientServiceRequest<Member>> requests = new List<ClientServiceRequest<Member>>();

                    foreach (Member member in members)
                    {
                        string memberKey = member.Email ?? member.Id;

                        Trace.WriteLine($"Queuing batch member role change for {memberKey} as {member.Role} to group {id}");
                        requests.Add(poolService.Item.Members.Patch(member, id, memberKey));
                    }

                    this.ProcessBatches(id, members, false, false, requests, poolService);
                }
            }
            finally
            {
                this.ReleaseGate();
            }
        }

        private bool IsExistingMemberError(HttpStatusCode statusCode, string message)
        {
            if (statusCode == HttpStatusCode.Conflict && message.IndexOf("member already exists", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private bool IsMissingMemberError(HttpStatusCode statusCode, string message)
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