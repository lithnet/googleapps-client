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
using User = Lithnet.GoogleApps.ManagedObjects.User;

namespace Lithnet.GoogleApps
{
    public static class UserRequestFactory
    {
        public static void StartImport(string customerID, BlockingCollection<object> importUsers)
        {
            UserRequestFactory.StartImport(customerID, null, importUsers);
        }

        public static void StartImport(string customerID, string fields, BlockingCollection<object> importUsers)
        {
            UserRequestFactory.GetUsers(customerID, fields, importUsers);
        }

        private static void GetUsers(string customerID, string fields, BlockingCollection<object> importUsers)
        {
            int totalUsers = 0;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                UserListRequest request = new UserListRequest(connection.Client);
                request.Customer = customerID;
                request.MaxResults = 500;
                if (fields != null)
                {
                    request.Projection = UserListRequest.ProjectionEnum.Full;
                    request.Fields = fields;
                }

                request.PrettyPrint = false;

                UserList pageResults;

                do
                {
                    request.PageToken = token;

                    pageResults = request.ExecuteWithBackoff();

                    if (pageResults.UsersValue == null)
                    {
                        //Logger.WriteLine("LIST USER page request returned no results");
                        break;
                    }
                    else
                    {
                        //Logger.WriteLine("LIST USER page returned {0} results", pageResults.UsersValue.Count);
                    }

                    foreach (User item in pageResults.UsersValue)
                    {
                        importUsers.Add(item);
                    }

                    totalUsers += pageResults.UsersValue.Count;
                    token = pageResults.NextPageToken;

                } while (token != null);
            }

            timer.Stop();

            //Logger.WriteLine("");
            //Logger.WriteLine("--------------------------------------------------------");
            //Logger.WriteLine("User enumeration completed:   {0}", timer.Elapsed);
            //Logger.WriteLine("");
            //Logger.WriteLine("Total users                   {0}", totalUsers);
            //Logger.WriteLine("Back-off retries              {0}", ApiExtensions.BackoffRetries);
        }

        public static void SetPassword(string id, string newPassword)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                User user = new User
                {
                    Password = newPassword
                };

                UserPatchRequest request = new UserPatchRequest(connection.Client, user, id);

                request.ExecuteWithBackoff();
            }
        }

        public static void Delete(string id)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserDeleteRequest request = new UserDeleteRequest(connection.Client, id);
                request.ExecuteWithBackoff();
            }
        }

        public static void Undelete(string id, string orgUnitPath = "/")
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UsersResource.UndeleteRequest request = new UsersResource.UndeleteRequest(connection.Client,
                    new UserUndelete() {OrgUnitPath = orgUnitPath}, id);

                request.ExecuteWithBackoff();
            }
        }

        public static IEnumerable<User> GetDeletedUsers(string customerID, string fields)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                UserListRequest request = new UserListRequest(connection.Client)
                {
                    Customer = customerID,
                    MaxResults = 500,
                    Fields = fields,
                    ShowDeleted = "true",
                    PrettyPrint = false
                };

                do
                {
                    request.PageToken = token;

                    UserList pageResults = request.ExecuteWithBackoff();

                    if (pageResults.UsersValue == null)
                    {
                        break;
                    }

                    foreach (User item in pageResults.UsersValue)
                    {
                        yield return item;
                    }

                    token = pageResults.NextPageToken;
                } while (token != null);
            }
        }

        public static User Get(string id)
        {
            return UserRequestFactory.Get(id, null);
        }

        public static User Get(string id, string fields)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserGetRequest request = new UserGetRequest(connection.Client, id);

                if (fields != null)
                {
                    request.Fields = fields;
                    request.Projection = UserGetRequest.ProjectionEnum.Custom;
                }
                else
                {
                    request.Projection = UserGetRequest.ProjectionEnum.Full;
                }

                return request.ExecuteWithBackoff();
            }
        }

        public static User Add(User item)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                InsertRequest request = new InsertRequest(connection.Client, item);
                return request.ExecuteWithBackoff();
            }
        }

        public static void MakeAdmin(bool isAdmin, string userKey)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MakeAdminRequest request = new MakeAdminRequest(connection.Client, isAdmin, userKey);

                request.ExecuteWithBackoff();
            }
        }

        public static User Patch(User item, string userKey)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserPatchRequest request = new UserPatchRequest(connection.Client, item, userKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static User Update(User item, string userKey)
        {
            if (item.Creating)
            {
                return UserRequestFactory.Add(item);
            }

            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                UserUpdateRequest request = new UserUpdateRequest(connection.Client, item, userKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static IEnumerable<string> GetAliases(string id)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {

                UserAliasListRequest request = new UserAliasListRequest(connection.Client, id);

                UserAliases result = request.ExecuteWithBackoff();
                return result.AliasesValue.Select(t => t.AliasValue);
            }
        }

        public static void AddAlias(string id, string newAlias)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAlias alias = new UserAlias
                {
                    AliasValue = newAlias
                };

                UserAliasInsertRequest request = new UserAliasInsertRequest(connection.Client, alias, id);

                request.ExecuteWithBackoff();
            }
        }

        public static void RemoveAlias(string id, string existingAlias)
        {
            using (BaseClientServiceWrapper<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAliasDeleteRequest request = new UserAliasDeleteRequest(connection.Client, id, existingAlias);

                request.ExecuteWithBackoff();
            }
        }

    }
}