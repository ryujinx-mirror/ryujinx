using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Graphics3d
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(NvGpuVmm Vmm, GpuMethodCall MethCall);
    }
}