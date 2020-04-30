using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Utilities
{
    public class JsonHelper
    {
        public static JsonNamingPolicy SnakeCase { get; }

        private class SnakeCaseNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return name;
                }

                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < name.Length; i++)
                {
                    char c = name[i];

                    if (char.IsUpper(c))
                    {
                        if (i == 0 || char.IsUpper(name[i - 1]))
                        {
                            builder.Append(char.ToLowerInvariant(c));
                        }
                        else
                        {
                            builder.Append("_");
                            builder.Append(char.ToLowerInvariant(c));
                        }
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }

                return builder.ToString();
            }
        }

        static JsonHelper()
        {
            SnakeCase = new SnakeCaseNamingPolicy();
        }

        public static JsonSerializerOptions GetDefaultSerializerOptions(bool prettyPrint = false)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                DictionaryKeyPolicy  = SnakeCase,
                PropertyNamingPolicy = SnakeCase,
                WriteIndented        = prettyPrint,
                AllowTrailingCommas  = true,
                ReadCommentHandling  = JsonCommentHandling.Skip
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }

        public static T Deserialize<T>(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return JsonSerializer.Deserialize<T>(reader.ReadBytes((int)(stream.Length - stream.Position)), GetDefaultSerializerOptions());
            }
        }

        public static T DeserializeFromFile<T>(string path)
        {
            return Deserialize<T>(File.ReadAllText(path));
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, GetDefaultSerializerOptions());
        }

        public static void Serialize<TValue>(Stream stream, TValue obj, bool prettyPrint = false)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(SerializeToUtf8Bytes(obj, prettyPrint));
            }
        }

        public static string Serialize<TValue>(TValue obj, bool prettyPrint = false)
        {
            return JsonSerializer.Serialize(obj, GetDefaultSerializerOptions(prettyPrint));
        }

        public static byte[] SerializeToUtf8Bytes<T>(T obj, bool prettyPrint = false)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj, GetDefaultSerializerOptions(prettyPrint));
        }
    }
}
