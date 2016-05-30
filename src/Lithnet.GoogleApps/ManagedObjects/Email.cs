using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;


namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Email : CustomTypeObject, IPrimaryCandidateObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "home", "work", "other" };
            }
        }

        [JsonProperty("address"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Address { get; set; }

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

        public override bool IsEmpty()
        {
            return this.Address.IsNullOrNullPlaceholder();
        }

        public override string ToString()
        {
            string format = this.IsPrimary ? "{0}:{1} (primary)" : "{0}:{1}";
            return string.Format(format, this.Type, this.Address);
        }
    }
}
