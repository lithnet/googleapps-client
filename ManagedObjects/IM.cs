using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class IM : CustomTypeObject, IPrimaryCandidateObject, IIsEmptyObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "home", "work", "other" };
            }
        }

        private static string[] StandardProtocols = { "aim", "gtalk", "icq", "jabber", "msn", "net_meeting", "qq", "skype", "yahoo" };

        [JsonProperty("im")]
        public string IMAddress { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonProperty("protocol")]
        private string ProtocolRaw { get; set; }

        [JsonProperty("customProtocol")]
        private string CustomProtocolRaw { get; set; }

        [JsonIgnore]
        public string Protocol
        {
            get
            {
                if (this.ProtocolRaw == "custom_protocol")
                {
                    return this.CustomProtocolRaw;
                }
                else
                {
                    return this.ProtocolRaw;
                }
            }
            set
            {
                if (IM.StandardProtocols.Contains(value))
                {
                    this.ProtocolRaw = value;
                    this.CustomProtocolRaw = Constants.NullValuePlaceholder;
                }
                else
                {
                    this.ProtocolRaw = "custom_protocol";
                    this.CustomProtocolRaw = value;
                }
            }
        }

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
            return this.IMAddress.IsNullOrNullPlaceholder();
        }
    }
}
