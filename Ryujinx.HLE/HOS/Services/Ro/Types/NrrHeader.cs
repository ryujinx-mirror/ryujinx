using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    [StructLayout(LayoutKind.Explicit, Size = 0x350)]
    unsafe struct NrrHeader
    {
        [FieldOffset(0)]
        public uint Magic;

        [FieldOffset(0x4)]
        public uint CertificationSignatureKeyGeneration; // 9.0.0+

        [FieldOffset(0x8)]
        public ulong Reserved;

        [FieldOffset(0x10)]
        public NRRCertification Certification;

        [FieldOffset(0x230)]
        public fixed byte NrrSignature[0x100];

        [FieldOffset(0x330)]
        public ulong TitleId;

        [FieldOffset(0x338)]
        public uint NrrSize;

        [FieldOffset(0x33C)]
        public byte Type; // 7.0.0+

        [FieldOffset(0x33D)]
        public fixed byte Reserved2[0x3];

        [FieldOffset(0x340)]
        public uint HashOffset;

        [FieldOffset(0x344)]
        public uint HashCount;

        [FieldOffset(0x348)]
        public ulong Reserved3;
    }
}
