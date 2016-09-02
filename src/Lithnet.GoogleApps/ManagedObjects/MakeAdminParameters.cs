using Google.Apis.Requests;
using Newtonsoft.Json;

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
