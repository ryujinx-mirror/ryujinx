using Gdk;
using System;
using System.IO;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Ryujinx.Debugger.Profiler
{
    public class ProfilerConfiguration
    {
        public bool   Enabled    { get; private set; }
        public string DumpPath   { get; private set; }
        public float  UpdateRate { get; private set; }
        public int    MaxLevel   { get; private set; }
        public int    MaxFlags   { get; private set; }
        public float  History    { get; private set; }

        /// <summary>
        /// Loads a configuration file from disk
        /// </summary>
        /// <param name="path">The path to the JSON configuration file</param>
        public static ProfilerConfiguration Load(string path)
        {
            var resolver = CompositeResolver.Create(
                new[] { new ConfigurationEnumFormatter<Key>() },
                new[] { StandardResolver.AllowPrivateSnakeCase }
            );

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Profiler configuration file {path} not found");
            }

            using (Stream stream = File.OpenRead(path))
            {
                return JsonSerializer.Deserialize<ProfilerConfiguration>(stream, resolver);
            }
        }

        private class ConfigurationEnumFormatter<T> : IJsonFormatter<T>
            where T : struct
        {
            public void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
            {
                formatterResolver.GetFormatterWithVerify<string>()
                    .Serialize(ref writer, value.ToString(), formatterResolver);
            }

            public T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
            {
                if (reader.ReadIsNull())
                {
                    return default(T);
                }

                string enumName = formatterResolver.GetFormatterWithVerify<string>()
                    .Deserialize(ref reader, formatterResolver);

                if (Enum.TryParse<T>(enumName, out T result))
                {
                    return result;
                }

                return default(T);
            }
        }
    }
}
