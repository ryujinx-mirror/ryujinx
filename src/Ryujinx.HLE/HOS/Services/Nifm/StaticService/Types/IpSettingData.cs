using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xc2)]
    struct IpSettingData
    {
        public IpAddressSetting IpAddressSetting;
        public DnsSetting DnsSetting;
        public ProxySetting ProxySetting;
        public short Mtu;
    }
}
