using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Common.Configuration.Hid.Keyboard;
using Ryujinx.Common.Utilities;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Configuration.Hid
{
    public class JsonInputConfigConverter : JsonConverter<InputConfig>
    {
        private static readonly InputConfigJsonSerializerContext _serializerContext = new(JsonHelper.GetDefaultSerializerOptions());

        private static InputBackendType GetInputBackendType(ref Utf8JsonReader reader)
        {
            // Temporary reader to get the backend type
            Utf8JsonReader tempReader = reader;

            InputBackendType result = InputBackendType.Invalid;

            while (tempReader.Read())
            {
                // NOTE: We scan all properties ignoring the depth entirely on purpose.
                // The reason behind this is that we cannot track in a reliable way the depth of the object because Utf8JsonReader never emit the first TokenType == StartObject if the json start with an object.
                // As such, this code will try to parse very field named "backend" to the correct enum.
                if (tempReader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = tempReader.GetString();

                    if (propertyName.Equals("backend"))
                    {
                        tempReader.Read();

                        if (tempReader.TokenType == JsonTokenType.String)
                        {
                            string backendTypeRaw = tempReader.GetString();

                            if (!Enum.TryParse(backendTypeRaw, out result))
                            {
                                result = InputBackendType.Invalid;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public override InputConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            InputBackendType backendType = GetInputBackendType(ref reader);

            return backendType switch
            {
                InputBackendType.WindowKeyboard => JsonSerializer.Deserialize(ref reader, _serializerContext.StandardKeyboardInputConfig),
                InputBackendType.GamepadSDL2 => JsonSerializer.Deserialize(ref reader, _serializerContext.StandardControllerInputConfig),
                _ => throw new InvalidOperationException($"Unknown backend type {backendType}"),
            };
        }

        public override void Write(Utf8JsonWriter writer, InputConfig value, JsonSerializerOptions options)
        {
            switch (value.Backend)
            {
                case InputBackendType.WindowKeyboard:
                    JsonSerializer.Serialize(writer, value as StandardKeyboardInputConfig, _serializerContext.StandardKeyboardInputConfig);
                    break;
                case InputBackendType.GamepadSDL2:
                    JsonSerializer.Serialize(writer, value as StandardControllerInputConfig, _serializerContext.StandardControllerInputConfig);
                    break;
                default:
                    throw new ArgumentException($"Unknown backend type {value.Backend}");
            }
        }
    }
}
