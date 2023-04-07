using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    [StructLayout(LayoutKind.Sequential, Size = 0x220)]
    struct NRRCertification
    {
        public ulong ApplicationIdMask;
        public ulong ApplicationIdPattern;
        private Array16<byte> _reserved;
        public ByteArray256 Modulus;
        public ByteArray256 Signature;
    }
}
