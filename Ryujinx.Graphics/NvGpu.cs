using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Graphics3d;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.VDec;
using Ryujinx.Graphics.Vic;

namespace Ryujinx.Graphics
{
    public class NvGpu
    {
        public const int MaxViewportSize = 0x3FFF;

        public IGalRenderer Renderer { get; private set; }

        public GpuResourceManager ResourceManager { get; private set; }

        public DmaPusher Pusher { get; private set; }

        internal NvGpuFifo       Fifo       { get; private set; }
        internal NvGpuEngine2d   Engine2d   { get; private set; }
        internal NvGpuEngine3d   Engine3d   { get; private set; }
        internal NvGpuEngineM2mf EngineM2mf { get; private set; }
        internal NvGpuEngineP2mf EngineP2mf { get; private set; }

        private  CdmaProcessor      _cdmaProcessor;
        internal VideoDecoder       VideoDecoder       { get; private set; }
        internal VideoImageComposer VideoImageComposer { get; private set; }

        public NvGpu(IGalRenderer renderer)
        {
            Renderer = renderer;

            ResourceManager = new GpuResourceManager(this);

            Pusher = new DmaPusher(this);

            Fifo       = new NvGpuFifo(this);
            Engine2d   = new NvGpuEngine2d(this);
            Engine3d   = new NvGpuEngine3d(this);
            EngineM2mf = new NvGpuEngineM2mf(this);
            EngineP2mf = new NvGpuEngineP2mf(this);

            _cdmaProcessor      = new CdmaProcessor(this);
            VideoDecoder       = new VideoDecoder(this);
            VideoImageComposer = new VideoImageComposer(this);
        }

        public void PushCommandBuffer(NvGpuVmm vmm, int[] cmdBuffer)
        {
            lock (_cdmaProcessor)
            {
                _cdmaProcessor.PushCommands(vmm, cmdBuffer);
            }
        }

        public void UninitializeVideoDecoder()
        {
            lock (_cdmaProcessor)
            {
                FFmpegWrapper.Uninitialize();
            }
        }
    }
}