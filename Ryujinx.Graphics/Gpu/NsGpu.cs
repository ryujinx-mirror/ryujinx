using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;

namespace Ryujinx.Graphics.Gpu
{
    public class NsGpu
    {
        public IGalRenderer Renderer { get; private set; }

        internal NsGpuMemoryMgr MemoryMgr { get; private set; }

        internal NsGpuPGraph PGraph { get; private set; }

        public NsGpu(IGalRenderer Renderer)
        {
            this.Renderer = Renderer;

            MemoryMgr = new NsGpuMemoryMgr();

            PGraph = new NsGpuPGraph(this);
        }

        public long GetCpuAddr(long Position)
        {
            return MemoryMgr.GetCpuAddr(Position);
        }

        public long MapMemory(long CpuAddr, long Size)
        {
            return MemoryMgr.Map(CpuAddr, Size);
        }

        public long MapMemory(long CpuAddr, long GpuAddr, long Size)
        {
            return MemoryMgr.Map(CpuAddr, GpuAddr, Size);
        }

        public void ProcessPushBuffer(NsGpuPBEntry[] PushBuffer, AMemory Memory)
        {
            PGraph.ProcessPushBuffer(PushBuffer, Memory);
        }

        public long ReserveMemory(long Size, long Align)
        {
            return MemoryMgr.Reserve(Size, Align);
        }

        public long ReserveMemory(long GpuAddr, long Size, long Align)
        {
            return MemoryMgr.Reserve(GpuAddr, Size, Align);
        }
    }
}