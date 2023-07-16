using Ryujinx.Common.Utilities;
using System.Text.Json.Serialization;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    [JsonConverter(typeof(TypedStringEnumConverter<AccountState>))]
    public enum AccountState
    {
        Closed,
        Open,
    }
}
