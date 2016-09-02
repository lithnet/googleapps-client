using Google.Apis.Requests;
using Newtonsoft.Json;
    
namespace Lithnet.GoogleApps.ManagedObjects
{
    public class UserUndeleteRequestParameters : IDirectResponseSchema
    {
        public UserUndeleteRequestParameters()
        {
        }

        public UserUndeleteRequestParameters(string orgUnitPath)
        {
            this.OrgUnitPath = orgUnitPath;
        }

        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; set; }

        [JsonProperty("orgUnitPath")]
        public string OrgUnitPath { get; set; }
    }
}
