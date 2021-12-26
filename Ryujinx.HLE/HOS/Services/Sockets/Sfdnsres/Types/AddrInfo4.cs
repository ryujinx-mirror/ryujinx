using Ryujinx.Common.Memory;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    struct AddrInfo4
    {
        public byte         Length;
        public byte         Family;
        public short        Port;
        public Array4<byte> Address;

        public AddrInfo4(IPAddress address, short port)
        {
            Length  = 0;
            Family  = (byte)AddressFamily.InterNetwork;
            Port    = port;
            Address = default;

            Span<byte> outAddress = Address.ToSpan();
            address.TryWriteBytes(outAddress, out _);
            outAddress.Reverse();
        }
    }
}