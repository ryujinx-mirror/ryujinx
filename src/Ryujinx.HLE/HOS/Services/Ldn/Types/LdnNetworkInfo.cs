using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x430)]
    struct LdnNetworkInfo
    {
        public Array16<byte> SecurityParameter;
        public ushort SecurityMode;
        public AcceptPolicy StationAcceptPolicy;
        public byte Reserved1;
        public ushort Reserved2;
        public byte NodeCountMax;
        public byte NodeCount;
        public Array8<NodeInfo> Nodes;
        public ushort Reserved3;
        public ushort AdvertiseDataSize;
        public Array384<byte> AdvertiseData;
        public Array140<byte> Reserved4;
        public ulong AuthenticationId;
    }
}
