using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ryujinx.Ui.Common.App
{
    [JsonSerializable(typeof(IEnumerable<LdnGameData>))]
    internal partial class LdnGameDataSerializerContext : JsonSerializerContext
    {

    }
}
