using System.Linq;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class IM : CustomTypeObject, IPrimaryCandidateObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "home", "work", "other" };

        private static string[] standardProtocols = { "aim", "gtalk", "icq", "jabber", "msn", "net_meeting", "qq", "skype", "yahoo" };

        [JsonProperty("im"), JsonConverter(typeof(JsonNullStringConverter))]
        public string IMAddress { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonProperty("protocol"), JsonConverter(typeof(JsonNullStringConverter))]
        private string ProtocolRaw { get; set; }

        [JsonProperty("customProtocol"), JsonConverter(typeof(JsonNullStringConverter))]
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
                if (IM.standardProtocols.Contains(value))
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
        public bool IsPrimary => this.Primary ?? false;

        public override bool IsEmpty()
        {
            return this.IMAddress.IsNullOrNullPlaceholder();
        }
    }
}
