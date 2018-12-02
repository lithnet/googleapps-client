using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    public class SchemaRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public SchemaRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
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

        public void CreateSchema(string customerID, Schema schema)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.InsertRequest schemaReq = connection.Item.Schemas.Insert(schema, customerID);
                schemaReq.ExecuteWithBackoff();
            }
        }

        public void DeleteSchema(string customerID, string schemaKey)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.DeleteRequest schemaReq = connection.Item.Schemas.Delete(customerID, schemaKey);
                schemaReq.ExecuteWithBackoff();
            }
        }

        public void UpdateSchema(string customerID, Schema schema)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Include))
            {
                SchemasResource.UpdateRequest schemaReq = connection.Item.Schemas.Update(schema, customerID, schema.SchemaName);
                schemaReq.ExecuteWithBackoff();
            }
        }

        public bool HasSchema(string customerID, string schemaName)
        {
            try
            {
                using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
                {
                    SchemasResource.GetRequest schemaReq = connection.Item.Schemas.Get(customerID, schemaName);
                    Schema schema = schemaReq.ExecuteWithBackoff();
                    if (schema != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound || ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public Schema GetSchema(string customerID, string schemaName)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.GetRequest schemaReq = connection.Item.Schemas.Get(customerID, schemaName);
                return schemaReq.ExecuteWithBackoff();
            }
        }

        public Schemas ListSchemas(string customerID)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.ListRequest schemaReq = connection.Item.Schemas.List(customerID);
                return schemaReq.ExecuteWithBackoff();
            }
        }

        public bool HasAccessToSchema(string customerID)
        {
            try
            {
                Schemas schemas = this.ListSchemas(customerID);
                if (schemas != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
