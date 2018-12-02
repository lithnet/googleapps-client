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
        private static int backoffRetryCount = 8;

        private static Random randomNumberGenerator = new Random();

        public static int BackoffRetryCount
        {
            get => ApiExtensions.backoffRetryCount;
            set => ApiExtensions.backoffRetryCount = value;
        }

        public static void ExecuteWithBackoff(this BatchRequest request, string serviceName)
        {
            request.ExecuteWithBackoff(serviceName, ApiExtensions.BackoffRetryCount);
        }

        public static void ExecuteWithBackoff(this BatchRequest request, string serviceName, int retryAttempts)
        {
            request.ExecuteWithBackoff(serviceName, retryAttempts, 1);
        }

        public static void ExecuteWithBackoff(this BatchRequest request, string serviceName, int retryAttempts, int consumeTokens)
        {
            int attemptCount = 0;

            while (true)
            {
                try
                {
                    attemptCount++;

                    RateLimiter.GetOrCreateBucket(serviceName).Consume(request.Count * consumeTokens);

                    request.ExecuteAsync().Wait();
                    break;
                }
                catch (Google.GoogleApiException ex)
                {
                    if (attemptCount <= retryAttempts)
                    {
                        if (ApiExtensions.IsRetryableError(ex.HttpStatusCode, ex.Message))
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request)
        {
            return request.ExecuteWithBackoff(ApiExtensions.BackoffRetryCount, 1);
        }

        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request, int retryAttempts)
        {
            return request.ExecuteWithBackoff(retryAttempts, 1);
        }

        public static T ExecuteWithBackoff<T>(this ClientServiceRequest<T> request, int retryAttempts, int consumeTokens)
        {
            int attemptCount = 0;

            if (retryAttempts < 0)
            {
                retryAttempts = Math.Max(0, backoffRetryCount);
            }

            consumeTokens = Math.Max(1, consumeTokens);

            while (true)
            {
                try
                {
                    attemptCount++;

                    RateLimiter.GetOrCreateBucket(request.Service.Name).Consume(consumeTokens);
                    return request.Execute();
                }
                catch (Google.GoogleApiException ex)
                {
                    Trace.WriteLine($"Google API request error\n{request.HttpMethod} {ex.Error?.Code} {ex.Error?.Message}");

                    if (attemptCount <= retryAttempts)
                    {
                        if (ApiExtensions.IsRetryableError(ex.HttpStatusCode, ex.Message))
                        {
                            ApiExtensions.SleepThread(attemptCount);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        public static bool IsRetryableError(HttpStatusCode code, string message)
        {
            switch (code)
            {
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.ServiceUnavailable:
                case (HttpStatusCode)429:
                    return true;

                case HttpStatusCode.Forbidden:
                    if (message.IndexOf("quotaExceeded", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        message.IndexOf("userRateLimitExceeded", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }

            return false;
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

        public static T ExecuteWithRetryOnNotFound<T>(this Func<T> t, int sleepInterval = 1000)
        {
            try
            {
                return t.Invoke();
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    Trace.WriteLine($"Object was not found. Sleeping {sleepInterval} milliseconds before retrying");
                    //Newly created object was not ready. Sleeping 1 second
                    Thread.Sleep(sleepInterval);
                    return t.Invoke();
                }

                throw;
            }
        }

        public static void ExecuteWithRetryOnNotFound(this Action t, int sleepInterval = 1000)
        {
            try
            {
                t.Invoke();
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    Trace.WriteLine($"Object was not found. Sleeping {sleepInterval} milliseconds before retrying");
                    //Newly created object was not ready. Sleeping 1 second
                    Thread.Sleep(sleepInterval);
                    t.Invoke();
                }

                throw;
            }
        }

        public static T InvokeWithRateLimit<T>(this Func<T> t, string bucketName, int consumeTokens = 1)
        {
            RateLimiter.GetOrCreateBucket(bucketName).Consume(consumeTokens);
            return t.Invoke();
        }

        public static void InvokeWithRateLimit(this Action t, string bucketName, int consumeTokens = 1)
        {
            RateLimiter.GetOrCreateBucket(bucketName).Consume(consumeTokens);
            t.Invoke();
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

        public static void ThrowIfNotEmailAddress(this string email)
        {
            if (email.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }
        }
    }
}
