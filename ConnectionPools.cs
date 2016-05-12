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
        public static bool DisableGzip { get; set; }

        private static BaseClientServicePool<DirectoryService> directoryServicePool;

        private static BaseClientServicePool<GroupssettingsService> groupSettingServicePool;

        private static Pool<EmailSettingsService> userSettingsServicePool;

        private static Pool<ContactsService> contactsServicePool;

        public static BaseClientServicePool<DirectoryService> DirectoryServicePool => ConnectionPools.directoryServicePool;

        public static BaseClientServicePool<GroupssettingsService> GroupSettingServicePool => ConnectionPools.groupSettingServicePool;

        public static Pool<EmailSettingsService> UserSettingsServicePool => ConnectionPools.userSettingsServicePool;

        public static Pool<ContactsService> ContactsServicePool => ConnectionPools.contactsServicePool;


        public static void InitializePools(ServiceAccountCredential credentials, int directoryServicePoolSize, int groupSettingServicePoolSize, int userSettingsPoolSize, int contactsPoolSize)
        {
            ConnectionPools.PopulateDirectoryServicePool(credentials, directoryServicePoolSize);
            ConnectionPools.PopulateGroupSettingServicePool(credentials, groupSettingServicePoolSize);
            ConnectionPools.PopulateUserSettingsServicePool(credentials, userSettingsPoolSize);
            ConnectionPools.PopulateContactsServicePool(credentials, contactsPoolSize);
        }

        private static void PopulateUserSettingsServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.userSettingsServicePool = new Pool<EmailSettingsService>(size, () =>
            {
                credentials.RequestAccessTokenAsync(System.Threading.CancellationToken.None).Wait();
                EmailSettingsService service = new EmailSettingsService("Lithnet.GoogleApps");
                GDataRequestFactory requestFactory = new GDataRequestFactory("Lithnet.GoogleApps");
                requestFactory.CustomHeaders.Add($"Authorization: Bearer {credentials.Token.AccessToken}");
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
                GDataRequestFactory requestFactory = new GDataRequestFactory("Lithnet.GoogleApps");
                requestFactory.CustomHeaders.Add($"Authorization: Bearer {credentials.Token.AccessToken}");
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
                    return new DirectoryService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = !ConnectionPools.DisableGzip,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None
                    });
                });
        }

        private static void PopulateGroupSettingServicePool(ServiceAccountCredential credentials, int size)
        {
            ConnectionPools.groupSettingServicePool = new BaseClientServicePool<GroupssettingsService>(size, () =>
                {
                    return new GroupssettingsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credentials,
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = !ConnectionPools.DisableGzip,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None

                    });
                });
        }
    }
}
