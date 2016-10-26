using System;
using System.Threading;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Groupssettings.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.GData.Contacts;

namespace Lithnet.GoogleApps
{
    public static class ConnectionPools
    {
        public static bool DisableGzip { get; set; }

        private static BaseClientServicePool<DirectoryService> directoryServicePool;

        private static BaseClientServicePool<GroupssettingsService> groupSettingServicePool;

        private static Pool<EmailSettingsService> userSettingsServicePool;

        private static Pool<ContactsService> contactsServicePool;

        public static BaseClientServicePool<DirectoryService> DirectoryServicePool => ConnectionPools.directoryServicePool;

        public static BaseClientServicePool<GroupssettingsService> GroupSettingServicePool => ConnectionPools.groupSettingServicePool;

        public static Pool<EmailSettingsService> UserSettingsServicePool => ConnectionPools.userSettingsServicePool;

        public static Pool<ContactsService> ContactsServicePool => ConnectionPools.contactsServicePool;

        public static void SetRateLimitDirectoryService(int requestsPerInterval, TimeSpan interval)
        {
            DirectoryService service = new DirectoryService();
            RateLimiter.SetRateLimit(service.Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitGroupSettingsService(int requestsPerInterval, TimeSpan interval)
        {
            GroupssettingsService service = new GroupssettingsService();
            RateLimiter.SetRateLimit(service.Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitEmailSettingsService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(typeof(EmailSettingsService).Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitContactsService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(typeof(ContactsService).Name, requestsPerInterval, interval);
        }

        public static void SetConcurrentOperationLimitGroupMember(int maxConcurrentOperations)
        {
            RateLimiter.SetConcurrentLimit(GroupMemberRequestFactory.ServiceName, maxConcurrentOperations);
        }

        public static void InitializePools(ServiceAccountCredential credentials, int directoryServicePoolSize, int groupSettingServicePoolSize, int userSettingsPoolSize, int contactsPoolSize)
        {
            ConnectionPools.PopulateDirectoryServicePool(credentials, directoryServicePoolSize);
            ConnectionPools.PopulateGroupSettingServicePool(credentials, groupSettingServicePoolSize);
            GroupRequestFactory.SettingsThreads = groupSettingServicePoolSize;
            
            ConnectionPools.PopulateUserSettingsServicePool(credentials, userSettingsPoolSize);
            ConnectionPools.PopulateContactsServicePool(credentials, contactsPoolSize);
        }

        private static void PopulateUserSettingsServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.userSettingsServicePool = new Pool<EmailSettingsService>(size, () =>
            {
                credentials.RequestAccessTokenAsync(System.Threading.CancellationToken.None).Wait();
                EmailSettingsService service = new EmailSettingsService("Lithnet.GoogleApps");
                OAuthGDataRequestFactory requestFactory = new OAuthGDataRequestFactory("Lithnet.GoogleApps", credentials);
                requestFactory.UseGZip = !ConnectionPools.DisableGzip;
                service.RequestFactory = requestFactory;
                return service;
            });
        }

        private static void PopulateContactsServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.contactsServicePool = new Pool<ContactsService>(size, () =>
            {
                credentials.RequestAccessTokenAsync(System.Threading.CancellationToken.None).Wait();
                ContactsService service = new ContactsService("Lithnet.GoogleApps");
                OAuthGDataRequestFactory requestFactory = new OAuthGDataRequestFactory("Lithnet.GoogleApps", credentials);
                requestFactory.CustomHeaders.Add("GData-Version: 3.0");
                requestFactory.UseGZip = !ConnectionPools.DisableGzip;
                service.RequestFactory = requestFactory;
                return service;
            });
        }

        private static void PopulateDirectoryServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.directoryServicePool = new BaseClientServicePool<DirectoryService>(size, () =>
                {
                    DirectoryService x = new DirectoryService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = !ConnectionPools.DisableGzip,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                    });

                    x.HttpClient.Timeout = Timeout.InfiniteTimeSpan;
                    return x;
                });

        }

        private static void PopulateGroupSettingServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.groupSettingServicePool = new BaseClientServicePool<GroupssettingsService>(size, () =>
                {
                    GroupssettingsService x = new GroupssettingsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = !ConnectionPools.DisableGzip,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None

                    });

                    x.HttpClient.Timeout = Timeout.InfiniteTimeSpan;
                    return x;
                });
        }
    }
}
