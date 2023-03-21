#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Utilities
{
    /// <summary>
    /// Specifies that value of <see cref="TEnum"/> will be serialized as string in JSONs
    /// </summary>
    /// <remarks>
    /// Trimming friendly alternative to <see cref="JsonStringEnumConverter"/>.
    /// Get rid of this converter if dotnet supports similar functionality out of the box.
    /// </remarks>
    /// <typeparam name="TEnum">Type of enum to serialize</typeparam>
    public sealed class TypedStringEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var enumValue = reader.GetString();
            if (string.IsNullOrEmpty(enumValue))
            {
                return default;
            }

            return Enum.Parse<TEnum>(enumValue);
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
