using Ryujinx.Common.Memory;
using Ryujinx.HLE.Utilities;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres.Types
{
    class AddrInfoSerialized
    {
        public AddrInfoSerializedHeader Header;
        public AddrInfo4? SocketAddress;
        public Array4<byte>? RawIPv4Address;
        public string CanonicalName;

        public AddrInfoSerialized(AddrInfoSerializedHeader header, AddrInfo4? address, Array4<byte>? rawIPv4Address, string canonicalName)
        {
            Header = header;
            SocketAddress = address;
            RawIPv4Address = rawIPv4Address;
            CanonicalName = canonicalName;
        }

        public static AddrInfoSerialized Read(ReadOnlySpan<byte> buffer, out ReadOnlySpan<byte> rest)
        {
            if (!MemoryMarshal.TryRead(buffer, out AddrInfoSerializedHeader header))
            {
                rest = buffer;

                return null;
            }

            AddrInfo4? socketAddress = null;
            Array4<byte>? rawIPv4Address = null;
            string canonicalName;

            buffer = buffer[Unsafe.SizeOf<AddrInfoSerializedHeader>()..];

            header.ToHostOrder();

            if (header.Magic != SfdnsresContants.AddrInfoMagic)
            {
                rest = buffer;

                return null;
            }

            Debug.Assert(header.Magic == SfdnsresContants.AddrInfoMagic);

            if (header.AddressLength == 0)
            {
                rest = buffer;

                return null;
            }

            if (header.Family == (int)AddressFamily.InterNetwork)
            {
                socketAddress = MemoryMarshal.Read<AddrInfo4>(buffer);
                socketAddress.Value.ToHostOrder();

                buffer = buffer[Unsafe.SizeOf<AddrInfo4>()..];
            }
            // AF_INET6
            else if (header.Family == 28)
            {
                throw new NotImplementedException();
            }
            else
            {
                // Nintendo hardcode 4 bytes in that case here.
                Array4<byte> address = MemoryMarshal.Read<Array4<byte>>(buffer);
                AddrInfo4.RawIpv4AddressNetworkEndianSwap(ref address);

                rawIPv4Address = address;

                buffer = buffer[Unsafe.SizeOf<Array4<byte>>()..];
            }

            canonicalName = StringUtils.ReadUtf8String(buffer, out int dataRead);
            buffer = buffer[dataRead..];

            rest = buffer;

            return new AddrInfoSerialized(header, socketAddress, rawIPv4Address, canonicalName);
        }

        public Span<byte> Write(Span<byte> buffer)
        {
            int familly = Header.Family;

            Header.ToNetworkOrder();

            MemoryMarshal.Write(buffer, in Header);

            buffer = buffer[Unsafe.SizeOf<AddrInfoSerializedHeader>()..];

            if (familly == (int)AddressFamily.InterNetwork)
            {
                AddrInfo4 socketAddress = SocketAddress.Value;
                socketAddress.ToNetworkOrder();

                MemoryMarshal.Write(buffer, in socketAddress);

                buffer = buffer[Unsafe.SizeOf<AddrInfo4>()..];
            }
            // AF_INET6
            else if (familly == 28)
            {
                throw new NotImplementedException();
            }
            else
            {
                Array4<byte> rawIPv4Address = RawIPv4Address.Value;
                AddrInfo4.RawIpv4AddressNetworkEndianSwap(ref rawIPv4Address);

                MemoryMarshal.Write(buffer, in rawIPv4Address);

                buffer = buffer[Unsafe.SizeOf<Array4<byte>>()..];
            }

            if (CanonicalName == null)
            {
                buffer[0] = 0;

                buffer = buffer[1..];
            }
            else
            {
                byte[] canonicalName = Encoding.ASCII.GetBytes(CanonicalName + '\0');

                canonicalName.CopyTo(buffer);

                buffer = buffer[canonicalName.Length..];
            }

            return buffer;
        }
    }
}
