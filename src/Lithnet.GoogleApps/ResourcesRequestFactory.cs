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
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

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

        public static IEnumerable<AclRule> GetCalendarAclRules(string customerId, string calendarId)
        {
            return ResourceRequestFactory.GetCalendarAclRules(customerId, calendarId, null);
        }

        public static IEnumerable<AclRule> GetCalendarAclRules(string customerId, string calendarId, string fields)
        {
            using (PoolItem<CalendarService> connection = ConnectionPools.CalendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.ListRequest request = new AclResource.ListRequest(connection.Item, calendarId);
                string token = null;

                request.MaxResults = 500;

                if (fields != null)
                {
                    request.Fields = fields;
                }

                request.PrettyPrint = false;

                do
                {
                    request.PageToken = token;

                    Acl pageResults = request.ExecuteWithBackoff();

                    if (pageResults.Items == null)
                    {
                        break;
                    }

                    foreach (AclRule item in pageResults.Items)
                    {
                        yield return item;
                    }

                    token = pageResults.NextPageToken;

                } while (token != null);
            }
        }

        public static void GetCalendarAclRule(string customerId, string calendarId, string ruleId)
        {
            using (PoolItem<CalendarService> connection = ConnectionPools.CalendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.GetRequest request = new AclResource.GetRequest(connection.Item, calendarId, ruleId);
                request.ExecuteWithBackoff();
            }
        }

        public static void DeleteCalendarAclRule(string customerId, string calendarId, string ruleId)
        {
            using (PoolItem<CalendarService> connection = ConnectionPools.CalendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.DeleteRequest request = new AclResource.DeleteRequest(connection.Item, calendarId, ruleId);
                request.ExecuteWithBackoff();
            }
        }

        public static void AddCalendarAclRule(string customerId, string calendarId, AclRule body, bool sendNotifications)
        {
            using (PoolItem<CalendarService> connection = ConnectionPools.CalendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.InsertRequest request = new AclResource.InsertRequest(connection.Item, body, calendarId);
                request.SendNotifications = sendNotifications;
                request.ExecuteWithBackoff();
            }
        }

        public static void UpdateCalendarAclRule(string customerId, string calendarId, string ruleId, AclRule body, bool sendNotifications)
        {
            using (PoolItem<CalendarService> connection = ConnectionPools.CalendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.UpdateRequest request = new AclResource.UpdateRequest(connection.Item, body, calendarId, ruleId);
                request.SendNotifications = sendNotifications;
                request.ExecuteWithBackoff();
            }
        }

        public static void PatchCalendarAclRule(string customerId, string calendarId, string ruleId, AclRule body, bool sendNotifications)
        {
            using (PoolItem<CalendarService> connection = ConnectionPools.CalendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.PatchRequest request = new AclResource.PatchRequest(connection.Item, body, calendarId, ruleId);
                request.SendNotifications = sendNotifications;
                request.ExecuteWithBackoff();
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

        public static IEnumerable<Building> GetBuildings(string customerID)
        {
            return ResourceRequestFactory.GetBuildings(customerID, null);
        }

        public static IEnumerable<Building> GetBuildings(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;

                ResourcesResource.BuildingsResource.ListRequest request = new ResourcesResource.BuildingsResource.ListRequest(connection.Item, customerID);

                if (fields != null)
                {
                    request.Fields = fields;
                }

                request.PrettyPrint = false;

                do
                {
                    Buildings pageResults = request.ExecuteWithBackoff();

                    if (pageResults.BuildingsValue == null)
                    {
                        break;
                    }

                    foreach (Building item in pageResults.BuildingsValue)
                    {
                        yield return item;
                    }

                    token = pageResults.NextPageToken;

                } while (token != null);
            }
        }

        public static void DeleteBuilding(string customerId, string id)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.DeleteRequest request = new ResourcesResource.BuildingsResource.DeleteRequest(connection.Item, customerId, id);
                request.ExecuteWithBackoff();
            }
        }

        public static Building GetBuilding(string customerId, string id)
        {
            return ResourceRequestFactory.GetBuilding(customerId, id, null);
        }

        public static Building GetBuilding(string customerId, string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.GetRequest request = new ResourcesResource.BuildingsResource.GetRequest(connection.Item, customerId, id);

                request.Fields = fields;

                return request.ExecuteWithBackoff();
            }
        }

        public static Building AddBuilding(string customerId, Building item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.InsertRequest request = new ResourcesResource.BuildingsResource.InsertRequest(connection.Item, item, customerId);
                return request.ExecuteWithBackoff();
            }
        }

        public static Building PatchBuilding(string customerId, string id, Building item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.PatchRequest request = new ResourcesResource.BuildingsResource.PatchRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public static Building UpdateBuilding(string customerId, string id, Building item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                ResourcesResource.BuildingsResource.UpdateRequest request = new ResourcesResource.BuildingsResource.UpdateRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }


        public static IEnumerable<Feature> GetFeatures(string customerID)
        {
            return ResourceRequestFactory.GetFeatures(customerID, null);
        }

        public static IEnumerable<Feature> GetFeatures(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;

                ResourcesResource.FeaturesResource.ListRequest request = new ResourcesResource.FeaturesResource.ListRequest(connection.Item, customerID);

                if (fields != null)
                {
                    request.Fields = fields;
                }

                request.PrettyPrint = false;

                do
                {
                    Features pageResults = request.ExecuteWithBackoff();

                    if (pageResults.FeaturesValue == null)
                    {
                        break;
                    }

                    foreach (Feature item in pageResults.FeaturesValue)
                    {
                        yield return item;
                    }

                    token = pageResults.NextPageToken;

                } while (token != null);
            }
        }

        public static void DeleteFeature(string customerId, string id)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.DeleteRequest request = new ResourcesResource.FeaturesResource.DeleteRequest(connection.Item, customerId, id);
                request.ExecuteWithBackoff();
            }
        }

        public static Feature GetFeature(string customerId, string id)
        {
            return ResourceRequestFactory.GetFeature(customerId, id, null);
        }

        public static Feature GetFeature(string customerId, string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.GetRequest request = new ResourcesResource.FeaturesResource.GetRequest(connection.Item, customerId, id);

                request.Fields = fields;

                return request.ExecuteWithBackoff();
            }
        }

        public static Feature AddFeature(string customerId, Feature item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.InsertRequest request = new ResourcesResource.FeaturesResource.InsertRequest(connection.Item, item, customerId);
                return request.ExecuteWithBackoff();
            }
        }

        public static Feature PatchFeature(string customerId, string id, Feature item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.PatchRequest request = new ResourcesResource.FeaturesResource.PatchRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public static Feature UpdateFeature(string customerId, string id, Feature item)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                ResourcesResource.FeaturesResource.UpdateRequest request = new ResourcesResource.FeaturesResource.UpdateRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }
    }
}