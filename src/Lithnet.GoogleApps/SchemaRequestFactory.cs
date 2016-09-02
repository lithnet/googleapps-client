using Google.Apis.Admin.Directory.directory_v1.Data;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    using Google.Apis.Admin.Directory.directory_v1;

    public static class SchemaRequestFactory
    {
        public static void CreateSchema(string customerID, Schema schema)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.InsertRequest schemaReq = connection.Item.Schemas.Insert(schema, customerID);
                schemaReq.Execute();
            }
        }

        public static void DeleteSchema(string customerID, string schemaKey)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                SchemasResource.DeleteRequest schemaReq = connection.Item.Schemas.Delete(customerID, schemaKey);
                schemaReq.Execute();
            }
        }

        public static void UpdateSchema(string customerID, Schema schema)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                SchemasResource.UpdateRequest schemaReq = connection.Item.Schemas.Update(schema, customerID, schema.SchemaName);
                schemaReq.Execute();
            }
        }

        public static bool HasSchema(string customerID, string schemaName)
        {
            try
            {
                using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    SchemasResource.GetRequest schemaReq = connection.Item.Schemas.Get(customerID, schemaName);
                    Schema schema = schemaReq.Execute();
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
                return schemaReq.Execute();
            }
        }

        public static bool HasAccessToSchema(string customerID)
        {
            try
            {
                using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    SchemasResource.ListRequest schemaReq = connection.Item.Schemas.List(customerID);
                    Schemas schemas = schemaReq.Execute();
                    if (schemas != null)
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
