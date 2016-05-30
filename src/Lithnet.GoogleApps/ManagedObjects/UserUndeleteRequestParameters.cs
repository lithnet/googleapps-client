using Google.Apis.Requests;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
    
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
        public virtual string ETag { get; set; }

        [JsonProperty("orgUnitPath")]
        public virtual string OrgUnitPath { get; set; }
    }
}
