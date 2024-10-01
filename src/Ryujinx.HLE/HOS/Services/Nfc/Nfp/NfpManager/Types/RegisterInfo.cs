using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Services.Mii.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0x100)]
    struct RegisterInfo
    {
        public CharInfo MiiCharInfo;
        public ushort FirstWriteYear;
        public byte FirstWriteMonth;
        public byte FirstWriteDay;
        public Array41<byte> Nickname;
        public byte FontRegion;
        public Array64<byte> Reserved1;
        public Array58<byte> Reserved2;
    }
}
