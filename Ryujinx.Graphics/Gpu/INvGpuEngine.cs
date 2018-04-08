using ChocolArm64.Memory;

namespace Ryujinx.Graphics.Gpu
{
    interface INvGpuEngine
    {
        int[] Registers { get; }

        void CallMethod(AMemory Memory, NsGpuPBEntry PBEntry);
    }
}