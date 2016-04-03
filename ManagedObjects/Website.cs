using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Website : CustomTypeObject, IPrimaryCandidateObject, IIsEmptyObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "app_install_page", "blog", "ftp", "home", "home_page", "other", "profile", "reservations", "work" };
            }
        }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonIgnore]
        public bool IsPrimary
        {
            get
            {
                return this.Primary != null ? this.Primary.Value : false;
            }
        }

        public bool IsEmpty()
        {
            return this.Value.IsNullOrNullPlaceholder();
        }
    }
}
