using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Email : CustomTypeObject, IPrimaryCandidateObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "home", "work", "other" };

        [JsonProperty("address"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Address { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }
      
        [JsonIgnore]
        public bool IsPrimary => this.Primary ?? false;

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
