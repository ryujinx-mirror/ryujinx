using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct CommonArguments
    {
        public uint Version;
        public uint StructureSize;
        public uint AppletVersion;
        public uint ThemeColor;
        [MarshalAs(UnmanagedType.I1)]
        public bool PlayStartupSound;
        public ulong SystemTicks;
    }
}
