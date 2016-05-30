using Google.Apis.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class MakeAdminParameters : IDirectResponseSchema
    {
        [JsonProperty("status")]
        public virtual bool? Status { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }
    }
}
