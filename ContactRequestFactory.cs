using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Lithnet.GoogleApps
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using Api;
    using Google.Apis.Admin.Directory.directory_v1;
    using Google.Contacts;
    using Google.GData.Apps;
    using Google.GData.Contacts;
    using Google.GData.Client;
    using Google.GData.Extensions.Apps;
    using ManagedObjects;
    using Newtonsoft.Json;

    public static class ContactRequestFactory
    {
        public static void GetContacts(string domain,  BlockingCollection<object> importObjects)
        {
            foreach (ContactEntry entry in GetContacts(domain))
            {
                importObjects.Add(entry);
            }
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
                return (ContactEntry)connection.Item.Get(id);
            }
        }

        public static void Delete(string id)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                ContactEntry e = (ContactEntry)(connection.Item.Get(id));
                connection.Item.Delete(e);
            }
        }

        public static void Delete(ContactEntry c)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                connection.Item.Delete(c.EditUri.ToString());
            }
        }

        public static ContactEntry Update(ContactEntry c)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                return connection.Item.Update(c);
            }
        }

        public static ContactEntry Add(ContactEntry c, string domain)
        {
            using (PoolItem<ContactsService> connection = ConnectionPools.ContactsServicePool.Take())
            {
                return connection.Item.Insert($"https://www.google.com/m8/feeds/contacts/{domain}/full", c);
            }
        }
    }
}
