using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Groupssettings.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    using Google.GData.Apps.GoogleMailSettings;
    using Google.GData.Client;
    using Google.GData.Contacts;

    public static class ConnectionPools
    {
        private static BaseClientServicePool<DirectoryService> directoryServicePool;

        private static BaseClientServicePool<GroupssettingsService> groupSettingServicePool;

        private static GDataServicePool<GoogleMailSettingsService> userSettingsServicePool;

        private static GDataServicePool<ContactsService> contactsServicePool;

        public static BaseClientServicePool<DirectoryService> DirectoryServicePool => ConnectionPools.directoryServicePool;

        public static BaseClientServicePool<GroupssettingsService> GroupSettingServicePool => ConnectionPools.groupSettingServicePool;

        public static GDataServicePool<GoogleMailSettingsService> UserSettingsServicePool => ConnectionPools.userSettingsServicePool;
        public static GDataServicePool<ContactsService> ContactsServicePool => ConnectionPools.contactsServicePool;


        public static void InitializePools(ServiceAccountCredential credentials, int directoryServicePoolSize, int groupSettingServicePoolSize)
        {
            ConnectionPools.PopulateDirectoryServicePool(credentials, directoryServicePoolSize);
            ConnectionPools.PopulateGroupSettingServicePool(credentials, groupSettingServicePoolSize);
            ConnectionPools.PopulateUserSettingsServicePool(credentials, directoryServicePoolSize);
            ConnectionPools.PopulateContactsServicePool(credentials, directoryServicePoolSize);
        }

        private static void PopulateUserSettingsServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.userSettingsServicePool = new GDataServicePool<GoogleMailSettingsService>(size, (domain) =>
            {
                credentials.RequestAccessTokenAsync(System.Threading.CancellationToken.None).Wait();
                GoogleMailSettingsService service = new GoogleMailSettingsService(domain, "Lithnet.GoogleApps");
                GDataRequestFactory requestFactory = new GDataRequestFactory("Lithnet.GoogleApps");
                requestFactory.CustomHeaders.Add($"Authorization: Bearer {credentials.Token.AccessToken}");
                service.RequestFactory = requestFactory;

                return service;
            });
        }

        private static void PopulateContactsServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.contactsServicePool = new GDataServicePool<ContactsService>(size, (i) =>
            {
                credentials.RequestAccessTokenAsync(System.Threading.CancellationToken.None).Wait();
                ContactsService service = new ContactsService("Lithnet.GoogleApps");
                GDataRequestFactory requestFactory = new GDataRequestFactory("Lithnet.GoogleApps");
                requestFactory.CustomHeaders.Add($"Authorization: Bearer {credentials.Token.AccessToken}");
                service.RequestFactory = requestFactory;

                return service;
            });
        }

        private static void PopulateDirectoryServicePool(ServiceAccountCredential credentials, int size)
        {
            //Logger.WriteLine("Populating directory service pool with {0} items", size);

            ConnectionPools.directoryServicePool = new BaseClientServicePool<DirectoryService>(size, (i) =>
                {
                    return new DirectoryService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = true,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None
                    });
                });
        }

        private static void PopulateGroupSettingServicePool(ServiceAccountCredential credentials, int size)
        {
            //Logger.WriteLine("Populating group setting service pool with {0} items", size);
            ConnectionPools.groupSettingServicePool = new BaseClientServicePool<GroupssettingsService>(size, (i) =>
                {
                    return new GroupssettingsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = true,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None

                    });
                });
        }
    }
}
