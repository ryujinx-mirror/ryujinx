using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    [StructLayout(LayoutKind.Sequential, Size = 0x350)]
    struct NrrHeader
    {
        public uint Magic;
        public uint KeyGeneration; // 9.0.0+
        private Array8<byte> _reserved;
        public NRRCertification Certification;
        public ByteArray256 Signature;
        public ulong TitleId;
        public uint Size;
        public byte Kind; // 7.0.0+
        private Array3<byte> _reserved2;
        public uint HashesOffset;
        public uint HashesCount;
        private Array8<byte> _reserved3;
    }
}
