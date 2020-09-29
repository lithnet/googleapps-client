using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Calendar.v3;
using Google.Apis.Classroom.v1;
using Google.Apis.Gmail.v1;
using Google.Apis.Groupssettings.v1;
using Google.GData.Contacts;

namespace Lithnet.GoogleApps
{
    public static class RateLimiter
    {
        private static ConcurrentDictionary<string, TokenBucket> buckets;

        private static ConcurrentDictionary<string, SemaphoreSlim> semaphores;

        static RateLimiter()
        {
            RateLimiter.buckets = new ConcurrentDictionary<string, TokenBucket>();
            RateLimiter.semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
            RateLimiter.SetDefaultRateLimits();
        }

        public static void SetRateLimitDirectoryService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(new DirectoryService().Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitCalendarService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(new CalendarService().Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitGroupSettingsService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(new GroupssettingsService().Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitContactsService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(typeof(ContactsService).Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitGmailService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(new GmailService().Name, requestsPerInterval, interval);
        }

        public static void SetRateLimitClassroomService(int requestsPerInterval, TimeSpan interval)
        {
            RateLimiter.SetRateLimit(new ClassroomService().Name, requestsPerInterval, interval);
        }

        private static void SetDefaultRateLimits()
        {
            RateLimiter.SetRateLimit(new DirectoryService().Name, 1500, new TimeSpan(0, 0, 100));
            RateLimiter.SetRateLimit(new GroupssettingsService().Name, 500, new TimeSpan(0, 0, 100));
            RateLimiter.SetRateLimit(new CalendarService().Name, 1500, new TimeSpan(0, 0, 100));
            RateLimiter.SetRateLimit(new GmailService().Name, 250, new TimeSpan(0, 0, 1));
            RateLimiter.SetRateLimit(typeof(ContactsService).Name, 1500, new TimeSpan(0, 0, 100));
            RateLimiter.SetRateLimit(new ClassroomService().Name, 50, new TimeSpan(0, 0, 10));
        }

        internal static TokenBucket GetOrCreateBucket(string serviceName)
        {
            return RateLimiter.buckets.GetOrAdd(serviceName, new TokenBucket(serviceName, 1500, new TimeSpan(0, 0, 100), 1500));
        }

        private static void SetRateLimit(string serviceName, int requestsPerInterval, TimeSpan refillInterval)
        {
            RateLimiter.buckets.AddOrUpdate(serviceName, new TokenBucket(serviceName, requestsPerInterval, refillInterval, requestsPerInterval),
                 (__, _) => new TokenBucket(serviceName, requestsPerInterval, refillInterval, requestsPerInterval));
        }

        internal static void SetConcurrentLimit(string serviceName, int maxConcurrentOperations)
        {
            RateLimiter.semaphores.AddOrUpdate(serviceName, new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations),
                (__, _) => new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations));
        }

        internal static void WaitForGate(string serviceName)
        {
            if (serviceName == null)
            {
                return;
            }
            
            if (RateLimiter.semaphores.ContainsKey(serviceName))
            {
                Trace.WriteLine($"Waiting for semaphore for service {serviceName}");
                RateLimiter.semaphores[serviceName].Wait();
                Trace.WriteLine($"Got semaphore for service {serviceName}");
            }
        }

        internal static void ReleaseGate(string serviceName)
        {
            if (serviceName == null)
            {
                return;
            }

            if (RateLimiter.semaphores.ContainsKey(serviceName))
            {
                try
                {
                    RateLimiter.semaphores[serviceName].Release();
                    Trace.WriteLine($"Released semaphore for service {serviceName}");
                }
                catch (SemaphoreFullException)
                {
                }
            }
        }
    }
}