using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Requests;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class DomainAlias : IDirectResponseSchema
    {
        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("domainAliasName")]
        public string DomainAliasName { get; set; }

        [JsonProperty("parentAliasName")]
        public string ParentDomainName { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("creationTime")]
        public long CreationTime { get; set; }

        public override string ToString()
        {
            return this.DomainAliasName;
        }
    }
}
