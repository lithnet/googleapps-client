using System;
using System.Net;
using System.Threading;
using Google.Apis.Requests;
using System.Security;
using System.Runtime.InteropServices;

namespace Lithnet.GoogleApps
{
    public static class ApiExtensions
    {
        public static int BackoffRetries = 0;

        private static Random randomNumberGenerator = new Random();

        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request)
        {
            return request.ExecuteWithBackoff(8);
        }

        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request, int retryAttemps)
        {
            int attemptCount = 0;

            while (true)
            {
                try
                {
                    attemptCount++;
                    return request.Execute();
                }
                catch (Google.GoogleApiException ex)
                {
                    Interlocked.Increment(ref BackoffRetries);
                    if (attemptCount <= retryAttemps)
                    {
                        if (ex.HttpStatusCode == HttpStatusCode.Forbidden && ex.Message.Contains("quotaExceeded"))
                        {
                            SleepThread(attemptCount);
                        }
                        else if (ex.HttpStatusCode == HttpStatusCode.InternalServerError)
                        {
                            SleepThread(attemptCount);
                        }
                        else if (ex.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            SleepThread(attemptCount);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private static void SleepThread(int attemptCount)
        {
            Thread.Sleep((attemptCount * 1000) + randomNumberGenerator.Next(1000));
        }

        public static bool IsNullOrNullPlaceholder(this string s)
        {
            return s == null || s == Constants.NullValuePlaceholder;
        }

        public static T ExecuteWithRetryOnNotFound<T>(this Func<T> t)
        {
            try
            {
                return t.Invoke();
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //Newly created object was not ready. Sleeping 1 second
                    System.Threading.Thread.Sleep(1000);
                    return t.Invoke();
                }
                else
                {
                    throw;
                }
            }
        }

        public static void ExecuteWithRetryOnNotFound(this Action t)
        {
            try
            {
                t.Invoke();
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //Newly created object was not ready. Sleeping 1 second
                    System.Threading.Thread.Sleep(1000);
                    t.Invoke();
                }
                else
                {
                    throw;
                }
            }
        }

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException(nameof(securePassword));

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
