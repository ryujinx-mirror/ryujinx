using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0x40)]
    struct CommonInfo
    {
        public ushort LastWriteYear;
        public byte LastWriteMonth;
        public byte LastWriteDay;
        public ushort WriteCounter;
        public ushort Version;
        public uint ApplicationAreaSize;
        public Array52<byte> Reserved;
    }
}
