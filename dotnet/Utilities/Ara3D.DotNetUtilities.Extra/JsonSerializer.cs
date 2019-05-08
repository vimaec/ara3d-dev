using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ara3D.DotNetUtilities.Extra
{
    public class WrappedJsonSerializer : JsonSerializer, IJsonSerializer {}

    public static class JsonSerializationExtensions
    {
        public static JObject ToJObject(this object o)
            => JObject.FromObject(o);

        public static JArray ToJArray<T>(this IEnumerable<T> xs)
            => xs.ToList().ToJArray();

        public static JArray ToJArray<T>(this IList<T> xs)
            => JArray.FromObject(xs);

        public static string ToJson(this object o)
            => o?.ToJObject()?.ToString() ?? "null";

        public static void ToJsonFile(this object o, string filePath)
        {
            new WrappedJsonSerializer
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }.ToJsonFile(o, filePath);
        }

        public static string PrettifyJson(this string s)
        {
            return JToken.Parse(s).ToString();
        }

        public static JObject LoadJson(string filePath)
            => JObject.Parse(File.ReadAllText(filePath));

        public static JArray LoadJsonArray(string filePath)
            => JArray.Parse(File.ReadAllText(filePath));
    }
}
