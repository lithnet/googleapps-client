using Newtonsoft.Json;
using System;
using System.IO;
using Google.Apis.Json;

namespace Lithnet.GoogleApps
{
    public class GoogleJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializer newtonsoftSerializer;

        public GoogleJsonSerializer()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            settings.Converters.Add(new RFC3339DateTimeConverter());
            settings.Converters.Add(new EmptyListConverter());
            settings.Converters.Add(new NotesConverter());
            settings.Converters.Add(new JsonNullStringConverter());
            this.newtonsoftSerializer = JsonSerializer.Create(settings);
        }

        public NullValueHandling NullValueHandling
        {
            get => this.newtonsoftSerializer.NullValueHandling;
            set => this.newtonsoftSerializer.NullValueHandling = value;
        }

        public T Deserialize<T>(Stream input)
        {
            using (StreamReader reader = new StreamReader(input))
            {
                return (T)this.newtonsoftSerializer.Deserialize(reader, typeof(T));
            }
        }

        public T Deserialize<T>(string input)
        {
            return string.IsNullOrEmpty(input) ? default(T) : JsonConvert.DeserializeObject<T>(input);
        }

        public object Deserialize(string input, Type type)
        {
            return string.IsNullOrEmpty(input) ? null : JsonConvert.DeserializeObject(input, type);
        }

        public string Serialize(object obj)
        {
            using (TextWriter writer = new StringWriter())
            {
                if (obj == null)
                {
                    obj = string.Empty;
                }

                this.newtonsoftSerializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public void Serialize(object obj, Stream target)
        {
            using (StreamWriter writer = new StreamWriter(target))
            {
                if (obj == null)
                {
                    obj = string.Empty;
                }

                this.newtonsoftSerializer.Serialize(writer, obj);
            }
        }

        public string Format => "json";
    }
}