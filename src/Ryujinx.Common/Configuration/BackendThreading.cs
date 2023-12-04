using Ryujinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Configuration
{
    [JsonConverter(typeof(TypedStringEnumConverter<BackendThreading>))]
    public enum BackendThreading
    {
        Auto,
        Off,
        On,
    }
}
