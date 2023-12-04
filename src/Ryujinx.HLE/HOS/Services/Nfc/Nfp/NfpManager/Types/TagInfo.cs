using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0x58)]
    struct TagInfo
    {
        public Array10<byte> Uuid;
        public byte UuidLength;
        public Array21<byte> Reserved1;
        public uint Protocol;
        public uint TagType;
        public Array6<byte> Reserved2;
    }
}
