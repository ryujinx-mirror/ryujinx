using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine;
using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu
{
    public class GpuContext
    {
        public IRenderer Renderer { get; }

        internal PhysicalMemory PhysicalMemory { get; private set; }

        public MemoryManager MemoryManager { get; }

        internal MemoryAccessor MemoryAccessor { get; }

        internal Methods Methods { get; }

        internal NvGpuFifo Fifo { get; }

        public DmaPusher DmaPusher { get; }

        public Window Window { get; }

        internal int SequenceNumber { get; private set; }

        private readonly Lazy<Capabilities> _caps;

        internal Capabilities Capabilities => _caps.Value;

        public GpuContext(IRenderer renderer)
        {
            Renderer = renderer;

            MemoryManager = new MemoryManager();

            MemoryAccessor = new MemoryAccessor(this);

            Methods = new Methods(this);

            Fifo = new NvGpuFifo(this);

            DmaPusher = new DmaPusher(this);

            Window = new Window(this);

            _caps = new Lazy<Capabilities>(Renderer.GetCapabilities);
        }

        internal void AdvanceSequence()
        {
            SequenceNumber++;
        }

        public void SetVmm(ARMeilleure.Memory.MemoryManager cpuMemory)
        {
            PhysicalMemory = new PhysicalMemory(cpuMemory);
        }
    }
}