using System;
using Google.Apis.Groupssettings.v1;
using Newtonsoft.Json;
using Lithnet.GoogleApps.ManagedObjects;
using Lithnet.GoogleApps.Api;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Lithnet.GoogleApps
{
    public class GroupSettingsRequestFactory
    {
        private readonly BaseClientServicePool<GroupssettingsService> groupSettingsServicePool;

        internal GroupSettingsRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
        {
            this.groupSettingsServicePool = new BaseClientServicePool<GroupssettingsService>(poolSize, () =>
            {
                GroupssettingsService x = new GroupssettingsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new ServiceAccountCredential(creds.GetInitializer(scopes)),
                    ApplicationName = "LithnetGoogleAppsLibrary",
                    GZipEnabled = !Settings.DisableGzip,
                    Serializer = new GoogleJsonSerializer(),
                    DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None
                });

                x.HttpClient.Timeout = Timeout.InfiniteTimeSpan;
                return x;
            });
        }

        public GroupSettings Get(string mail)
        {
            mail.ThrowIfNotEmailAddress();

            using (PoolItem<GroupssettingsService> connection = this.groupSettingsServicePool.Take(NullValueHandling.Ignore))
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

        public GroupSettings Update(string mail, GroupSettings item)
        {
            mail.ThrowIfNotEmailAddress();

            using (PoolItem<GroupssettingsService> connection = this.groupSettingsServicePool.Take(NullValueHandling.Include))
            {
                GroupSettingsUpdateRequest request = new GroupSettingsUpdateRequest(connection.Item, item, mail);
                return request.ExecuteWithBackoff();
            }
        }

        public GroupSettings Patch(string mail, GroupSettings item)
        {
            mail.ThrowIfNotEmailAddress();

            using (PoolItem<GroupssettingsService> connection = this.groupSettingsServicePool.Take(NullValueHandling.Ignore))
            {
                GroupSettingsPatchRequest request = new GroupSettingsPatchRequest(connection.Item, item, mail);
                return request.ExecuteWithBackoff();
            }
        }
    }
}
