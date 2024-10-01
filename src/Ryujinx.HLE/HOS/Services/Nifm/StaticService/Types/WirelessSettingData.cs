using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x65)]
    struct WirelessSettingData
    {
        public byte SsidLength;
        public Array32<byte> Ssid;
        public Array3<byte> Unknown;
        public Array64<byte> Passphrase1;
        public byte Passphrase2;
    }
}
