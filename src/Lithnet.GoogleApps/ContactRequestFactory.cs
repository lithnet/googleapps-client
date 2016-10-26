using Google.GData.Contacts;
using System.Collections.Generic;
using System.Linq;

namespace Lithnet.GoogleApps
{
    public static class ContactRequestFactory
    {
        private static string serviceName;

        static ContactRequestFactory()
        {
            ContactRequestFactory.serviceName = typeof(ContactsService).Name;
        }

        public static IEnumerable<ContactEntry> GetContacts(string domain)
        {

            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                string uri = ContactsQuery.CreateContactsUri(domain);
             
                do
                {
                    ContactsQuery request = new ContactsQuery(uri)
                    {
                        NumberToRetrieve = 1000
                    };

                    RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                    ContactsFeed x = connection.Item.Query(request);

                    foreach (ContactEntry entry in x.Entries.OfType<ContactEntry>())
                    {
                        yield return entry;
                    }

                    uri = x.NextChunk;

                } while (uri != null);

            }
        }

        public static ContactEntry GetContact(string id)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                return (ContactEntry)connection.Item.Get(id);
            }
        }

        public static void Delete(string id)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                ContactEntry e = (ContactEntry)(connection.Item.Get(id));
                RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                connection.Item.Delete(e);
            }
        }

        public static void Delete(ContactEntry c)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                connection.Item.Delete(c.EditUri.ToString());
            }
        }

        public static ContactEntry Update(ContactEntry c)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                return connection.Item.Update(c);
            }
        }

        public static ContactEntry Add(ContactEntry c, string domain)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                RateLimiter.GetOrCreateBucket(ContactRequestFactory.serviceName).Consume();
                return connection.Item.Insert($"https://www.google.com/m8/feeds/contacts/{domain}/full", c);
            }
        }
    }
}
