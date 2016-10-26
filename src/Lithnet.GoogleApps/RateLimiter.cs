using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lithnet.GoogleApps
{
    internal static class RateLimiter
    {
        private static Dictionary<string, TokenBucket> buckets;

        private static Dictionary<string, SemaphoreSlim> semaphores;

        static RateLimiter()
        {
            buckets = new Dictionary<string, TokenBucket>();
            semaphores = new Dictionary<string, SemaphoreSlim>();
        }

        internal static TokenBucket GetOrCreateBucket(string name)
        {
            if (!RateLimiter.buckets.ContainsKey(name))
            {
                RateLimiter.SetRateLimit(name, 1500, new TimeSpan(0, 0, 100));
            }

            return RateLimiter.buckets[name];
        }

        internal static void SetRateLimit(string serviceName, int requestsPerInterval, TimeSpan refillInterval)
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