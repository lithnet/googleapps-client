using System;
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
    internal class RateLimiter
    {
        private static Dictionary<string, TokenBucket> buckets;

        private static Dictionary<string, SemaphoreSlim> semaphores;
        
        static RateLimiter()
        {
            buckets = new Dictionary<string, TokenBucket>();
            semaphores = new Dictionary<string, SemaphoreSlim>();
            SetDefaultRateLimits();
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
            RateLimiter.SetRateLimit(new ClassroomService().Name, 48, new TimeSpan(0, 0, 10));

        }

        internal static TokenBucket GetOrCreateBucket(string name)
        {
            if (!RateLimiter.buckets.ContainsKey(name))
            {
                RateLimiter.SetRateLimit(name, 1500, new TimeSpan(0, 0, 100));
            }

            return RateLimiter.buckets[name];
        }

        private static void SetRateLimit(string serviceName, int requestsPerInterval, TimeSpan refillInterval)
        {
            if (!RateLimiter.buckets.ContainsKey(serviceName))
            {
                RateLimiter.buckets.Add(serviceName, new TokenBucket(serviceName, requestsPerInterval, refillInterval, requestsPerInterval));
            }
            else
            {
                RateLimiter.buckets[serviceName] = new TokenBucket(serviceName, requestsPerInterval, refillInterval, requestsPerInterval);
            }
        }

        internal static void SetConcurrentLimit(string serviceName, int maxConcurrentOperations)
        {
            if (!semaphores.ContainsKey(serviceName))
            {
                semaphores.Add(serviceName, new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations));
            }
            else
            {
                semaphores[serviceName] = new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations);
            }
        }

        internal static void WaitForGate(string serviceName)
        {
            if (serviceName == null)
            {
                return;
            }

            if (semaphores.ContainsKey(serviceName))
            {
                Trace.WriteLine($"Waiting for semaphore for service {serviceName}");
                semaphores[serviceName].Wait();
                Trace.WriteLine($"Got semaphore for service {serviceName}");
            }
        }

        internal static void ReleaseGate(string serviceName)
        {
            if (serviceName == null)
            {
                return;
            }

            if (semaphores.ContainsKey(serviceName))
            {
                try
                {
                    semaphores[serviceName].Release();
                    Trace.WriteLine($"Released semaphore for service {serviceName}");
                }
                catch (SemaphoreFullException)
                {
                }
            }
        }
    }
}