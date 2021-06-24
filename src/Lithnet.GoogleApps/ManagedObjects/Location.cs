using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Location : CustomTypeObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "default", "desk" };

        [JsonProperty("area"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Area { get; set; }

        [JsonProperty("buildingId"), JsonConverter(typeof(JsonNullStringConverter))]
        public string BuildingId { get; set; }

        [JsonProperty("deskCode"), JsonConverter(typeof(JsonNullStringConverter))]
        public string DeskCode { get; set; }

        [JsonProperty("floorName"), JsonConverter(typeof(JsonNullStringConverter))]
        public string FloorName { get; set; }

        [JsonProperty("floorSection"), JsonConverter(typeof(JsonNullStringConverter))]
        public string FloorSection { get; set; }

        public override bool IsEmpty()
        {
            return
                this.Area.IsNullOrNullPlaceholder() &&
                this.BuildingId.IsNullOrNullPlaceholder() &&
                this.DeskCode.IsNullOrNullPlaceholder() &&
                this.FloorName.IsNullOrNullPlaceholder() &&
                this.FloorSection.IsNullOrNullPlaceholder();
             
        }
    }
}
