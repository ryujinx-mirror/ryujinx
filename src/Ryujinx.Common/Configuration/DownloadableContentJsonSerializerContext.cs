using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Configuration
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(List<DownloadableContentContainer>))]
    public partial class DownloadableContentJsonSerializerContext : JsonSerializerContext
    {
    }
}
