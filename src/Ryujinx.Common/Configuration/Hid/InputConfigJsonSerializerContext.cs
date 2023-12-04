using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Common.Configuration.Hid.Keyboard;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Configuration.Hid
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(InputConfig))]
    [JsonSerializable(typeof(StandardKeyboardInputConfig))]
    [JsonSerializable(typeof(StandardControllerInputConfig))]
    public partial class InputConfigJsonSerializerContext : JsonSerializerContext
    {
    }
}
