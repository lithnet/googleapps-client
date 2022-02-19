using System.Threading;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Newtonsoft.Json;

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

        public Domains2 List(string customerID)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainsResource.ListRequest request = new DomainsResource.ListRequest(connection.Item, customerID);

                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public Domains Get(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainsResource.GetRequest request = new DomainsResource.GetRequest(connection.Item, customerID, domain);

                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public void Delete(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainsResource.DeleteRequest request = new DomainsResource.DeleteRequest(connection.Item, customerID, domain);
                request.ExecuteWithRetryOnBackoff();
            }
        }

        public Domains Insert(string customerID, Domains domain)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainsResource.InsertRequest request = new DomainsResource.InsertRequest(connection.Item, domain, customerID);
                return request.ExecuteWithRetryOnBackoff();
            }
        }
    }
}