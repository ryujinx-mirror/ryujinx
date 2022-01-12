using Ryujinx.Common.Memory;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    struct BsdSockAddr
    {
        public byte Length;
        public byte Family;
        public ushort Port;
        public Array4<byte> Address;
        private Array8<byte> _reserved;

        public IPEndPoint ToIPEndPoint()
        {
            IPAddress address = new IPAddress(Address.ToSpan());
            int port = (ushort)IPAddress.NetworkToHostOrder((short)Port);

            return new IPEndPoint(address, port);
        }

        public static BsdSockAddr FromIPEndPoint(IPEndPoint endpoint)
        {
            BsdSockAddr result = new BsdSockAddr
            {
                Length = 0,
                Family = (byte)endpoint.AddressFamily,
                Port = (ushort)IPAddress.HostToNetworkOrder((short)endpoint.Port)
            };

            endpoint.Address.GetAddressBytes().AsSpan().CopyTo(result.Address.ToSpan());

            return result;
        }
    }
}
