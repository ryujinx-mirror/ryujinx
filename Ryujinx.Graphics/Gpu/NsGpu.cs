using Ryujinx.Graphics.Gal;
using System.Threading;

namespace Ryujinx.Graphics.Gpu
{
    public class NsGpu
    {
        public IGalRenderer Renderer { get; private set; }

        internal NsGpuMemoryMgr MemoryMgr { get; private set; }

        public NvGpuFifo Fifo;

        internal NvGpuEngine3d Engine3d;

        private Thread FifoProcessing;

        private bool KeepRunning;

        public NsGpu(IGalRenderer Renderer)
        {
            this.Renderer = Renderer;

            MemoryMgr = new NsGpuMemoryMgr();

            Fifo = new NvGpuFifo(this);

            Engine3d = new NvGpuEngine3d(this);

            KeepRunning = true;

            FifoProcessing = new Thread(ProcessFifo);            

            FifoProcessing.Start();
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

        public long ReserveMemory(long Size, long Align)
        {
            return MemoryMgr.Reserve(Size, Align);
        }

        public long ReserveMemory(long GpuAddr, long Size, long Align)
        {
            return MemoryMgr.Reserve(GpuAddr, Size, Align);
        }

        private void ProcessFifo()
        {
            while (KeepRunning)
            {
                Fifo.DispatchCalls();

                Thread.Yield();
            }
        }
    }
}