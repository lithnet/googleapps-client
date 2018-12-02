using System;
using Google.GData.Contacts;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;

namespace Lithnet.GoogleApps
{
    public class ContactRequestFactory
    {
        private readonly Pool<ContactsService> contactsServicePool;

        private readonly string serviceName = "ContactRequestFactory";

        public ContactRequestFactory(GoogleServiceCredentials creds, string[] scopes, int poolSize = 30)
        {
            ServiceAccountCredential credentials = new ServiceAccountCredential(creds.GetInitializer(scopes));

            this.contactsServicePool = new Pool<ContactsService>(poolSize, () =>
            {
                ContactsService service = new ContactsService("Lithnet.GoogleApps");
                OAuthGDataRequestFactory requestFactory = new OAuthGDataRequestFactory("Lithnet.GoogleApps", credentials);
                requestFactory.CustomHeaders.Add("GData-Version: 3.0");
                requestFactory.UseGZip = !Settings.DisableGzip;
                service.RequestFactory = requestFactory;
                return service;
            });

        }

        public IEnumerable<ContactEntry> GetContacts(string domain)
        {
            using (PoolItem<ContactsService> connection = this.contactsServicePool.Take())
            {
                string uri = ContactsQuery.CreateContactsUri(domain);

                do
                {
                    ContactsQuery request = new ContactsQuery(uri)
                    {
                        NumberToRetrieve = 1000
                    };

                    ContactsFeed result = ApiExtensions.InvokeWithRateLimit(() => connection.Item.Query(request), this.serviceName);

                    foreach (ContactEntry entry in result.Entries.OfType<ContactEntry>())
                    {
                        yield return entry;
                    }

                    uri = result.NextChunk;

                } while (uri != null);

            }
        }

        public ContactEntry GetContact(string id)
        {
            using (PoolItem<ContactsService> connection = this.contactsServicePool.Take())
            {
                return ApiExtensions.InvokeWithRateLimit(() => (ContactEntry)connection.Item.Get(id), this.serviceName);
            }
        }

        public void Delete(string id)
        {
            using (PoolItem<ContactsService> connection = this.contactsServicePool.Take())
            {
                ContactEntry e = ApiExtensions.InvokeWithRateLimit(() => (ContactEntry)connection.Item.Get(id), this.serviceName);
                this.Delete(e);
            }
        }

        public void Delete(ContactEntry c)
        {
            using (PoolItem<ContactsService> connection = this.contactsServicePool.Take())
            {
                ApiExtensions.InvokeWithRateLimit(() => connection.Item.Delete(c), this.serviceName);
            }
        }

        public ContactEntry Update(ContactEntry c)
        {
            using (PoolItem<ContactsService> connection = this.contactsServicePool.Take())
            {
                return ApiExtensions.InvokeWithRateLimit(() => connection.Item.Update(c), this.serviceName);
            }
        }

        public ContactEntry Add(ContactEntry c, string domain)
        {
            using (PoolItem<ContactsService> connection = this.contactsServicePool.Take())
            {
                return ApiExtensions.InvokeWithRateLimit(() => connection.Item.Insert($"https://www.google.com/m8/feeds/contacts/{domain}/full", c), this.serviceName);
            }
        }
    }
}
