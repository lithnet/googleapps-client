using System;
using System.Linq;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public abstract class TypeObject : IIsEmptyObject
    {
        protected abstract string[] StandardTypes { get; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        protected string TypeRaw { get; set; }

        [JsonIgnore]
        public virtual string Type
        {
            get => this.TypeRaw;
            set
            {
                if (this.StandardTypes.Contains(value))
                {
                    this.TypeRaw = value;
                }
                else
                {
                    throw new ArgumentException("Type {value} is not a supported type for this object");
                }
            }
        }

        public abstract bool IsEmpty();
    }
}
