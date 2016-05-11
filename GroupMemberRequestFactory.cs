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

        private static Stopwatch totalTimer = new Stopwatch();

        public static TimeSpan CumlativeTime { get; private set; }

        public static TimeSpan Elapsed
        {
            get
            {
                return GroupMemberRequestFactory.totalTimer.Elapsed;
            }
        }

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
            ParallelOptions op = new ParallelOptions();
            op.MaxDegreeOfParallelism = threads;

            totalTimer.Start();

            Parallel.ForEach(queue.GetConsumingEnumerable(), op, (myGroup) =>
            {
                try
                {
                    myGroup.Membership = GetMembership(myGroup.Group.Email);
                }
                catch (AggregateException ex)
                {
                    //Logger.WriteException(ex);
                    myGroup.Errors.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    //Logger.WriteException(ex);
                    myGroup.Errors.Add(ex.InnerException);
                }
            });

            totalTimer.Stop();
        }

        public static GroupMembership GetMembership(string groupKey)
        {
            Stopwatch timer = new Stopwatch();
            GroupMembership membership = new GroupMembership();

            using (BaseClientServiceWrapper<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                MembersResource.ListRequest request = poolService.Client.Members.List(groupKey);
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

            lock (totalTimer)
            {
                CumlativeTime = CumlativeTime.Add(timer.Elapsed);
            }

            //Logger.WriteLine("{2}:{3:D3}:members:{1}:{0}", membership.Members.Count, groupKey, timer.Elapsed, ConnectionPools.DirectoryServicePool.AvailableCount);

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
            using (BaseClientServiceWrapper<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MembersResource.InsertRequest request = poolService.Client.Members.Insert(item, groupID);
//#if DEBUG
//                Logger.WriteLine("ADD MEMBER request");
//                Logger.WriteSeparatorLine('-');
//                Logger.WriteLine(poolService.Client.Serializer.Serialize(item));
//                Logger.WriteSeparatorLine('-');
//#endif

                Member members = request.ExecuteWithBackoff();
                //Logger.WriteLine("Added member {0} from {1}", item.Email ?? item.Id, groupID);
            }
        }

        public static void RemoveMember(string groupID, string memberID)
        {
            using (BaseClientServiceWrapper<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MembersResource.DeleteRequest request = poolService.Client.Members.Delete(groupID, memberID);

                string members = request.ExecuteWithBackoff();
                //Logger.WriteLine("Deleted member {0} from {1}", memberID, groupID);
            }
        }

        public static void AddMembers(string id, IList<Member> members, bool throwOnExistingMember)
        {
            using (BaseClientServiceWrapper<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                Queue<MembersResource.InsertRequest> requests = new Queue<MembersResource.InsertRequest>();

                foreach (Member member in members)
                {
                    //Logger.WriteLine("Queuing request to add member {0}", member.Email ?? member.Id);
                    requests.Enqueue(poolService.Client.Members.Insert(member, id));
                }

                Google.Apis.Requests.BatchRequest batchRequest = new Google.Apis.Requests.BatchRequest(poolService.Client);
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

                              GoogleApiException ex = new Google.GoogleApiException(poolService.Client.Name, errorString);
                              ex.HttpStatusCode = message.StatusCode;

                              failedMembers.Add(itemKey.Email);
                              failures.Add(ex);
                             // Logger.WriteException(ex);
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
            using (BaseClientServiceWrapper<DirectoryService> poolService = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                List<MembersResource.DeleteRequest> requests = new List<MembersResource.DeleteRequest>();

                foreach (string member in members)
                {
                    //Logger.WriteLine("Queuing request to delete member {0}", member);
                    requests.Add(poolService.Client.Members.Delete(id, member));
                }

                Google.Apis.Requests.BatchRequest batchRequest = new Google.Apis.Requests.BatchRequest(poolService.Client);

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

                              GoogleApiException ex = new Google.GoogleApiException(poolService.Client.Name, errorString);
                              ex.HttpStatusCode = message.StatusCode;
                              //Logger.WriteException(ex);
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
