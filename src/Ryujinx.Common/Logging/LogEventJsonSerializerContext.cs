using System.Text.Json.Serialization;

namespace Ryujinx.Common.Logging
{
    [JsonSerializable(typeof(LogEventArgsJson))]
    internal partial class LogEventJsonSerializerContext : JsonSerializerContext
    {
    }
}
