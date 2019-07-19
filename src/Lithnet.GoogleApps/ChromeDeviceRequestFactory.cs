using System;
using System.Collections.Generic;
using System.Linq;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Newtonsoft.Json;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Lithnet.GoogleApps
{
    public class ChromeDeviceRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public ChromeDeviceRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
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

                x.HttpClient.Timeout = Settings.DefaultTimeout;
                return x;
            });
        }

        public IEnumerable<ChromeOsDevice> GetChromeDevices(string customerID)
        {
            return this.GetChromeDevices(customerID, null);
        }

        public IEnumerable<ChromeOsDevice> GetChromeDevices(string customerID, string fields)
        {
            return this.GetChromeDevices(customerID, fields, null);
        }

        public IEnumerable<ChromeOsDevice> GetChromeDevices(string customerID, string fields, string query)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;

                ChromeosdevicesResource.ListRequest request = new ChromeosdevicesResource.ListRequest(connection.Item, customerID)
                {
                    MaxResults = 500
                };

                if (fields != null)
                {
                    request.Projection = ChromeosdevicesResource.ListRequest.ProjectionEnum.FULL;
                    request.Fields = fields;
                }

                request.Query = query;
                request.PrettyPrint = false;

                do
                {
                    request.PageToken = token;

                    ChromeOsDevices pageResults = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout);

                    if (pageResults.Chromeosdevices == null)
                    {
                        break;
                    }

                    foreach (var item in pageResults.Chromeosdevices)
                    {
                        yield return item;
                    }

                    token = pageResults.NextPageToken;

                } while (token != null);
            }
        }
    }
}