namespace Lithnet.GoogleApps.ManagedObjects
{
    using Google.Apis.Requests;
    using Newtonsoft.Json;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public class UserName : IDirectResponseSchema, ISerializable
    {
        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; private set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        public UserName()
        {
        }

        protected UserName(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry entry in info)
            {
                switch (entry.Name)
                {
                    case "familyName":
                        this.FamilyName = info.GetString(entry.Name);
                        break;

                    case "givenName":
                        this.GivenName = info.GetString(entry.Name);
                        break;

                    case "fullName":
                        this.FullName = info.GetString(entry.Name);
                        break;

                    case "etag":
                        this.ETag = info.GetString(entry.Name);
                        break;
                    default:
                        break;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("familyName", this.FamilyName);
            info.AddValue("givenName", this.GivenName);
            info.AddValue("fullName", string.Format("{0} {1}", this.GivenName, this.FamilyName));
        }

        public override string ToString()
        {
            return this.FamilyName;
        }
    }
}