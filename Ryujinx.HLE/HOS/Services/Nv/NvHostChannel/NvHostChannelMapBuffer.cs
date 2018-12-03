using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvHostChannel
{
    [StructLayout(LayoutKind.Sequential, Size = 0xc, Pack = 4)]
    struct NvHostChannelMapBuffer
    {
        public int  NumEntries;
        public int  DataAddress; //Ignored by the driver.
        public bool AttachHostChDas;
    }
}