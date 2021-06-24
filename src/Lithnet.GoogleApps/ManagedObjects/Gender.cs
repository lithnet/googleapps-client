namespace Lithnet.GoogleApps.ManagedObjects
{
    using Google.Apis.Requests;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System;

    [Serializable]
    public class Gender : ISerializable
    {
        public List<string> KnownTypes = new List<string>() { "female", "male", "unknown" };

        public string AddressMeAs { get; set; }

        public string CustomGender { get; set; }

        public string Type { get; set; }

        public string GenderValue { get => this.GetGender(); set => this.SetGender(value); }

        public Gender()
        {
        }

        protected Gender(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry entry in info)
            {
                switch (entry.Name)
                {
                    case "addressMeAs":
                        this.AddressMeAs = info.GetString(entry.Name);
                        break;

                    case "customGender":
                        this.CustomGender = info.GetString(entry.Name);
                        break;

                    case "type":
                        this.Type = info.GetString(entry.Name);
                        break;

                    default:
                        break;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("addressMeAs", this.AddressMeAs);
            info.AddValue("customGender", this.CustomGender);
            info.AddValue("type", this.Type);
        }

        private string GetGender()
        {
            if (this.Type == "other")
            {
                return this.CustomGender;
            }
            else
            {
                return this.Type;
            }
        }
        private void SetGender(string gender)
        {
            if (this.KnownTypes.Contains(gender.ToLowerInvariant()))
            {
                this.CustomGender = null;
                this.Type = gender;
            }
            else
            {
                this.Type = "other";
                this.CustomGender = gender;
            }
        }
    }
}