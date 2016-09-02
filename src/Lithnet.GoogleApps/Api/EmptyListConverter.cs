using System;
using Newtonsoft.Json;
using System.Collections;

namespace Lithnet.GoogleApps
{
    public class EmptyListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (typeof(IList)).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IList typedValue = (IList)value;
            if (typedValue.Count == 0)
            {
                writer.WriteNull();
            }
            else
            {
                JsonSerializer x = JsonSerializer.Create();
                x.Serialize(writer, value);
            }
        }

        public override bool CanRead => false;

        public override bool CanWrite => true;
    }
}
