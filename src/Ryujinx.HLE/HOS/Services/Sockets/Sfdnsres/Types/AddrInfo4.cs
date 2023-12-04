using Ryujinx.Common.Memory;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    struct AddrInfo4
    {
        public byte Length;
        public byte Family;
        public short Port;
        public Array4<byte> Address;
        public Array8<byte> Padding;

        public AddrInfo4(IPAddress address, short port)
        {
            Length = (byte)Unsafe.SizeOf<Array4<byte>>();
            Family = (byte)AddressFamily.InterNetwork;
            Port = IPAddress.HostToNetworkOrder(port);
            Address = new Array4<byte>();

            address.TryWriteBytes(Address.AsSpan(), out _);
        }

        public void ToNetworkOrder()
        {
            Port = IPAddress.HostToNetworkOrder(Port);

            RawIpv4AddressNetworkEndianSwap(ref Address);
        }

        public void ToHostOrder()
        {
            Port = IPAddress.NetworkToHostOrder(Port);

            RawIpv4AddressNetworkEndianSwap(ref Address);
        }

        public static void RawIpv4AddressNetworkEndianSwap(ref Array4<byte> address)
        {
            if (BitConverter.IsLittleEndian)
            {
                address.AsSpan().Reverse();
            }
        }
    }
}
