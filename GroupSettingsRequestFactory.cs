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
            ParallelOptions op = new ParallelOptions {MaxDegreeOfParallelism = threads};

            Parallel.ForEach(queue.GetConsumingEnumerable(), op, (myGroup) =>
            {
                try
                {
                    myGroup.Settings = GroupSettingsRequestFactory.Get(myGroup.Group.Email);
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

        public static GroupSettings Get(string mail)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 1)
            {
                throw new ArgumentException("The group key must be the group email address");
            }

            using (PoolItem<GS.GroupssettingsService> connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Ignore))
            {
                GroupSettingsGetRequest request = new GroupSettingsGetRequest(connection.Item, mail);
                return request.ExecuteWithBackoff();
            }
        }

        public static GroupSettings Update(string mail, GroupSettings item)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 1)
            {
                throw new ArgumentException("The group key must be the group email address");
            }

            using (PoolItem<GS.GroupssettingsService> connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Include))
            {
                GroupSettingsUpdateRequest request = new GroupSettingsUpdateRequest(connection.Item, item, mail);
                return request.ExecuteWithBackoff();
            }
        }

        public static GroupSettings Patch(string mail, GroupSettings item)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 1)
            {
                throw new ArgumentException("The group key must be the group email address");
            }

            using (PoolItem<GS.GroupssettingsService> connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Ignore))
            {
                GroupSettingsPatchRequest request = new GroupSettingsPatchRequest(connection.Item, item, mail);
                return request.ExecuteWithBackoff();
            }
        }
    }
}
