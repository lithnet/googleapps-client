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
using System.Security;

namespace Lithnet.GoogleApps
{
    public static class UserRequestFactory
    {
        public static IEnumerable<User> GetUsers(string customerID)
        {
            return UserRequestFactory.GetUsers(customerID, null);
        }

        public static IEnumerable<User> GetUsers(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                UserListRequest request = new UserListRequest(connection.Item)
                {
                    Customer = customerID,
                    MaxResults = 500
                };

                if (fields != null)
                {
                    request.Projection = ProjectionEnum.Full;
                    request.Fields = fields;
                }

                request.PrettyPrint = false;

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

        public static void SetPassword(string id, string newPassword)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                User user = new User
                {
                    Password = newPassword
                };

                UserPatchRequest request = new UserPatchRequest(connection.Item, user, id);

                request.ExecuteWithBackoff();
            }
        }

        public static void SetPassword(string id, SecureString newPassword)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                User user = new User
                {
                    SecurePassword = newPassword
                };

                UserPatchRequest request = new UserPatchRequest(connection.Item, user, id);

                request.ExecuteWithBackoff();
            }
        }

        public static void Delete(string id)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserDeleteRequest request = new UserDeleteRequest(connection.Item, id);
                request.ExecuteWithBackoff();
            }
        }

        public static void Undelete(string id, string orgUnitPath = "/")
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UsersResource.UndeleteRequest request = new UsersResource.UndeleteRequest(connection.Item,
                    new UserUndelete() {OrgUnitPath = orgUnitPath}, id);

                request.ExecuteWithBackoff();
            }
        }

        public static IEnumerable<User> GetDeletedUsers(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                UserListRequest request = new UserListRequest(connection.Item)
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
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserGetRequest request = new UserGetRequest(connection.Item, id);

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
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                InsertRequest request = new InsertRequest(connection.Item, item);
                return request.ExecuteWithBackoff();
            }
        }

        public static void MakeAdmin(bool isAdmin, string userKey)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                MakeAdminRequest request = new MakeAdminRequest(connection.Item, isAdmin, userKey);

                request.ExecuteWithBackoff();
            }
        }

        public static User Patch(User item, string userKey)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserPatchRequest request = new UserPatchRequest(connection.Item, item, userKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static User Update(User item, string userKey)
        {
            if (item.Creating)
            {
                return UserRequestFactory.Add(item);
            }

            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                UserUpdateRequest request = new UserUpdateRequest(connection.Item, item, userKey);
                return request.ExecuteWithBackoff();
            }
        }

        public static IEnumerable<string> GetAliases(string id)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAliasListRequest request = new UserAliasListRequest(connection.Item, id);

                UserAliases result = request.ExecuteWithBackoff();

                if (result.AliasesValue == null)
                {
                    return new List<string>();
                }

                return result.AliasesValue.Select(t => t.AliasValue);
            }
        }

        public static void AddAlias(string id, string newAlias)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAlias alias = new UserAlias
                {
                    AliasValue = newAlias
                };

                UserAliasInsertRequest request = new UserAliasInsertRequest(connection.Item, alias, id);

                request.ExecuteWithBackoff();
            }
        }

        public static void RemoveAlias(string id, string existingAlias)
        {
            using (PoolItem<DirectoryService> connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAliasDeleteRequest request = new UserAliasDeleteRequest(connection.Item, id, existingAlias);

                request.ExecuteWithBackoff();
            }
        }

    }
}