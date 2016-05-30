using Google.Apis.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class UserList : IDirectResponseSchema
    {
        [JsonProperty("etag")]
        public virtual string ETag { get; set; }

        [JsonProperty("kind")]
        public virtual string Kind { get; set; }

        [JsonProperty("nextPageToken")]
        public virtual string NextPageToken { get; set; }

        [JsonProperty("trigger_event")]
        public virtual string TriggerEvent { get; set; }

        [JsonProperty("users")]
        public virtual IList<User> UsersValue { get; set; }
    }
}
