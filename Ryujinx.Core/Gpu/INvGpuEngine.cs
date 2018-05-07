namespace Ryujinx.Core.Gpu
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry);
    }
}