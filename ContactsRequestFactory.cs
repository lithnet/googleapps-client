using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lithnet.GoogleApps
{
    using Google.GData.Apps;
    using Google.GData.Contacts;
    using Google.GData.Client;
    using Google.GData.Extensions.Apps;

    public static class ContactsRequestFactory
    {
        public static IEnumerable<ContactEntry> GetContacts(string domain)
        {
            using (GDataServiceWrapper<ContactsService> connection = ConnectionPools.ContactsServicePool.Take(""))
            {
                string uri = ContactsQuery.CreateContactsUri(domain);

                do
                {
                    ContactsQuery request = new ContactsQuery(uri)
                    {
                        NumberToRetrieve = 1000
                    
                    };

                    ContactsFeed x = connection.Client.Query(request);

                    foreach (ContactEntry entry in x.Entries.OfType<ContactEntry>())
                    {
                        yield return entry;
                    }

                    uri = x.NextChunk;

                } while (uri != null);

            }
        }
    }
}
