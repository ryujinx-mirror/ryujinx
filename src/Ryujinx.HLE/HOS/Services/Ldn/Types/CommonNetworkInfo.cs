using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x30)]
    struct CommonNetworkInfo
    {
        public Array6<byte> MacAddress;
        public Ssid Ssid;
        public ushort Channel;
        public byte LinkLevel;
        public byte NetworkType;
        public uint Reserved;
    }
}
