using Ryujinx.Graphics.Gal;
using System.Threading;

namespace Ryujinx.Core.Gpu
{
    public class NvGpu
    {
        public IGalRenderer Renderer { get; private set; }

        public NvGpuFifo Fifo { get; private set; }

        public NvGpuEngine2d Engine2d { get; private set; }
        public NvGpuEngine3d Engine3d { get; private set; }

        private Thread FifoProcessing;

        private bool KeepRunning;

        public NvGpu(IGalRenderer Renderer)
        {
            this.Renderer = Renderer;

            Fifo = new NvGpuFifo(this);

            Engine2d = new NvGpuEngine2d(this);
            Engine3d = new NvGpuEngine3d(this);

            KeepRunning = true;

            FifoProcessing = new Thread(ProcessFifo);

            FifoProcessing.Start();
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