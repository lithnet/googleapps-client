using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Lithnet.GoogleApps
{
    public class CustomerRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public CustomerRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
        {
            this.directoryServicePool = new BaseClientServicePool<DirectoryService>(poolSize, () =>
            {
                DirectoryService x = new DirectoryService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new ServiceAccountCredential(creds.GetInitializer(scopes)),
                    ApplicationName = "LithnetGoogleAppsLibrary",
                    GZipEnabled = !Settings.DisableGzip,
                    Serializer = new GoogleJsonSerializer(),
                    DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                });
                
                x.HttpClient.Timeout = Timeout.InfiniteTimeSpan;
                return x;
            });
        }

        public Customer Get(string customerID)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                CustomersResource.GetRequest request = new CustomersResource.GetRequest(connection.Item, customerID);

                return request.ExecuteWithRetryOnBackoff();
            }
        }
    }
}