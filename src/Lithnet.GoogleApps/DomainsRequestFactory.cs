using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using G=Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Concurrent;
using System.Collections;
using Newtonsoft.Json;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps
{
    using Google.Apis.Admin.Directory.directory_v1;

    public static class DomainsRequestFactory
    {
        public static DomainList GetDomains(string customerID)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainListRequest request = new DomainListRequest(connection.Item, customerID);

                return request.ExecuteWithBackoff();
            }
        }

        public static Domain GetDomain(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainGetRequest request = new DomainGetRequest(connection.Item, domain, customerID);

                return request.ExecuteWithBackoff();
            }
        }

        public static void DeleteDomain(string customerID, string domain)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainDeleteRequest request = new DomainDeleteRequest(connection.Item, domain, customerID);
                request.ExecuteWithBackoff();
            }
        }

        public static void InsertDomain(string customerID, Domain domain)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                DomainInsertRequest request = new DomainInsertRequest(connection.Item, customerID, domain);
                request.ExecuteWithBackoff();
            }
        }
    }
}