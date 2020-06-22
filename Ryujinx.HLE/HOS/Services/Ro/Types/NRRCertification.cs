using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    [StructLayout(LayoutKind.Explicit, Size = 0x220)]
    unsafe struct NRRCertification
    {
        [FieldOffset(0)]
        public ulong ApplicationIdMask;

        [FieldOffset(0x8)]
        public ulong ApplicationIdPattern;

        [FieldOffset(0x10)]
        public fixed byte Reserved[0x10];

        [FieldOffset(0x20)]
        public fixed byte Modulus[0x100];

        [FieldOffset(0x120)]
        public fixed byte Signature[0x100];
    }
}
