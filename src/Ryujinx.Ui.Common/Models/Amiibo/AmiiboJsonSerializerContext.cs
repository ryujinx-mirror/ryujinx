using System.Text.Json.Serialization;

namespace Ryujinx.Ui.Common.Models.Amiibo
{
    [JsonSerializable(typeof(AmiiboJson))]
    public partial class AmiiboJsonSerializerContext : JsonSerializerContext
    {
    }
}
