using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Lithnet.GoogleApps;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Address : CustomTypeObject, IPrimaryCandidateObject, IIsEmptyObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "home", "work", "other" };
            }
        }

        [JsonProperty("sourceIsStructured")]
        public bool? SourceIsStructured { get; set; }

        [JsonProperty("formatted")]
        public string Formatted { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonProperty("poBox")]
        public string POBox { get; set; }

        [JsonProperty("extendedAddress")]
        public string ExtendedAddress { get; set; }

        [JsonProperty("streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty("locality")]
        public string Locality { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonIgnore]
        public bool IsPrimary
        {
            get
            {
                return this.Primary != null ? this.Primary.Value : false;
            }
        }

        public bool IsEmpty()
        {
            return 
                this.Country.IsNullOrNullPlaceholder() &&
                this.CountryCode.IsNullOrNullPlaceholder() &&
                this.ExtendedAddress.IsNullOrNullPlaceholder() &&
                this.Formatted.IsNullOrNullPlaceholder() &&
                this.Locality.IsNullOrNullPlaceholder() &&
                this.POBox.IsNullOrNullPlaceholder() &&
                this.PostalCode.IsNullOrNullPlaceholder() &&
                this.Region.IsNullOrNullPlaceholder() &&
                this.StreetAddress.IsNullOrNullPlaceholder() &&
                this.Type.IsNullOrNullPlaceholder();
        }

        public override string ToString()
        {
            return string.Format("{0}", this.Type);
        }
    }
}
