using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Relation : CustomTypeObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "assistant", "brother", "child", "domestic_partner", "father", "friend", "manager", "mother", "parent", "partner", "referred_by", "relative", "sister", "spouse" };
            }
        }

        [JsonProperty("value"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Value { get; set; }

        public override bool IsEmpty()
        {
            return this.Value.IsNullOrNullPlaceholder();
        }
    }
}
