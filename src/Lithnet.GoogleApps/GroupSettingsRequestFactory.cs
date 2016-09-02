using System;
using Google.Apis.Groupssettings.v1;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;
using Lithnet.GoogleApps.Api;

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
                return request.ExecuteWithBackoff();
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
