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
    public static class ConnectionPools
    {
        private static BaseClientServicePool<DirectoryService> directoryServicePool;

        private static BaseClientServicePool<GroupssettingsService> groupSettingServicePool;
       
        public static BaseClientServicePool<DirectoryService> DirectoryServicePool
        {
            get
            {
                return ConnectionPools.directoryServicePool;
            }
        }

        public static BaseClientServicePool<GroupssettingsService> GroupSettingServicePool
        {
            get
            {
                return ConnectionPools.groupSettingServicePool;
            }
        }

        public static void InitializePools(ServiceAccountCredential credentials, int directoryServicePoolSize, int groupSettingServicePoolSize)
        {
            ConnectionPools.PopulateDirectoryServicePool(credentials, directoryServicePoolSize);
            ConnectionPools.PopulateGroupSettingServicePool(credentials, groupSettingServicePoolSize);
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
