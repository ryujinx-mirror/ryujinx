using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 9)]
    struct DnsSetting
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool        IsDynamicDnsEnabled;
        public IpV4Address PrimaryDns;
        public IpV4Address SecondaryDns;

        public DnsSetting(IPInterfaceProperties interfaceProperties)
        {
            IsDynamicDnsEnabled = interfaceProperties.IsDynamicDnsEnabled;
            PrimaryDns          = new IpV4Address(interfaceProperties.DnsAddresses[0]);
            SecondaryDns        = new IpV4Address(interfaceProperties.DnsAddresses[1]);
        }
    }
}
