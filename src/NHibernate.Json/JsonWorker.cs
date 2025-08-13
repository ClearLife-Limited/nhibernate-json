namespace NHibernate.Json
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class JsonWorker
    {
        private static JsonSerializerOptions _options;
        private static readonly object _optionsLock = new object();

        public static JsonSerializerOptions Options => _options;

        static JsonWorker()
        {
            _options = CreateBaseOptions();
        }

        private static JsonSerializerOptions CreateBaseOptions()
        {
            var opts = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
            return opts;
        }

        public static JsonSerializerOptions Configure(params Action<JsonSerializerOptions>[] actions)
        {
            if (actions == null || actions.Length == 0) return _options;

            // Clone current options to allow mutation even if previously used (previous instance may be frozen).
            var newOptions = new JsonSerializerOptions(_options);
            foreach (var action in actions)
            {
                action?.Invoke(newOptions);
            }
            lock (_optionsLock)
            {
                _options = newOptions;
            }
            return _options;
        }

        public static string Serialize(object obj)
        {
            var opts = _options;
            return JsonSerializer.Serialize(obj, opts);
        }

        public static T? Deserialize<T>(string json)
        {
            var opts = _options;
            return JsonSerializer.Deserialize<T>(json, opts);
        }

        /// <summary>
        /// Mimics Newtonsoft.Json's PopulateObject by deserializing then copying writable property values.
        /// Note: This is a shallow copy for writable public properties.
        /// </summary>
        public static void PopulateObject<T>(string json, T target) where T : class
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            var opts = _options;
            var deserialized = JsonSerializer.Deserialize<T>(json, opts);
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
