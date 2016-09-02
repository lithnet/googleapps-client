using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Website : CustomTypeObject, IPrimaryCandidateObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes => new string[] { "app_install_page", "blog", "ftp", "home", "home_page", "other", "profile", "reservations", "work" };

        [JsonProperty("value"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Value { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonIgnore]
        public bool IsPrimary => this.Primary ?? false;

        public override bool IsEmpty()
        {
            return this.Value.IsNullOrNullPlaceholder();
        }
    }
}
