using System;
using System.Threading;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Groupssettings.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Http;
using Google.GData.Contacts;

namespace Lithnet.GoogleApps
{
    public static class ConnectionPools
    {
        public static bool DisableGzip { get; set; }

        private static BaseClientServicePool<DirectoryService> directoryServicePool;

        private static BaseClientServicePool<GroupssettingsService> groupSettingServicePool;

        private static BaseClientServicePool<CalendarService> calendarServicePool;
        
        private static Pool<GmailService> gmailServicePool;
        
        private static Pool<ContactsService> contactsServicePool;

        public static BaseClientServicePool<DirectoryService> DirectoryServicePool => ConnectionPools.directoryServicePool;

        public static BaseClientServicePool<GroupssettingsService> GroupSettingServicePool => ConnectionPools.groupSettingServicePool;

        public static Pool<GmailService> GmailServicePool => ConnectionPools.gmailServicePool;
        
        public static Pool<ContactsService> ContactsServicePool => ConnectionPools.contactsServicePool;

        public static BaseClientServicePool<CalendarService> CalendarServicePool => ConnectionPools.calendarServicePool;

        public static void SetRateLimitDirectoryService(int requestsPerInterval, TimeSpan interval)
        {
            DirectoryService service = new DirectoryService();
            RateLimiter.SetRateLimit(service.Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitCalendarService(int requestsPerInterval, TimeSpan interval)
        {
            CalendarService service = new CalendarService();
            RateLimiter.SetRateLimit(service.Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitGroupSettingsService(int requestsPerInterval, TimeSpan interval)
        {
            GroupssettingsService service = new GroupssettingsService();
            RateLimiter.SetRateLimit(service.Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitContactsService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(typeof(ContactsService).Name, requestsPerInterval, interval);
        }
        
        public static void SetRateLimitGmailService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(typeof(GmailService).Name, requestsPerInterval, interval);
        }

        public static void SetConcurrentOperationLimitGroupMember(int maxConcurrentOperations)
        {
            RateLimiter.SetConcurrentLimit(GroupMemberRequestFactory.ServiceName, maxConcurrentOperations);
        }

        public static void InitializePools(ServiceAccountCredential credentials, int directoryServicePoolSize, int groupSettingServicePoolSize, int userSettingsPoolSize, int contactsPoolSize, int calendarPoolSize)
        {
            ConnectionPools.PopulateDirectoryServicePool(credentials, directoryServicePoolSize);
            ConnectionPools.PopulateGroupSettingServicePool(credentials, groupSettingServicePoolSize);
            GroupRequestFactory.SettingsThreads = groupSettingServicePoolSize;
            
            ConnectionPools.PopulateGmailServicePool(credentials, userSettingsPoolSize);
            ConnectionPools.PopulateContactsServicePool(credentials, contactsPoolSize);
            ConnectionPools.PopulateCalendarServicePool(credentials, calendarPoolSize);
        }

        private static void PopulateCalendarServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.calendarServicePool = new BaseClientServicePool<CalendarService>(size, () =>
            {
                CalendarService x = new CalendarService(
                    new BaseClientService.Initializer()
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

        private static void PopulateGmailServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.gmailServicePool = new BaseClientServicePool<GmailService>(size, () =>
            {
                GmailService x = new GmailService(new BaseClientService.Initializer()
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
