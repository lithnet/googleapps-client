using System.Collections.Generic;
using Google.Apis.Requests;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class DomainList : IDirectResponseSchema
    {
        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("domains")]
        public List<Domain> Domains { get; set; }
    }
}
