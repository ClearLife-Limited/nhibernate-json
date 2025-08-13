namespace NHibernate.Json
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class JsonWorker
    {
        public static readonly JsonSerializerOptions Options;

        static JsonWorker()
        {
            Options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
        }

        public static JsonSerializerOptions Configure(params Action<JsonSerializerOptions>[] actions)
        {
            if (actions == null || actions.Length == 0) return Options;
            foreach (var action in actions)
            {
                action?.Invoke(Options);
            }
            return Options;
        }

        public static string Serialize(object obj) => JsonSerializer.Serialize(obj, Options);

        public static T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);

        /// <summary>
        /// Mimics Newtonsoft.Json's PopulateObject by deserializing then copying writable property values.
        /// Note: This is a shallow copy for writable public properties.
        /// </summary>
        public static void PopulateObject<T>(string json, T target) where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            var deserialized = JsonSerializer.Deserialize<T>(json, Options);
            if (deserialized == null) return;

            var targetType = target.GetType();
            foreach (var prop in targetType.GetProperties().Where(p => p.CanWrite))
            {
                try
                {
                    var value = prop.GetValue(deserialized);
                    prop.SetValue(target, value);
                }
                catch
                {
                    // Intentionally swallow to mirror lenient population behavior.
                }
            }
        }
    }
}
