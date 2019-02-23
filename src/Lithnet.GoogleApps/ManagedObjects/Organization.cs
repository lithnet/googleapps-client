using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Organization : CustomTypeObject, IPrimaryCandidateObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "unknown", "work", "school", "unknown" };

        [JsonProperty("name"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Name { get; set; }

        [JsonProperty("title"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Title { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonProperty("department"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Department { get; set; }

        [JsonProperty("symbol"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Symbol { get; set; }

        [JsonProperty("location"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Location { get; set; }

        [JsonProperty("description"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Description { get; set; }

        [JsonProperty("domain"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Domain { get; set; }

        [JsonProperty("costCenter"), JsonConverter(typeof(JsonNullStringConverter))]
        public string CostCenter { get; set; }

        [JsonIgnore]
        public bool IsPrimary => this.Primary ?? false;

        public override bool IsEmpty()
        {
            return
                this.CostCenter.IsNullOrNullPlaceholder() &&
                this.Department.IsNullOrNullPlaceholder() &&
                this.Description.IsNullOrNullPlaceholder() &&
                this.Domain.IsNullOrNullPlaceholder() &&
                this.Location.IsNullOrNullPlaceholder() &&
                this.Name.IsNullOrNullPlaceholder() &&
                this.Symbol.IsNullOrNullPlaceholder() &&
                this.Title.IsNullOrNullPlaceholder();
        }
    }
}
