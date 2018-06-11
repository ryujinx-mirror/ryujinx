namespace Ryujinx.HLE.Gpu
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(NvGpuVmm Vmm, NvGpuPBEntry PBEntry);
    }
}