using Ryujinx.Graphics.Gal;

namespace Ryujinx.Graphics
{
    public class NvGpu
    {
        public IGalRenderer Renderer { get; private set; }

        public GpuResourceManager ResourceManager { get; private set; }

        public DmaPusher Pusher { get; private set; }

        internal NvGpuFifo       Fifo       { get; private set; }
        internal NvGpuEngine2d   Engine2d   { get; private set; }
        internal NvGpuEngine3d   Engine3d   { get; private set; }
        internal NvGpuEngineM2mf EngineM2mf { get; private set; }
        internal NvGpuEngineP2mf EngineP2mf { get; private set; }

        public NvGpu(IGalRenderer Renderer)
        {
            this.Renderer = Renderer;

            ResourceManager = new GpuResourceManager(this);

            Pusher = new DmaPusher(this);

            Fifo       = new NvGpuFifo(this);
            Engine2d   = new NvGpuEngine2d(this);
            Engine3d   = new NvGpuEngine3d(this);
            EngineM2mf = new NvGpuEngineM2mf(this);
            EngineP2mf = new NvGpuEngineP2mf(this);
        }
    }
}