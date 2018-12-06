using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Newtonsoft.Json;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Lithnet.GoogleApps
{
    public class DomainsRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public DomainsRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
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

        public DomainList List(string customerID)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainListRequest request = new DomainListRequest(connection.Item, customerID);

                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public Domain Get(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainGetRequest request = new DomainGetRequest(connection.Item, domain, customerID);

                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public void Delete(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainDeleteRequest request = new DomainDeleteRequest(connection.Item, domain, customerID);
                request.ExecuteWithRetryOnBackoff();
            }
        }

        public Domain Insert(string customerID, Domain domain)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainInsertRequest request = new DomainInsertRequest(connection.Item, customerID, domain);
                return request.ExecuteWithRetryOnBackoff();
            }
        }
    }
}