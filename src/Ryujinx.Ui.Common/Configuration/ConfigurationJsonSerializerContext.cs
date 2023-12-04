using System.Text.Json.Serialization;

namespace Ryujinx.Ui.Common.Configuration
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ConfigurationFileFormat))]
    internal partial class ConfigurationJsonSerializerContext : JsonSerializerContext
    {
    }
}
