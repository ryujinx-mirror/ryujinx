using System.Text.Json.Serialization;

namespace Ryujinx.UI.Common.Models.Amiibo
{
    public class AmiiboApiUsage
    {
        [JsonPropertyName("Usage")]
        public string Usage { get; set; }
        [JsonPropertyName("write")]
        public bool Write { get; set; }
    }
}
