using System;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [Flags]
    enum ScanFilterFlag : byte
    {
        LocalCommunicationId = 1 << 0,
        SessionId = 1 << 1,
        NetworkType = 1 << 2,
        MacAddress = 1 << 3,
        Ssid = 1 << 4,
        SceneId = 1 << 5,
        IntentId = LocalCommunicationId | SceneId,
        NetworkId = IntentId | SessionId,
        All = NetworkType | IntentId | SessionId | MacAddress | Ssid,
    }
}
