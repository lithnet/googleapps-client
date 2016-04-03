using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class ExternalID : CustomTypeObject, IIsEmptyObject
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "account", "customer", "network", "organization" };
            }
        }

        public bool IsEmpty()
        {
            return this.Value.IsNullOrNullPlaceholder();
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.Type, this.Value);
        }
    }
}


