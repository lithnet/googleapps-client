using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Lithnet.GoogleApps
{
    public class ResourceRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        private readonly BaseClientServicePool<CalendarService> calendarServicePool;

        public ResourceRequestFactory(GoogleServiceCredentials creds, string[] resourceServiceScopes, string[] calendarServiceScopes, int resourceServicePoolSize, int calendarServicePoolSize)
        {
            this.directoryServicePool = new BaseClientServicePool<DirectoryService>(resourceServicePoolSize, () =>
            {
                DirectoryService x = new DirectoryService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = new ServiceAccountCredential(creds.GetInitializer(resourceServiceScopes)),
                    ApplicationName = "LithnetGoogleAppsLibrary",
                    GZipEnabled = !Settings.DisableGzip,
                    Serializer = new GoogleJsonSerializer(),
                    DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                });

                x.HttpClient.Timeout = Timeout.InfiniteTimeSpan;
                return x;
            });
            
            this.calendarServicePool = new BaseClientServicePool<CalendarService>(calendarServicePoolSize, () =>
            {
                CalendarService x = new CalendarService(
                    new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = new ServiceAccountCredential(creds.GetInitializer(calendarServiceScopes)),
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = !Settings.DisableGzip,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                    });

                x.HttpClient.Timeout = Timeout.InfiniteTimeSpan;
                return x;
            });
        }

        public IEnumerable<CalendarResource> GetCalendars(string customerID)
        {
            return this.GetCalendars(customerID, null);
        }

        public IEnumerable<CalendarResource> GetCalendars(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
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

        public void DeleteCalendar(string customerId, string id)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.DeleteRequest request = new ResourcesResource.CalendarsResource.DeleteRequest(connection.Item, customerId, id);
                request.ExecuteWithBackoff();
            }
        }

        public CalendarResource GetCalendar(string customerId, string id)
        {
            return this.GetCalendar(customerId, id, null);
        }

        public CalendarResource GetCalendar(string customerId, string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.GetRequest request = new ResourcesResource.CalendarsResource.GetRequest(connection.Item, customerId, id);

                request.Fields = fields;

                return request.ExecuteWithBackoff();
            }
        }

        public IEnumerable<AclRule> GetCalendarAclRules(string customerId, string calendarId)
        {
            return this.GetCalendarAclRules(customerId, calendarId, null);
        }

        public IEnumerable<AclRule> GetCalendarAclRules(string customerId, string calendarId, string fields)
        {
            using (PoolItem<CalendarService> connection = this.calendarServicePool.Take(NullValueHandling.Ignore))
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

                    Acl pageResults;

                    try
                    {
                        pageResults = request.ExecuteWithBackoff();
                    }
                    catch (Google.GoogleApiException e)
                    {
                        // 2018-01-17 List ACL is returning 404 randomly for some calendars. Subsequent calls seem to work
                        if (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Thread.Sleep(1000);
                            pageResults = request.ExecuteWithBackoff();
                        }
                        else
                        {
                            throw;
                        }
                    }

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

        public void GetCalendarAclRule(string customerId, string calendarId, string ruleId)
        {
            using (PoolItem<CalendarService> connection = this.calendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.GetRequest request = new AclResource.GetRequest(connection.Item, calendarId, ruleId);
                request.ExecuteWithBackoff();
            }
        }

        public void DeleteCalendarAclRule(string customerId, string calendarId, string ruleId)
        {
            using (PoolItem<CalendarService> connection = this.calendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.DeleteRequest request = new AclResource.DeleteRequest(connection.Item, calendarId, ruleId);
                request.ExecuteWithBackoff();
            }
        }

        public void AddCalendarAclRule(string customerId, string calendarId, AclRule body, bool sendNotifications)
        {
            using (PoolItem<CalendarService> connection = this.calendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.InsertRequest request = new AclResource.InsertRequest(connection.Item, body, calendarId);
                request.SendNotifications = sendNotifications;
                request.ExecuteWithBackoff();
            }
        }

        public void UpdateCalendarAclRule(string customerId, string calendarId, string ruleId, AclRule body, bool sendNotifications)
        {
            using (PoolItem<CalendarService> connection = this.calendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.UpdateRequest request = new AclResource.UpdateRequest(connection.Item, body, calendarId, ruleId);
                request.SendNotifications = sendNotifications;
                request.ExecuteWithBackoff();
            }
        }

        public void PatchCalendarAclRule(string customerId, string calendarId, string ruleId, AclRule body, bool sendNotifications)
        {
            using (PoolItem<CalendarService> connection = this.calendarServicePool.Take(NullValueHandling.Ignore))
            {
                AclResource.PatchRequest request = new AclResource.PatchRequest(connection.Item, body, calendarId, ruleId);
                request.SendNotifications = sendNotifications;
                request.ExecuteWithBackoff();
            }
        }

        public CalendarResource AddCalendar(string customerId, CalendarResource item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.InsertRequest request = new ResourcesResource.CalendarsResource.InsertRequest(connection.Item, item, customerId);
                return request.ExecuteWithBackoff();
            }
        }

        public CalendarResource PatchCalendar(string customerId, string id, CalendarResource item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.CalendarsResource.PatchRequest request = new ResourcesResource.CalendarsResource.PatchRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public CalendarResource UpdateCalendar(string customerId, string id, CalendarResource item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Include))
            {
                ResourcesResource.CalendarsResource.UpdateRequest request = new ResourcesResource.CalendarsResource.UpdateRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public IEnumerable<Building> GetBuildings(string customerID)
        {
            return this.GetBuildings(customerID, null);
        }

        public IEnumerable<Building> GetBuildings(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
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

        public void DeleteBuilding(string customerId, string id)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.DeleteRequest request = new ResourcesResource.BuildingsResource.DeleteRequest(connection.Item, customerId, id);
                request.ExecuteWithBackoff();
            }
        }

        public Building GetBuilding(string customerId, string id)
        {
            return this.GetBuilding(customerId, id, null);
        }

        public Building GetBuilding(string customerId, string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.GetRequest request = new ResourcesResource.BuildingsResource.GetRequest(connection.Item, customerId, id);

                request.Fields = fields;

                return request.ExecuteWithBackoff();
            }
        }

        public Building AddBuilding(string customerId, Building item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.InsertRequest request = new ResourcesResource.BuildingsResource.InsertRequest(connection.Item, item, customerId);
                return request.ExecuteWithBackoff();
            }
        }

        public Building PatchBuilding(string customerId, string id, Building item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.BuildingsResource.PatchRequest request = new ResourcesResource.BuildingsResource.PatchRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public Building UpdateBuilding(string customerId, string id, Building item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Include))
            {
                ResourcesResource.BuildingsResource.UpdateRequest request = new ResourcesResource.BuildingsResource.UpdateRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }


        public IEnumerable<Feature> GetFeatures(string customerID)
        {
            return this.GetFeatures(customerID, null);
        }

        public IEnumerable<Feature> GetFeatures(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
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

        public void DeleteFeature(string customerId, string id)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.DeleteRequest request = new ResourcesResource.FeaturesResource.DeleteRequest(connection.Item, customerId, id);
                request.ExecuteWithBackoff();
            }
        }

        public Feature GetFeature(string customerId, string id)
        {
            return this.GetFeature(customerId, id, null);
        }

        public Feature GetFeature(string customerId, string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.GetRequest request = new ResourcesResource.FeaturesResource.GetRequest(connection.Item, customerId, id);

                request.Fields = fields;

                return request.ExecuteWithBackoff();
            }
        }

        public Feature AddFeature(string customerId, Feature item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.InsertRequest request = new ResourcesResource.FeaturesResource.InsertRequest(connection.Item, item, customerId);
                return request.ExecuteWithBackoff();
            }
        }

        public Feature PatchFeature(string customerId, string id, Feature item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                ResourcesResource.FeaturesResource.PatchRequest request = new ResourcesResource.FeaturesResource.PatchRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }

        public Feature UpdateFeature(string customerId, string id, Feature item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Include))
            {
                ResourcesResource.FeaturesResource.UpdateRequest request = new ResourcesResource.FeaturesResource.UpdateRequest(connection.Item, item, customerId, id);
                return request.ExecuteWithBackoff();
            }
        }
    }
}