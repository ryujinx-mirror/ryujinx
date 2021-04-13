using System;

namespace Ryujinx.HLE.HOS.Services.Ssl.Types
{
    [Flags]
    enum SslVersion : uint
    {
        Auto   = 1 << 0,
        TlsV10 = 1 << 3,
        TlsV11 = 1 << 4,
        TlsV12 = 1 << 5,
        TlsV13 = 1 << 6, // 11.0.0+
        Auto2  = 1 << 24 // 11.0.0+
    }
}