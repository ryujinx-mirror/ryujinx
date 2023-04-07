using System.Runtime.InteropServices;

namespace Ryujinx.Common.GraphicsDriver.NVAPI
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    unsafe struct NvdrsApplicationV4
    {
        public uint Version;
        public uint IsPredefined;
        public NvapiUnicodeString AppName;
        public NvapiUnicodeString UserFriendlyName;
        public NvapiUnicodeString Launcher;
        public NvapiUnicodeString FileInFolder;
        public uint Flags;
        public NvapiUnicodeString CommandLine;
    }
}
