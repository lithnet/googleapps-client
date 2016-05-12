using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lithnet.GoogleApps
{
    using Google.GData.Apps;
    using Google.GData.Apps.GoogleMailSettings;
    using Google.GData.Client;
    using Google.GData.Extensions.Apps;

    public static class UserSettingsRequestFactory
    {
        public static IEnumerable<string> GetDelegates(string mail)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            string[] mailParts = mail.Split('@');

            using (PoolItem<EmailSettingsService> connection = ConnectionPools.UserSettingsServicePool.Take())
            {
                connection.Item.SetDomain(mailParts[1]);

                AppsExtendedFeed x = connection.Item.RetrieveDelegates(mailParts[0]);

                foreach (AppsExtendedEntry item in x.Entries.OfType<AppsExtendedEntry>())
                {
                    PropertyElement property = item.Properties.FirstOrDefault(t => t.Name == "address");

                    if (property != null)
                    {
                        yield return property.Value;
                    }
                }
            }
        }

        public static void RemoveDelegate(string mail, string @delegate)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            string[] mailParts = mail.Split('@');

            using (PoolItem<EmailSettingsService> connection = ConnectionPools.UserSettingsServicePool.Take())
            {
                connection.Item.SetDomain(mailParts[1]);

                connection.Item.DeleteDelegate(mailParts[0], @delegate);
            }
        }

        public static void RemoveDelegates(string mail)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            string[] mailParts = mail.Split('@');

            using (PoolItem<EmailSettingsService> connection = ConnectionPools.UserSettingsServicePool.Take())
            {
                connection.Item.SetDomain(mailParts[1]);

                foreach (string @delegate in UserSettingsRequestFactory.GetDelegates(mail))
                {
                    connection.Item.DeleteDelegate(mailParts[0], @delegate);
                }
            }
        }

        public static void AddDelegate(string mail, string @delegate)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            string[] mailParts = mail.Split('@');

            using (PoolItem<EmailSettingsService> connection = ConnectionPools.UserSettingsServicePool.Take())
            {
                connection.Item.SetDomain(mailParts[1]);

                connection.Item.CreateDelegate(mailParts[0], @delegate);
            }
        }

        public static void AddDelegate(string mail, IEnumerable<string> delegates)
        {
            if (mail.IndexOf("@", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException("Mail argument must be a valid email address");
            }

            string[] mailParts = mail.Split('@');

            using (PoolItem<EmailSettingsService> connection = ConnectionPools.UserSettingsServicePool.Take())
            {
                connection.Item.SetDomain(mailParts[1]);

                foreach (string @delegate in delegates)
                {
                    connection.Item.CreateDelegate(mailParts[0], @delegate);
                }
            }
        }
    }
}
