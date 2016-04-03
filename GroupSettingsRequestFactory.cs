using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using GS = Google.Apis.Groupssettings.v1;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;
using Lithnet.GoogleApps.Api;

namespace Lithnet.GoogleApps
{
    public static class GroupSettingsRequestFactory
    {
        private static BlockingCollection<GoogleGroup> queue = new BlockingCollection<GoogleGroup>();

        private static Stopwatch totalTimer = new Stopwatch();

        public static TimeSpan CumlativeTime { get; private set; }

        public static TimeSpan Elapsed
        {
            get
            {
                return GroupSettingsRequestFactory.totalTimer.Elapsed;
            }
        }

        public static void AddJob(GoogleGroup group)
        {
            GroupSettingsRequestFactory.queue.Add(group);
        }

        public static void CompleteAdding()
        {
            GroupSettingsRequestFactory.queue.CompleteAdding();
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
                    Stopwatch timer = new Stopwatch();

                    timer.Start();
                    myGroup.Settings = GroupSettingsRequestFactory.Get(myGroup.Group.Email);
                    timer.Stop();

                    lock (totalTimer)
                    {
                        CumlativeTime = CumlativeTime.Add(timer.Elapsed);
                    }

                    //Logger.WriteLine("{1}:{2:D3}:settings:{0}", myGroup.Group.Email, timer.Elapsed, ConnectionPools.GroupSettingServicePool.AvailableCount);
                }
                catch (AggregateException ex)
                {
                   // Logger.WriteException(ex);
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

        public static GroupSettings Get(string groupKey)
        {
            using (var connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    GroupSettingsGetRequest request = new GroupSettingsGetRequest(connection.Client, groupKey);
//#if DEBUG
//                    Logger.WriteLine("GET GROUP SETTINGS request: {0}", groupKey);
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("GET GROUP SETTINGS response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("GET GROUP SETTINGS {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static GroupSettings Update(string groupKey, GroupSettings item)
        {
            using (var connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Include))
            {
                try
                {
                    GroupSettingsUpdateRequest request = new GroupSettingsUpdateRequest(connection.Client, item, groupKey);
//#if DEBUG
//                    Logger.WriteLine("UPDATE GROUP SETTINGS request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("UPDATE GROUP SETTINGS response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("UPDATE GROUP SETTINGS {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static GroupSettings Patch(string groupKey, GroupSettings item)
        {
            using (var connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Ignore))
            {
                try
                {
                    GroupSettingsPatchRequest request = new GroupSettingsPatchRequest(connection.Client, item, groupKey);
//#if DEBUG
//                    Logger.WriteLine("PATCH GROUP SETTINGS request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    var result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("PATCH GROUP SETTINGS response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("PATCH GROUP SETTINGS {0} threw an exception", groupKey);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }
    }
}
