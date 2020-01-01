using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine;
using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// GPU emulation context.
    /// </summary>
    public sealed class GpuContext : IDisposable
    {
        /// <summary>
        /// Host renderer.
        /// </summary>
        public IRenderer Renderer { get; }

        /// <summary>
        /// Physical memory access (it actually accesses the process memory, not actual physical memory).
        /// </summary>
        internal PhysicalMemory PhysicalMemory { get; private set; }

        /// <summary>
        /// GPU memory manager.
        /// </summary>
        public MemoryManager MemoryManager { get; }

        /// <summary>
        /// GPU memory accessor.
        /// </summary>
        public MemoryAccessor MemoryAccessor { get; }

        /// <summary>
        /// GPU engine methods processing.
        /// </summary>
        internal Methods Methods { get; }

        /// <summary>
        /// GPU commands FIFO.
        /// </summary>
        internal NvGpuFifo Fifo { get; }

        /// <summary>
        /// DMA pusher.
        /// </summary>
        public DmaPusher DmaPusher { get; }

        /// <summary>
        /// Presentation window.
        /// </summary>
        public Window Window { get; }

        /// <summary>
        /// Internal sequence number, used to avoid needless resource data updates
        /// in the middle of a command buffer before synchronizations.
        /// </summary>
        internal int SequenceNumber { get; private set; }

        private readonly Lazy<Capabilities> _caps;

        /// <summary>
        /// Host hardware capabilities.
        /// </summary>
        internal Capabilities Capabilities => _caps.Value;

        /// <summary>
        /// Creates a new instance of the GPU emulation context.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
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

        /// <summary>
        /// Advances internal sequence number.
        /// This forces the update of any modified GPU resource.
        /// </summary>
        internal void AdvanceSequence()
        {
            SequenceNumber++;
        }

        /// <summary>
        /// Sets the process memory manager, after the application process is initialized.
        /// This is required for any GPU memory access.
        /// </summary>
        /// <param name="cpuMemory">CPU memory manager</param>
        public void SetVmm(ARMeilleure.Memory.MemoryManager cpuMemory)
        {
            PhysicalMemory = new PhysicalMemory(cpuMemory);
        }

        /// <summary>
        /// Disposes all GPU resources currently cached.
        /// It's an error to push any GPU commands after disposal.
        /// Additionally, the GPU commands FIFO must be empty for disposal,
        /// and processing of all commands must have finished.
        /// </summary>
        public void Dispose()
        {
            Methods.ShaderCache.Dispose();
            Methods.BufferManager.Dispose();
            Methods.TextureManager.Dispose();
        }
    }
}