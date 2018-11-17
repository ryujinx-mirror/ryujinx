using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(NvGpuVmm Vmm, GpuMethodCall MethCall);
    }
}