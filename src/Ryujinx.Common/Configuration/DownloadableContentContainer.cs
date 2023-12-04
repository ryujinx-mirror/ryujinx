using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ryujinx.Common.Configuration
{
    public struct DownloadableContentContainer
    {
        [JsonPropertyName("path")]
        public string ContainerPath { get; set; }
        [JsonPropertyName("dlc_nca_list")]
        public List<DownloadableContentNca> DownloadableContentNcaList { get; set; }
    }
}
