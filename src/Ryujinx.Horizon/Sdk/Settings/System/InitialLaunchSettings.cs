using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Settings.System
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20, Pack = 0x8)]
    struct InitialLaunchSettings
    {
        public uint Flags;
        public uint Reserved;
        public ulong TimeStamp1;
        public ulong TimeStamp2;
        public ulong TimeStamp3;
    }
}
