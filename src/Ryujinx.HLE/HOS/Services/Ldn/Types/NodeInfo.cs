using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x40)]
    struct NodeInfo
    {
        public uint Ipv4Address;
        public Array6<byte> MacAddress;
        public byte NodeId;
        public byte IsConnected;
        public Array33<byte> UserName;
        public byte Reserved1;
        public ushort LocalCommunicationVersion;
        public Array16<byte> Reserved2;
    }
}
