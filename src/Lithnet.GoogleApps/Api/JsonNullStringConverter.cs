using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Lithnet.GoogleApps
{
    public class JsonNullStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == (typeof(string));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string stringValue = (string)value;
            if (stringValue == Constants.NullValuePlaceholder)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(stringValue);
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
    }
}
