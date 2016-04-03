using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class Organization : CustomTypeObject, IPrimaryCandidateObject, IIsEmptyObject
    {
        [JsonIgnore]
        protected override string[] StandardTypes
        {
            get
            {

                return new string[] { "unknown", "work", "school", "unknown" };
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("primary")]
        public bool? Primary { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("costCenter")]
        public string CostCenter { get; set; }
     
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
                this.CostCenter.IsNullOrNullPlaceholder() &&
                this.Department.IsNullOrNullPlaceholder() &&
                this.Description.IsNullOrNullPlaceholder() &&
                this.Domain.IsNullOrNullPlaceholder() &&
                this.Location.IsNullOrNullPlaceholder() &&
                this.Name.IsNullOrNullPlaceholder() &&
                this.Symbol.IsNullOrNullPlaceholder() &&
                this.Title.IsNullOrNullPlaceholder() &&
                this.Type.IsNullOrNullPlaceholder();
        }
    }
}
