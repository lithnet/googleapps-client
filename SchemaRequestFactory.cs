using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    public static class SchemaRequestFactory
    {
        static SchemaRequestFactory()
        {
        }

        public static void CreateSchema(string customerID, Schema schema)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var schemaReq = connection.Client.Schemas.Insert(schema, customerID);
                schemaReq.Execute();
            }
        }

        public static void DeleteSchema(string customerID, string schemaKey)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var schemaReq = connection.Client.Schemas.Delete(customerID, schemaKey);
                schemaReq.Execute();
            }
        }

        public static void UpdateSchema(string customerID, Schema schema)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                var schemaReq = connection.Client.Schemas.Update(schema, customerID, schema.SchemaName);
                schemaReq.Execute();
            }
        }

        public static bool HasSchema(string customerID, string schemaName)
        {
            try
            {
                using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    var schemaReq = connection.Client.Schemas.Get(customerID, schemaName);
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

        public static bool HasAccessToSchema(string customerID)
        {
            try
            {
                using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
                {
                    var schemaReq = connection.Client.Schemas.List(customerID);
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
