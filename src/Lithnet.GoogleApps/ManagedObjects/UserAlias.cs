using Google.Apis.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class UserAlias : IDirectResponseSchema, IIsEmptyObject
    {
        [JsonProperty("alias")]
        public string AliasValue { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("primaryEmail")]
        public string PrimaryEmail { get; set; }

        public bool IsEmpty()
        {
            return this.AliasValue.IsNullOrNullPlaceholder();
        }
    }
}
