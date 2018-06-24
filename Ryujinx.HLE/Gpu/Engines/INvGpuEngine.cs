using Ryujinx.HLE.Gpu.Memory;

namespace Ryujinx.HLE.Gpu.Engines
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry);
    }
}