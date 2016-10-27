using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Google.Apis.Requests;
using System.Security;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Lithnet.GoogleApps
{
    public static class ApiExtensions
    {
        private static int backoffRetryCount = 0;

        private static Random randomNumberGenerator = new Random();

        public static int BackoffRetryCount
        {
            get
            {
                return ApiExtensions.backoffRetryCount;
            }
            set
            {
                ApiExtensions.backoffRetryCount = value;
            }
        }

        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request)
        {
            return request.ExecuteWithBackoff(8);
        }

        public static void ExecuteWithBackoff(this BatchRequest request, string serviceName)
        {
            request.ExecuteWithBackoff(serviceName, 8);
        }

        public static void ExecuteWithBackoff(this BatchRequest request, string serviceName, int retryAttempts)
        {
            int attemptCount = 0;

            while (true)
            {
                try
                {
                    attemptCount++;

                    RateLimiter.GetOrCreateBucket(serviceName).Consume(request.Count);

                    request.ExecuteAsync().Wait();
                    break;
                }
                catch (Google.GoogleApiException ex)
                {
                    if (attemptCount <= retryAttempts)
                    {
                        if (ex.HttpStatusCode == HttpStatusCode.Forbidden &&
                            (ex.Message.IndexOf("quotaExceeded", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             ex.Message.IndexOf("userRateLimitExceeded", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                        else if (ex.HttpStatusCode == HttpStatusCode.InternalServerError)
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                        else if (ex.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }


        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request, int retryAttempts)
        {
            int attemptCount = 0;

            while (true)
            {
                try
                {
                    attemptCount++;

                    RateLimiter.GetOrCreateBucket(request.Service.Name).Consume();

                    return request.Execute();
                }
                catch (Google.GoogleApiException ex)
                {
                    Trace.WriteLine($"Google API request error\n{request.HttpMethod} {ex.Error?.Code} {ex.Error?.Message}");

                    if (attemptCount <= retryAttempts)
                    {
                        if (ex.HttpStatusCode == HttpStatusCode.Forbidden &&
                            (ex.Message.IndexOf("quotaExceeded", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            ex.Message.IndexOf("userRateLimitExceeded", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                        else if (ex.HttpStatusCode == HttpStatusCode.InternalServerError)
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                        else if (ex.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        private static void SleepThread(int attemptCount)
        {
            Interlocked.Increment(ref ApiExtensions.backoffRetryCount);
            int interval = (attemptCount * 1000) + randomNumberGenerator.Next(1000);
            Trace.WriteLine($"Backing off request attempt {attemptCount} for {interval} milliseconds");
            Thread.Sleep(interval);
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
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    //Newly created object was not ready. Sleeping 1 second
                    Thread.Sleep(1000);
                    return t.Invoke();
                }

                throw;
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
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    //Newly created object was not ready. Sleeping 1 second
                    Thread.Sleep(1000);
                    t.Invoke();
                }

                throw;
            }
        }

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException(nameof(securePassword));
            }

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

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int batchSize)
        {
            List<TSource> batch = new List<TSource>(batchSize);

            foreach (TSource item in source)
            {
                batch.Add(item);

                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<TSource>(batchSize);
                }
            }

            if (batch.Any())
            {
                yield return batch;
            }
        }
    }
}
