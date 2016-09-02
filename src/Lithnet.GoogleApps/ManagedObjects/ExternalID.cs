using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class ExternalID : CustomTypeObject
    {
        [JsonProperty("value"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Value { get; set; }

        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "account", "customer", "network", "organization" };

        public override bool IsEmpty()
        {
            return this.Value.IsNullOrNullPlaceholder();
        }

        public override string ToString()
        {
            return $"{this.Type}:{this.Value}";
        }
    }
}


