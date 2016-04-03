using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
