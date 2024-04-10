using System;
using System.Collections.Generic;
using System.Linq;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Newtonsoft.Json;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using User = Lithnet.GoogleApps.ManagedObjects.User;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;

namespace Lithnet.GoogleApps
{
    public class UserRequestFactory
    {
        private readonly BaseClientServicePool<DirectoryService> directoryServicePool;

        public UserRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize)
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

        public IEnumerable<User> GetUsers(string customerID)
        {
            return this.GetUsers(customerID, null);
        }

        public IEnumerable<User> GetUsers(string customerID, string fields)
        {
            return this.GetUsers(customerID, fields, null);
        }

        public IEnumerable<User> GetUsers(string customerID, string fields, string query)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
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
                
                request.Query = query;
                request.PrettyPrint = false;

                do
                {
                    request.PageToken = token;

                    UserList pageResults = request.ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.Timeout);

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

        public void SetPassword(string id, string newPassword)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                User user = new User
                {
                    Password = newPassword
                };

                UserPatchRequest request = new UserPatchRequest(connection.Item, user, id);

                request.ExecuteWithRetry(RetryEvents.BackoffNotFound);
            }
        }

        public void SetPassword(string id, SecureString newPassword)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                User user = new User
                {
                    SecurePassword = newPassword
                };

                UserPatchRequest request = new UserPatchRequest(connection.Item, user, id);

                request.ExecuteWithRetry(RetryEvents.BackoffNotFound);
            }
        }

        public void Delete(string id)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserDeleteRequest request = new UserDeleteRequest(connection.Item, id);
                request.ExecuteWithRetryOnBackoff();
            }
        }

        public void Undelete(string id, string orgUnitPath = "/")
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                UsersResource.UndeleteRequest request = new UsersResource.UndeleteRequest(connection.Item,
                    new UserUndelete() { OrgUnitPath = orgUnitPath }, id);

                request.ExecuteWithRetryOnBackoff();
            }
        }

        public IEnumerable<User> GetDeletedUsers(string customerID, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
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

                    UserList pageResults = request.ExecuteWithRetryOnBackoff();

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

        public User Get(string id)
        {
            return this.Get(id, null);
        }

        public User Get(string id, string fields)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
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

                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public User Add(User item)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                InsertRequest request = new InsertRequest(connection.Item, item);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public void MakeAdmin(bool isAdmin, string userKey)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                MakeAdminRequest request = new MakeAdminRequest(connection.Item, isAdmin, userKey);

                request.ExecuteWithRetry(RetryEvents.BackoffNotFound);
            }
        }

        public User Patch(User item, string userKey)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserPatchRequest request = new UserPatchRequest(connection.Item, item, userKey);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public User Update(User item, string userKey)
        {
            if (item.Creating)
            {
                return this.Add(item);
            }

            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Include))
            {
                UserUpdateRequest request = new UserUpdateRequest(connection.Item, item, userKey);
                return request.ExecuteWithRetryOnBackoff();
            }
        }

        public IEnumerable<string> GetAliases(string id)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAliasListRequest request = new UserAliasListRequest(connection.Item, id);

                UserAliases result = request.ExecuteWithRetryOnBackoff();

                if (result.AliasesValue == null)
                {
                    return new List<string>();
                }

                return result.AliasesValue.Select(t => t.AliasValue);
            }
        }

        public void AddAlias(string id, string newAlias)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAlias alias = new UserAlias
                {
                    AliasValue = newAlias
                };

                UserAliasInsertRequest request = new UserAliasInsertRequest(connection.Item, alias, id);

                request.ExecuteWithRetry(RetryEvents.BackoffNotFound);
            }
        }

        public void RemoveAlias(string id, string existingAlias)
        {
            using (PoolItem<DirectoryService> connection = this.directoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAliasDeleteRequest request = new UserAliasDeleteRequest(connection.Item, id, existingAlias);

                request.ExecuteWithRetryOnBackoff();
            }
        }
    }
}