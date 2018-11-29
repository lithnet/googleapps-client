using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace Lithnet.GoogleApps
{
    public static class GmailServiceRequestFactory
    {
        private static string serviceName;

        static GmailServiceRequestFactory()
        {
            GmailServiceRequestFactory.serviceName = typeof(GmailService).Name;
        }

        public static IEnumerable<string> GetDelegates(string id)
        {
            if (id.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            RateLimiter.GetOrCreateBucket(GmailServiceRequestFactory.serviceName).Consume();

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                var request = connection.Item.Users.Settings.Delegates.List(id);
                var result = request.ExecuteWithBackoff();

                return result.Delegates.Select(t => t.DelegateEmail);
            }
        }

        public static IEnumerable<string> GetSendAs(string id)
        {
            if (id.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            RateLimiter.GetOrCreateBucket(GmailServiceRequestFactory.serviceName).Consume();

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                var request = connection.Item.Users.Settings.SendAs.List(id);
                var result = request.ExecuteWithBackoff();

                return result.SendAs.Select(t => t.SendAsEmail);
            }
        }

        public static void RemoveDelegate(string id, string @delegate)
        {
            RateLimiter.GetOrCreateBucket(GmailServiceRequestFactory.serviceName).Consume();

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                var request = connection.Item.Users.Settings.Delegates.Delete(id, @delegate);
                request.ExecuteWithBackoff();
            }
        }

        public static void RemoveSendAs(string id, string sendas)
        {
            RateLimiter.GetOrCreateBucket(GmailServiceRequestFactory.serviceName).Consume();

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                var request = connection.Item.Users.Settings.SendAs.Delete(id, sendas);
                request.ExecuteWithBackoff();
            }
        }

        public static void RemoveDelegate(string id)
        {
            RateLimiter.GetOrCreateBucket(GmailServiceRequestFactory.serviceName).Consume();

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                var result = connection.Item.Users.Settings.Delegates.List(id).ExecuteWithBackoff();

                foreach (var item in result.Delegates)
                {
                    connection.Item.Users.Settings.Delegates.Delete(id, item.DelegateEmail).ExecuteWithBackoff();
                }
            }
        }
        public static void RemoveSendAs(string id)
        {
            RateLimiter.GetOrCreateBucket(GmailServiceRequestFactory.serviceName).Consume();

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                var result = connection.Item.Users.Settings.SendAs.List(id).ExecuteWithBackoff();

                foreach (var item in result.SendAs)
                {
                    connection.Item.Users.Settings.SendAs.Delete(id, item.SendAsEmail).ExecuteWithBackoff();
                }
            }
        }

        public static void AddDelegate(string id, string @delegate)
        {
            if (id.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                Google.Apis.Gmail.v1.Data.Delegate d = new Google.Apis.Gmail.v1.Data.Delegate();
                d.DelegateEmail = @delegate;

                connection.Item.Users.Settings.Delegates.Create(d, id).ExecuteWithBackoff();
            }
        }

        public static void AddSendAs(string id, string sendAs)
        {
            if (id.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                SendAs s = new SendAs();
                s.SendAsEmail = sendAs;

                connection.Item.Users.Settings.SendAs.Create(s, id).ExecuteWithBackoff();
            }
        }
  
        public static void AddSendAs(string id, IEnumerable<string> sendAs)
        {
            if (id.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                foreach (string item in sendAs)
                {
                    SendAs s = new SendAs();
                    s.SendAsEmail = item;

                    connection.Item.Users.Settings.SendAs.Create(s, id).ExecuteWithBackoff();
                }
            }
        }
        
        public static void AddDelegate(string id, IEnumerable<string> delegates)
        {
            if (id.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            using (PoolItem<GmailService> connection = ConnectionPools.GmailServicePool.Take())
            {
                foreach (string item in delegates)
                {
                    Google.Apis.Gmail.v1.Data.Delegate s = new Google.Apis.Gmail.v1.Data.Delegate();
                    s.DelegateEmail = item;

                    connection.Item.Users.Settings.Delegates.Create(s, id).ExecuteWithBackoff();
                }
            }
        }
    }
}
