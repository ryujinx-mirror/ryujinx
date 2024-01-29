using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [StructLayout(LayoutKind.Sequential, Size = 0x14, Pack = 0x1)]
    struct HomeMenuScheme
    {
        public uint Main;
        public uint Back;
        public uint Sub;
        public uint Bezel;
        public uint Extra;
    }
}
