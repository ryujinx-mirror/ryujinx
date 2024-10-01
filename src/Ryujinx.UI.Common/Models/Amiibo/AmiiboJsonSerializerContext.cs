using System.Text.Json.Serialization;

namespace Ryujinx.UI.Common.Models.Amiibo
{
    [JsonSerializable(typeof(AmiiboJson))]
    public partial class AmiiboJsonSerializerContext : JsonSerializerContext
    {
    }
}
