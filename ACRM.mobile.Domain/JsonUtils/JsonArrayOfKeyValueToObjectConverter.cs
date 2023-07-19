using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ACRM.mobile.Domain.JsonUtils
{
    public class JsonArrayOfKeyValueToObjectConverter<T> : JsonConverter where T : class, new()
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);

            var props = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .ToDictionary(p => p.Name.ToLower());

            JObject obj = new JObject(array
                .Select((jt, i) =>
                {
                    PropertyInfo prop;
                    var arrayPropertyInfo = jt[0].ToString().ToLower();
                    return props.TryGetValue(arrayPropertyInfo, out prop) ? new JProperty(prop.Name, jt[1]) : null;
                })
                .Where(jp => jp != null)
            );

            T target = new T();
            serializer.Populate(obj.CreateReader(), target);

            return target;
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray arr = new JArray();
            var props = typeof(T).GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();

            foreach (var p in props)
            {
                var v = value.GetType().GetProperty(p.Name).GetValue(value);
                JArray valArr = new JArray();
                valArr.Add(JToken.FromObject(p.Name));
                valArr.Add(JToken.FromObject(v));
                arr.Add(valArr);
            }

            arr.WriteTo(writer);
        }

    }
}
