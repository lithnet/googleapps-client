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
    public static class DomainsRequestFactory
    {
        public static DomainList GetDomains(string customerID)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new DomainListRequest(connection.Client, customerID);

                return request.ExecuteWithBackoff();
            }
        }
    }
}