using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Notes : IIsEmptyObject
    {
        [JsonProperty("value"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Value { get; set; }

        [JsonProperty("contentType", NullValueHandling=NullValueHandling.Ignore)]
        private string ContentTypeRaw { get; set; }

        [JsonIgnore]
        public string ContentType
        {
            get
            {
                return this.ContentTypeRaw ?? "text_plain";
            }
            set
            {
                this.ContentTypeRaw = value;
            }
        }

        public bool IsEmpty()
        {
            return this.Value.IsNullOrNullPlaceholder();
        }
    }
}
