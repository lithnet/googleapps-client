using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lithnet.GoogleApps.Api;
using Lithnet.GoogleApps.ManagedObjects;
using Newtonsoft.Json;

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
            try
            {
                UserRequestFactory.GetUsers(customerID, fields, importUsers);
            }
            catch (Exception)
            {
                //Logger.WriteException(ex);
                throw;
            }
        }

        private static void GetUsers(string customerID, string fields, BlockingCollection<object> importUsers)
        {
            int totalUsers = 0;

            Stopwatch timer = new Stopwatch();
            timer.Start();

            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                var request = new UserListRequest(connection.Client);
                request.Customer = customerID;
                request.MaxResults = 500;
                request.Fields = fields;
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

                    foreach (var item in pageResults.UsersValue)
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
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                User user = new User();
                user.Password = newPassword;
//#if DEBUG
//                Logger.WriteLine("SET PASSWORD USER request: {0}", id);
//#endif
                var request = new UserPatchRequest(connection.Client, user, id);
                var result = request.ExecuteWithBackoff();
//#if DEBUG
//                Logger.WriteLine("SET PASSWORD USER response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
            }
        }

        public static void Delete(string id)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new UserDeleteRequest(connection.Client, id);
                string result;

                try
                {
#if DEBUG
                    //Logger.WriteLine("DELETE USER request: {0}", id);
#endif
                    result = request.ExecuteWithBackoff();
#if DEBUG
                    //Logger.WriteLine("DELETE USER response: {0}", result);
#endif
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("DELETE USER {0} threw an exception", id);
                    //Logger.WriteException(ex);

                    throw;
                }
            }
        }

        public static void Undelete(string id, string orgUnitPath = "/")
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new Google.Apis.Admin.Directory.directory_v1.UsersResource.UndeleteRequest( connection.Client,
                     new Google.Apis.Admin.Directory.directory_v1.Data.UserUndelete() { OrgUnitPath = null }, id);
                
                //var request = new UserUndeleteRequest(connection.Client, id, orgUnitPath);
                string result;

                try
                {
#if DEBUG
      //              Logger.WriteLine("UNDELETE USER request: {0}", id);
#endif
                    result = request.ExecuteWithBackoff();
#if DEBUG
        //            Logger.WriteLine("UNDELETE USER response: {0}", result);
#endif
                }
                catch (Google.GoogleApiException)
                {
                  //  Logger.WriteLine("UNDELETE USER {0} threw an exception", id);
                    //Logger.WriteException(ex);

                    throw;
                }
            }
        }

        public static IEnumerable<User> GetDeletedUsers(string customerID, string fields)
        {
            int totalUsers = 0;

            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                string token = null;
                var request = new UserListRequest(connection.Client);
                request.Customer = customerID;
                request.MaxResults = 500;
                request.Fields = fields;
                request.ShowDeleted = "true";
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

                    foreach (var item in pageResults.UsersValue)
                    {
                        yield return item;
                    }

                    totalUsers += pageResults.UsersValue.Count;
                    token = pageResults.NextPageToken;

                } while (token != null);
            }
        }

        public static User Get(string id)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new UserGetRequest(connection.Client, id);
                //var request = connection.Client.Users.Get(id);
                User result;

                try
                {
#if DEBUG
                    //Logger.WriteLine("GET USER request: {0}", id);
#endif
                    result = request.ExecuteWithBackoff();
#if DEBUG
                    // Logger.WriteLine("GET USER response: {0}", connection.Client.Serializer.Serialize(result));
#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("GET USER {0} threw an exception", id);
                    //Logger.WriteException(ex);

                    throw;
                }
            }
        }

        public static User Add(User item)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new InsertRequest(connection.Client, item);
                User result;

                try
                {
//#if DEBUG
//                    Logger.WriteLine("ADD USER request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("ADD USER response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("ADD USER {0} threw an exception", item.PrimaryEmail);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static User Patch(User item, string userKey)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new UserPatchRequest(connection.Client, item, userKey);
                User result;

                try
                {
//#if DEBUG
//                    Logger.WriteLine("PATCH USER request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("PATCH USER response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("PATCH USER {0} threw an exception", item.PrimaryEmail);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static User Update(User item, string userKey)
        {
            if (item.Creating)
            {
                return UserRequestFactory.Add(item);
            }

            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Include))
            {
                var request = new UserUpdateRequest(connection.Client, item, userKey);
                User result;

                try
                {
//#if DEBUG
                    // Logger.WriteLine("UPDATE USER request: {0}", connection.Client.Serializer.Serialize(item));
//#endif
                    result = request.ExecuteWithBackoff();
//#if DEBUG
                    // Logger.WriteLine("UPDATE USER response: {0}", connection.Client.Serializer.Serialize(result));
//#endif

                    return result;
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("UPDATE USER {0} threw an exception", item.PrimaryEmail);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static IEnumerable<string> GetAliases(string id)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {

                var request = new UserAliasListRequest(connection.Client, id);

                try
                {
//#if DEBUG
//                    Logger.WriteLine("LIST USER ALIAS request: {0}", id);
//#endif
                    UserAliases result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("LIST USER ALIAS response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                    return result.AliasesValue.Select(t => t.AliasValue);
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("LIST USER ALIAS threw an exception: {0}", id);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static void AddAlias(string id, string newAlias)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                UserAlias alias = new UserAlias();
                alias.AliasValue = newAlias;

                var request = new UserAliasInsertRequest(connection.Client, alias, id);

                try
                {
//#if DEBUG
//                    Logger.WriteLine("ADD USER ALIAS request: {0}", newAlias);
//#endif
                    UserAlias result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("ADD USER ALIAS response: {0}", connection.Client.Serializer.Serialize(result));
//#endif
                }
                catch (Google.GoogleApiException)
                {
                    //Logger.WriteLine("ADD USER ALIAS {0} to user {1} threw an exception", newAlias, id);
                    //Logger.WriteException(ex);
                    throw;
                }
            }
        }

        public static void RemoveAlias(string id, string existingAlias)
        {
            using (var connection = ConnectionPools.DirectoryServicePool.Take(NullValueHandling.Ignore))
            {
                var request = new UserAliasDeleteRequest(connection.Client, id, existingAlias);

                try
                {
//#if DEBUG
//                    Logger.WriteLine("REMOVE USER ALIAS request: {0}", existingAlias);
//#endif
                    string result = request.ExecuteWithBackoff();
//#if DEBUG
//                    Logger.WriteLine("REMOVE USER ALIAS response: {0}", result);
//#endif
                }
                catch (Google.GoogleApiException)
                {
                   // Logger.WriteLine("REMOVE USER ALIAS {0} from user {1} threw an exception", existingAlias, id);
                   // Logger.WriteException(ex);
                    throw;
                }
            }
        }
    }
}
