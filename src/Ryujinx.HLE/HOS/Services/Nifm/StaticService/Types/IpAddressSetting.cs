using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xd)]
    struct IpAddressSetting
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool IsDhcpEnabled;
        public IpV4Address Address;
        public IpV4Address IPv4Mask;
        public IpV4Address GatewayAddress;

        public IpAddressSetting(IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastIPAddressInformation)
        {
            IsDhcpEnabled = OperatingSystem.IsMacOS() || interfaceProperties.DhcpServerAddresses.Count != 0;
            Address = new IpV4Address(unicastIPAddressInformation.Address);
            IPv4Mask = new IpV4Address(unicastIPAddressInformation.IPv4Mask);
            GatewayAddress = (interfaceProperties.GatewayAddresses.Count == 0) ? new IpV4Address() : new IpV4Address(interfaceProperties.GatewayAddresses[0].Address);
        }
    }
}
