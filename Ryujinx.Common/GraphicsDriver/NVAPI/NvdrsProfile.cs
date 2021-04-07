using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.GraphicsDriver.NVAPI
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct NvdrsProfile
    {
        public uint Version;
        public NvapiUnicodeString ProfileName;
        public uint GpuSupport;
        public uint IsPredefined;
        public uint NumOfApps;
        public uint NumOfSettings;
    }
}
