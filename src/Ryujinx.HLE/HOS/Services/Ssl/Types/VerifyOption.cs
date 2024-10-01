using System;

namespace Ryujinx.HLE.HOS.Services.Ssl.Types
{
    [Flags]
    enum VerifyOption : uint
    {
        PeerCa = 1 << 0,
        HostName = 1 << 1,
        DateCheck = 1 << 2,
        EvCertPartial = 1 << 3,
        EvPolicyOid = 1 << 4, // 6.0.0+
        EvCertFingerprint = 1 << 5, // 6.0.0+
    }
}
