using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    public static class SchemaRequestFactory
    {
        public static void CreateSchema(string customerID, Schema schema)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.InsertRequest schemaReq = connection.Item.Schemas.Insert(schema, customerID);
                schemaReq.ExecuteWithBackoff();
            }
        }

        public static void DeleteSchema(string customerID, string schemaKey)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.DeleteRequest schemaReq = connection.Item.Schemas.Delete(customerID, schemaKey);
                schemaReq.ExecuteWithBackoff();
            }
        }

        public static void UpdateSchema(string customerID, Schema schema)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                SchemasResource.UpdateRequest schemaReq = connection.Item.Schemas.Update(schema, customerID, schema.SchemaName);
                schemaReq.ExecuteWithBackoff();
            }
        }

        public static bool HasSchema(string customerID, string schemaName)
        {
            try
            {
                using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
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

        public static Schema GetSchema(string customerID, string schemaName)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.GetRequest schemaReq = connection.Item.Schemas.Get(customerID, schemaName);
                return schemaReq.ExecuteWithBackoff();
            }
        }

        public static Schemas ListSchemas(string customerID)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.ListRequest schemaReq = connection.Item.Schemas.List(customerID);
                return schemaReq.ExecuteWithBackoff();
            }
        }

        public static bool HasAccessToSchema(string customerID)
        {
            try
            {
                Schemas schemas = SchemaRequestFactory.ListSchemas(customerID);
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
