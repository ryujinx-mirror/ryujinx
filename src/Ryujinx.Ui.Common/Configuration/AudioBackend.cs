using Ryujinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Ryujinx.Ui.Common.Configuration
{
    [JsonConverter(typeof(TypedStringEnumConverter<AudioBackend>))]
    public enum AudioBackend
    {
        Dummy,
        OpenAl,
        SoundIo,
        SDL2,
    }
}
