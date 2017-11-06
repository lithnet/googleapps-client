using System;
using Google.Apis.Groupssettings.v1;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;
using Lithnet.GoogleApps.Api;
using System.Threading;

namespace Lithnet.GoogleApps
{
    public static class GroupSettingsRequestFactory
    {
        public static GroupSettings Get(string mail)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 1)
            {
                throw new ArgumentException("The group key must be the group email address");
            }

            using (PoolItem<GroupssettingsService> connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Ignore))
            {
                GroupSettingsGetRequest request = new GroupSettingsGetRequest(connection.Item, mail);

                try
                {
                    return request.ExecuteWithBackoff();
                }
                catch (Google.GoogleApiException e)
                {
                    // 2016-10-29 Groupssettings is returning 400 randomly for some group settings. Subsequent calls seem to work
                    if (e.HttpStatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        Thread.Sleep(1000);
                        return request.ExecuteWithBackoff();
                    }

                    // 2017-11-07 Groupssettings is returning 404 randomly for some group settings. Subsequent calls seem to work
                    if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Thread.Sleep(1000);
                        return request.ExecuteWithBackoff();
                    }

                    throw;
                }
            }
        }

        public static GroupSettings Update(string mail, GroupSettings item)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 1)
            {
                throw new ArgumentException("The group key must be the group email address");
            }

            using (PoolItem<GroupssettingsService> connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Include))
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

            using (PoolItem<GroupssettingsService> connection = ConnectionPools.GroupSettingServicePool.Take(NullValueHandling.Ignore))
            {
                GroupSettingsPatchRequest request = new GroupSettingsPatchRequest(connection.Item, item, mail);
                return request.ExecuteWithBackoff();
            }
        }
    }
}
