using Google.Apis.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class UserAliases : IDirectResponseSchema
    {
        [JsonProperty("aliases")]
        public IList<UserAlias> AliasesValue { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }
    }
}
