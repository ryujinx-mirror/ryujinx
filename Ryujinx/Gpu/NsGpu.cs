using Gal;

namespace Ryujinx.Gpu
{
    class NsGpu
    {
        public IGalRenderer Renderer { get; private set; }

        public NsGpuMemoryMgr MemoryMgr { get; private set; }

        public NsGpuPGraph PGraph { get; private set; }

        public NsGpu(IGalRenderer Renderer)
        {
            this.Renderer = Renderer;

            MemoryMgr = new NsGpuMemoryMgr();

            PGraph = new NsGpuPGraph(this);
        }
    }
}