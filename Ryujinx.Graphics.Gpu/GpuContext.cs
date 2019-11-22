using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Engine;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.State;
using System;

namespace Ryujinx.Graphics.Gpu
{
    public class GpuContext
    {
        public IRenderer Renderer { get; }

        internal IPhysicalMemory PhysicalMemory { get; private set; }

        public MemoryManager MemoryManager { get; }

        internal MemoryAccessor MemoryAccessor { get; }

        internal Methods Methods { get; }

        internal NvGpuFifo Fifo { get; }

        public DmaPusher DmaPusher { get; }

        internal int SequenceNumber { get; private set; }

        private Lazy<Capabilities> _caps;

        internal Capabilities Capabilities => _caps.Value;

        public GpuContext(IRenderer renderer)
        {
            Renderer = renderer;

            MemoryManager = new MemoryManager();

            MemoryAccessor = new MemoryAccessor(this);

            Methods = new Methods(this);

            Fifo = new NvGpuFifo(this);

            DmaPusher = new DmaPusher(this);

            _caps = new Lazy<Capabilities>(GetCapabilities);
        }

        internal void AdvanceSequence()
        {
            SequenceNumber++;
        }

        public ITexture GetTexture(
            ulong  address,
            int    width,
            int    height,
            int    stride,
            bool   isLinear,
            int    gobBlocksInY,
            Format format,
            int    bytesPerPixel)
        {
            FormatInfo formatInfo = new FormatInfo(format, 1, 1, bytesPerPixel);

            TextureInfo info = new TextureInfo(
                address,
                width,
                height,
                1,
                1,
                1,
                1,
                stride,
                isLinear,
                gobBlocksInY,
                1,
                1,
                Target.Texture2D,
                formatInfo);

            return Methods.GetTexture(address)?.HostTexture;
        }

        private Capabilities GetCapabilities()
        {
            return Renderer.GetCapabilities();
        }

        public void SetVmm(IPhysicalMemory mm)
        {
            PhysicalMemory = mm;
        }
    }
}