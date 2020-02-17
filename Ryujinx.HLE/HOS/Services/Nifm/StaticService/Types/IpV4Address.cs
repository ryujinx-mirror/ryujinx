using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct IpV4Address
    {
        public uint Address;

        public IpV4Address(IPAddress address)
        {
            if (address == null)
            {
                Address = 0;
            }
            else
            {
                Address = BitConverter.ToUInt32(address.GetAddressBytes());
            }
        }
    }
}
