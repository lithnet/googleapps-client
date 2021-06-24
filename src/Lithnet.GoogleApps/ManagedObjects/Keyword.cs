using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Keyword : CustomTypeObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "mission", "occupation", "outlook" };

        [JsonProperty("value"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Value { get; set; }

        public override bool IsEmpty()
        {
            return
                this.Value.IsNullOrNullPlaceholder();
             
        }
    }
}
