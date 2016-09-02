using System.Linq;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public abstract class CustomTypeObject : IIsEmptyObject
    {
        protected abstract string[] StandardTypes { get; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        private string TypeRaw { get; set; }

        [JsonProperty("customType", NullValueHandling = NullValueHandling.Include)]
        private string CustomTypeRaw { get; set; }

        [JsonIgnore]
        public string Type
        {
            get
            {
                if (this.TypeRaw == "custom")
                {
                    return this.CustomTypeRaw;
                }
                else
                {
                    return this.TypeRaw;
                }
            }
            set
            {
                if (this.StandardTypes.Contains(value))
                {
                    this.TypeRaw = value;
                    this.CustomTypeRaw = null;
                }
                else
                {
                    this.TypeRaw = "custom";
                    this.CustomTypeRaw = value;
                }
            }
        }

        public abstract bool IsEmpty();
    }
}
