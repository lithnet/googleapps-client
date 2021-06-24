using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Newtonsoft.Json;
using System.Threading;

namespace Lithnet.GoogleApps
{
    public class OrgUnitsRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public OrgUnitsRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
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

        public OrgUnits List(string customerID)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new OrgunitsResource.ListRequest(connection.Item, customerID);
                request.Type = OrgunitsResource.ListRequest.TypeEnum.All;
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public OrgUnit Get(string customerID, string orgUnit)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new OrgunitsResource.GetRequest(connection.Item, customerID, orgUnit);

                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public void Delete(string customerID, string orgUnit)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new OrgunitsResource.DeleteRequest(connection.Item, customerID, orgUnit);

                request.ExecuteWithRetryOnBackoff();
            }
        }

        public OrgUnit Insert(string customerID, OrgUnit orgUnit)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new OrgunitsResource.InsertRequest(connection.Item, orgUnit, customerID);
                return request.ExecuteWithRetryOnBackoff();
            }
        }


        public OrgUnit Patch(OrgUnit item, string customerId, string orgUnitId)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new OrgunitsResource.PatchRequest(connection.Item, item, customerId, orgUnitId);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public OrgUnit Update(OrgUnit item, string customerId, string orgUnitId)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Include))
            {
                var request = new OrgunitsResource.UpdateRequest(connection.Item, item, customerId, orgUnitId);
                return request.ExecuteWithRetryOnBackoff();
            }
        }
    }
}