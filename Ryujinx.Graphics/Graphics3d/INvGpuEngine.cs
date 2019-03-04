using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Graphics3d
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(NvGpuVmm vmm, GpuMethodCall methCall);
    }
}