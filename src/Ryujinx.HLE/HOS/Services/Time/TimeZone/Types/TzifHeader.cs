using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.TimeZone
{
    [StructLayout(LayoutKind.Sequential, Pack = 0x4, Size = 0x2C)]
    struct TzifHeader
    {
        public Array4<byte> Magic;
        public byte Version;
        private Array15<byte> _reserved;
        public int TtisGMTCount;
        public int TtisSTDCount;
        public int LeapCount;
        public int TimeCount;
        public int TypeCount;
        public int CharCount;
    }
}
