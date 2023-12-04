using Ryujinx.HLE.HOS.Services.Nv.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct AllocGpfifoExArguments
    {
        public uint NumEntries;
        public uint NumJobs;
        public uint Flags;
        public NvFence Fence;
        public uint Reserved1;
        public uint Reserved2;
        public uint Reserved3;
    }
}
