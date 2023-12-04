using Ryujinx.Common.Memory;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6 * sizeof(int))]
    struct AddrInfoSerializedHeader
    {
        public uint Magic;
        public int Flags;
        public int Family;
        public int SocketType;
        public int Protocol;
        public uint AddressLength;

        public AddrInfoSerializedHeader(IPAddress address, SocketType socketType)
        {
            Magic = SfdnsresContants.AddrInfoMagic;
            Flags = 0;
            Family = (int)address.AddressFamily;
            SocketType = (int)socketType;
            Protocol = 0;

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                AddressLength = (uint)Unsafe.SizeOf<AddrInfo4>();
            }
            else
            {
                AddressLength = (uint)Unsafe.SizeOf<Array4<byte>>();
            }
        }

        public void ToNetworkOrder()
        {
            Magic = (uint)IPAddress.HostToNetworkOrder((int)Magic);
            Flags = IPAddress.HostToNetworkOrder(Flags);
            Family = IPAddress.HostToNetworkOrder(Family);
            SocketType = IPAddress.HostToNetworkOrder(SocketType);
            Protocol = IPAddress.HostToNetworkOrder(Protocol);
            AddressLength = (uint)IPAddress.HostToNetworkOrder((int)AddressLength);
        }

        public void ToHostOrder()
        {
            Magic = (uint)IPAddress.NetworkToHostOrder((int)Magic);
            Flags = IPAddress.NetworkToHostOrder(Flags);
            Family = IPAddress.NetworkToHostOrder(Family);
            SocketType = IPAddress.NetworkToHostOrder(SocketType);
            Protocol = IPAddress.NetworkToHostOrder(Protocol);
            AddressLength = (uint)IPAddress.NetworkToHostOrder((int)AddressLength);
        }
    }
}
