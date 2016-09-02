using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Requests;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Domain : IDirectResponseSchema
    {
        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("kind", NullValueHandling = NullValueHandling.Ignore)]
        private string Kind { get; set; }

        [JsonProperty("domainName")]
        public string DomainName { get; set; }

        [JsonProperty("domainAliases")]
        public List<DomainAlias> DomainAliases { get; set; }

        [JsonProperty("isPrimary")]
        public bool IsPrimary { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("creationTime")]
        private long CreationTimeRaw { get; set; }

        [JsonIgnore]
        public IReadOnlyCollection<string> DomainAliasNames
        {
            get
            {
                return this.DomainAliases?.Select(t => t.DomainAliasName).ToList().AsReadOnly();
            }
        }

        [JsonIgnore]
        public DateTime? CreationTime
        {
            get
            {
                if (this.CreationTimeRaw == 0)
                {
                    return null;
                }
                else
                {
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    return origin.AddMilliseconds(this.CreationTimeRaw);
                }
            }
        }

        public override string ToString()
        {
            return this.DomainName;
        }
    }
}
