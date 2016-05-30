using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps
{
    using Google;

    public static class GroupMemberRequestFactory
    {
        private static BlockingCollection<GoogleGroup> queue = new BlockingCollection<GoogleGroup>();

        public static void AddJob(GoogleGroup group)
        {
            GroupMemberRequestFactory.queue.Add(group);
        }

        public static void CompleteAdding()
        {
            GroupMemberRequestFactory.queue.CompleteAdding();
        }

        public static void ConsumeQueue(int threads)
        {
            ParallelOptions op = new ParallelOptions {MaxDegreeOfParallelism = threads};

            Parallel.ForEach(queue.GetConsumingEnumerable(), op, (myGroup) =>
            {
                try
                {
                    myGroup.Membership = GetMembership(myGroup.Group.Email);
                }
                catch (AggregateException ex)
                {
                    myGroup.Errors.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    myGroup.Errors.Add(ex);
                }
            });
        }

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
                Member members = request.ExecuteWithBackoff();
            }
        }

        public static void RemoveMember(string groupID, string memberID)
        {
            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MembersResource.DeleteRequest request = poolService.Item.Members.Delete(groupID, memberID);

                string members = request.ExecuteWithBackoff();
            }
        }

        public static void AddMembers(string id, IList<Member> members, bool throwOnExistingMember)
        {
            using (PoolItem<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                Queue<MembersResource.InsertRequest> requests = new Queue<MembersResource.InsertRequest>();

                foreach (Member member in members)
                {
                    requests.Enqueue(poolService.Item.Members.Insert(member, id));
                }

                Google.Apis.Requests.BatchRequest batchRequest = new Google.Apis.Requests.BatchRequest(poolService.Item);
                List<string> failedMembers = new List<string>();
                List<Exception> failures = new List<Exception>();

                foreach (MembersResource.InsertRequest request in requests)
                {
                    batchRequest.Queue<MembersResource>(request,
                          (content, error, i, message) =>
                          {
                              Member itemKey = members[i];

                              if (error == null)
                              {
                                  //Logger.WriteLine("Member '{1}' added to group {0}", id, itemKey.Email);
                                  return;
                              }

                              string errorString = string.Format(
                                  "{0}\nFailed member add: {1}\nGroup: {2}",
                                  error.ToString(),
                                  itemKey == null ? string.Empty : itemKey.Email,
                                  id);

                              if (!throwOnExistingMember)
                              {
                                  if (message.StatusCode == System.Net.HttpStatusCode.Conflict && errorString.ToLower().Contains("member already exists"))
                                  {
                                     // Logger.WriteLine("Warning: Member '{1}' already exists in group {0}", id, itemKey.Email);
                                      return;
                                  }
                              }

                              GoogleApiException ex = new Google.GoogleApiException(poolService.Item.Name, errorString);
                              ex.HttpStatusCode = message.StatusCode;

                              failedMembers.Add(itemKey.Email);
                              failures.Add(ex);
                          });
                }

                batchRequest.ExecuteAsync().Wait();

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

                foreach (string member in members)
                {
                    //Logger.WriteLine("Queuing request to delete member {0}", member);
                    requests.Add(poolService.Item.Members.Delete(id, member));
                }

                Google.Apis.Requests.BatchRequest batchRequest = new Google.Apis.Requests.BatchRequest(poolService.Item);

                List<string> failedMembers = new List<string>();
                List<Exception> failures = new List<Exception>();

                foreach (MembersResource.DeleteRequest request in requests)
                {
                    batchRequest.Queue<string>(request,
                          (content, error, i, message) =>
                          {
                              string itemKey = members[i];

                              if (error == null)
                              {
                                  //Logger.WriteLine("Member '{1}' deleted from group {0}", id, itemKey);
                                  return;
                              }

                              string errorString = string.Format("{0}\nFailed member delete: {1}\nGroup: {2}", error.ToString(), itemKey, id);

                              if (!throwOnMissingMember)
                              {
                                  if (message.StatusCode == System.Net.HttpStatusCode.NotFound && errorString.Contains("Resource Not Found: memberKey"))
                                  {
                                      //Logger.WriteLine("Warning: request to delete non-existent member '{1}' from group {0}", id, itemKey);
                                      return;
                                  }
                              }

                              GoogleApiException ex = new Google.GoogleApiException(poolService.Item.Name, errorString);
                              ex.HttpStatusCode = message.StatusCode;
                              failedMembers.Add(itemKey);
                              failures.Add(ex);
                          });
                }

                batchRequest.ExecuteAsync().Wait();

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
