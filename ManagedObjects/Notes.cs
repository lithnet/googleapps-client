using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Notes : IIsEmptyObject
    {
        [JsonProperty("value")]
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
