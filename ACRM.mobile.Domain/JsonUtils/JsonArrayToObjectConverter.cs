using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ACRM.mobile.Domain.JsonUtils
{
	public class JsonArrayIndexAttribute : Attribute
	{
		public int Index { get; private set; }
		public JsonArrayIndexAttribute(int index)
		{
			Index = index;
		}
	}

	public class JsonArrayToObjectConverter<T> : JsonConverter where T : class, new()
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(T);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			try
			{
				T target = new T();
				JArray array = JArray.Load(reader);

				var propsByIndex = typeof(T).GetProperties()
					.Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<JsonArrayIndexAttribute>() != null)
					.ToDictionary(p => p.GetCustomAttribute<JsonArrayIndexAttribute>().Index);

				JObject obj = new JObject(array
					.Select((jt, i) =>
					{
						PropertyInfo prop;
						return propsByIndex.TryGetValue(i, out prop) ? new JProperty(prop.Name, jt) : null;
					})
					.Where(jp => jp != null)
				);
				serializer.Populate(obj.CreateReader(), target);
				return target;
			}
			catch (Exception e)
			{
				Debug.WriteLine($"{ e.GetType().Name + " : " + e.Message}");
				return null;
			}
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
            {
				writer.WriteNull();
            }
			else
			{
				JArray arr = new JArray();
				var propsByIndex = typeof(T).GetProperties()
					.Where(p => p.CanRead && p.CanWrite && p.GetCustomAttribute<JsonArrayIndexAttribute>() != null)
					.OrderBy(p => p.GetCustomAttribute<JsonArrayIndexAttribute>().Index);

				foreach (var p in propsByIndex)
				{
					var v = value.GetType().GetProperty(p.Name).GetValue(value);
					arr.Add(JToken.FromObject(v));
				}

				arr.WriteTo(writer);
			}
		}
	}
}
