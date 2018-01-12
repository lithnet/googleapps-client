using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Newtonsoft.Json;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Security;

namespace Lithnet.GoogleApps
{
    public static class ResourceRequestFactory
    {
        public static IEnumerable<CalendarResource> GetCalendars(string customerID)
        {
            return ResourceRequestFactory.GetCalendars(customerID, null);
        }

        public static IEnumerable<CalendarResource> GetCalendars(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;

                ResourcesResource.CalendarsResource.ListRequest request = new ResourcesResource.CalendarsResource.ListRequest(connection.Item, customerID);

                if (fields != null)
                {
                    request.MaxResults = 500;
                    request.Fields = fields;
                }

                request.PrettyPrint = false;

                do
                {
                    request.PageToken = token;

                    CalendarResources pageResults = request.ExecuteWithBackoff();

                    if (pageResults.Items == null)
                    {
                        break;
                    }

                    foreach (CalendarResource item in pageResults.Items)
                    {
                        yield return item;
                    }

                    token = pageResults.NextPageToken;

                } while (token != null);
            }
        }

        public static void DeleteCalendar(string customerId, string id)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.DeleteRequest request = new ResourcesResource.CalendarsResource.DeleteRequest(connection.Item, customerId, id);
                request.ExecuteWithBackoff();
            }
        }

        public static CalendarResource GetCalendar(string customerId, string id)
        {
            return ResourceRequestFactory.GetCalendar(customerId, id, null);
        }

        public static CalendarResource GetCalendar(string customerId, string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.GetRequest request = new ResourcesResource.CalendarsResource.GetRequest(connection.Item, customerId, id);

                request.Fields = fields;

                return request.ExecuteWithBackoff();
            }
        }

        public static CalendarResource AddCalendar(string customerId, CalendarResource item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.InsertRequest request = new ResourcesResource.CalendarsResource.InsertRequest(connection.Item, item, customerId);
                return request.ExecuteWithBackoff();
            }
        }

        public static CalendarResource PatchCalendar(string customerId, string id, CalendarResource item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.PatchRequest request = new ResourcesResource.CalendarsResource.PatchRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public static CalendarResource UpdateCalendar(string customerId, string id, CalendarResource item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                ResourcesResource.CalendarsResource.UpdateRequest request = new ResourcesResource.CalendarsResource.UpdateRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }
    }
}