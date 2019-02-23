using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Google.Apis.Requests;
using System.Security;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Google.Apis.Auth.OAuth2.Responses;
using System.Threading.Tasks;
using System.Net.Http;

namespace Lithnet.GoogleApps
{
    public static class ApiExtensions
    {
        private static int retryCount = 8;

        private static Random randomNumberGenerator = new Random();

        public static int RetryCount
        {
            get => ApiExtensions.retryCount;
            set => ApiExtensions.retryCount = value;
        }

        public static void ExecuteWithRetryOnBackoff(this BatchRequest request, string serviceName)
        {
            request.ExecuteWithRetryOnBackoff(serviceName, ApiExtensions.RetryCount);
        }

        public static void ExecuteWithRetryOnBackoff(this BatchRequest request, string serviceName, int retryAttempts)
        {
            request.ExecuteWithRetryOnBackoff(serviceName, retryAttempts, 1);
        }

        public static void ExecuteWithRetryOnBackoff(this BatchRequest request, string serviceName, int retryAttempts, int consumeTokens)
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
                            ApiExtensions.SleepThread(attemptCount, retryAttempts);
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        public static T ExecuteWithRetryOnBackoff<T>(this ClientServiceRequest<T> request)
        {
            return request.ExecuteWithRetryOnBackoff(ApiExtensions.RetryCount);
        }

        public static T ExecuteWithRetryOnBackoff<T>(this ClientServiceRequest<T> request, int retryAttempts)
        {
            return request.ExecuteWithRetryOnBackoff(retryAttempts, 1);
        }

        public static T ExecuteWithRetryOnBackoff<T>(this ClientServiceRequest<T> request, int retryAttempts, int consumeTokens)
        {
            return request.ExecuteWithRetry(RetryEvents.Backoff, retryAttempts, consumeTokens);
        }

        public static T ExecuteWithRetry<T>(this ClientServiceRequest<T> request, RetryEvents policy)
        {
            return request.ExecuteWithRetry(policy, ApiExtensions.retryCount, 1);
        }

        public static T ExecuteWithRetry<T>(this ClientServiceRequest<T> request, RetryEvents policy, int retryAttempts)
        {
            return request.ExecuteWithRetry(policy, retryAttempts, 1);
        }

        public static T ExecuteWithRetry<T>(this ClientServiceRequest<T> request, RetryEvents policy, int retryAttempts, int consumeTokens)
        {
            bool Result(Exception ex) => policy.HasFlag(RetryEvents.Backoff) && ApiExtensions.ShouldRetryOnBackoffError(ex)
                                         || policy.HasFlag(RetryEvents.NotFound) && ApiExtensions.ShouldRetryOnNotFound(ex)
                                         || policy.HasFlag(RetryEvents.OAuthImpersonationError) && ApiExtensions.ShouldRetryOnOAuthError(ex)
                                         || policy.HasFlag(RetryEvents.BadRequest) && ApiExtensions.ShouldRetryOnBadRequest(ex)
                                         || policy.HasFlag(RetryEvents.Aborted) && ApiExtensions.ShouldRetryOnAborted(ex)
                                         || policy.HasFlag(RetryEvents.Timeout) && ApiExtensions.ShouldRetryOnTimeout(ex);

            return request.ExecuteWithRetry((Func<Exception, bool>) Result, retryAttempts, consumeTokens);
        }

        public static T ExecuteWithRetry<T>(this ClientServiceRequest<T> request, Func<Exception, bool> shouldRetry, int retryAttempts, int consumeTokens)
        {
            int attemptCount = 0;

            if (retryAttempts < 0)
            {
                retryAttempts = Math.Max(0, ApiExtensions.retryCount);
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
                catch (Exception ex)
                {
                    if (attemptCount <= retryAttempts)
                    {
                        if (shouldRetry(ex))
                        {
                            ApiExtensions.SleepThread(attemptCount, retryAttempts);
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
                case (HttpStatusCode) 429:
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

        private static void SleepThread(int attemptCount, int retryAttempts)
        {
            int interval = (attemptCount * 1000) + randomNumberGenerator.Next(1000);
            Trace.WriteLine($"Backing off request attempt {attemptCount}/{retryAttempts} for {interval} milliseconds");
            Thread.Sleep(interval);
        }

        public static bool IsNullOrNullPlaceholder(this string s)
        {
            return s == null || s == Constants.NullValuePlaceholder;
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


        private static bool ShouldRetryOnBackoffError(Exception e)
        {
            if (e is Google.GoogleApiException ex)
            {
                Trace.WriteLine($"Google API request error - {ex.Error?.Code} {ex.Error?.Message}");

                if (ApiExtensions.IsRetryableError(ex.HttpStatusCode, ex.Message))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldRetryOnOAuthError(Exception e)
        {
            if (e is TokenResponseException ex)
            {
                Trace.WriteLine($"Google OAuth request error - {ex.StatusCode} {ex.Message}");

                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldRetryOnNotFound(Exception e)
        {
            if (e is Google.GoogleApiException ex)
            {
                Trace.WriteLine($"Google API request error - {ex.Error?.Code} {ex.Error?.Message}");

                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldRetryOnBadRequest(Exception e)
        {
            if (e is Google.GoogleApiException ex)
            {
                Trace.WriteLine($"Google API request error - {ex.Error?.Code} {ex.Error?.Message}");

                if (ex.HttpStatusCode == HttpStatusCode.BadRequest)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldRetryOnTimeout(Exception e)
        {
            if (e is TaskCanceledException || e is HttpRequestException)
            {
                Trace.WriteLine($"Timeout error - {e.Message} {e.StackTrace}");

                return true;
            }

            return false;
        }

        private static bool ShouldRetryOnAborted(Exception e)
        {
            if (e is Google.GoogleApiException ex)
            {
                Trace.WriteLine($"Google API request error - {ex.Error?.Code} {ex.Error?.Message}");

                if (ex.HttpStatusCode == HttpStatusCode.Conflict && ex.Message.IndexOf("The operation was aborted", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

         


    }
}