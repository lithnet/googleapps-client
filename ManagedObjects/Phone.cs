using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Phone : CustomTypeObject, IPrimaryCandidateObject, IIsEmptyObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "home", "work", "other", "home_fax", "work_fax", "mobile", "pager", "other_fax", "compain_main", "assistant", "car", "radio", "isdn", "callback", "telex", "tty_tdd", "work_mobile", "work_pager", "main", "grand_central" };
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

        public override string ToString()
        {
            string format = this.IsPrimary ? "{0}:{1} (primary)" : "{0}:{1}";
            return string.Format(format, this.Type, this.Value);
        }
    }
}
