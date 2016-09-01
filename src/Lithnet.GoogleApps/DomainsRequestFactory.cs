using Newtonsoft.Json;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Google.Apis.Admin.Directory.directory_v1;

namespace Lithnet.GoogleApps
{
    public static class DomainsRequestFactory
    {
        public static DomainList List(string customerID)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainListRequest request = new DomainListRequest(connection.Item, customerID);

                return request.ExecuteWithBackoff();
            }
        }

        public static Domain Get(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainGetRequest request = new DomainGetRequest(connection.Item, domain, customerID);

                return request.ExecuteWithBackoff();
            }
        }

        public static void Delete(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainDeleteRequest request = new DomainDeleteRequest(connection.Item, domain, customerID);
                request.ExecuteWithBackoff();
            }
        }

        public static Domain Insert(string customerID, Domain domain)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainInsertRequest request = new DomainInsertRequest(connection.Item, customerID, domain);
                return request.ExecuteWithBackoff();
            }
        }
    }
}