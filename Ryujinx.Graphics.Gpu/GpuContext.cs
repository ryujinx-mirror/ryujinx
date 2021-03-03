using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine;
using Ryujinx.Graphics.Gpu.Engine.GPFifo;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Synchronization;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// GPU emulation context.
    /// </summary>
    public sealed class GpuContext : IDisposable
    {
        /// <summary>
        /// Event signaled when the host emulation context is ready to be used by the gpu context.
        /// </summary>
        public ManualResetEvent HostInitalized { get; }

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
        /// GPU engine methods processing.
        /// </summary>
        internal Methods Methods { get; }

        /// <summary>
        /// GPU General Purpose FIFO queue.
        /// </summary>
        public GPFifoDevice GPFifo { get; }

        /// <summary>
        /// GPU synchronization manager.
        /// </summary>
        public SynchronizationManager Synchronization { get; }

        /// <summary>
        /// Presentation window.
        /// </summary>
        public Window Window { get; }

        /// <summary>
        /// Internal sequence number, used to avoid needless resource data updates
        /// in the middle of a command buffer before synchronizations.
        /// </summary>
        internal int SequenceNumber { get; private set; }

        /// <summary>
        /// Internal sync number, used to denote points at which host synchronization can be requested.
        /// </summary>
        internal ulong SyncNumber { get; private set; }

        /// <summary>
        /// Actions to be performed when a CPU waiting sync point is triggered.
        /// If there are more than 0 items when this happens, a host sync object will be generated for the given <see cref="SyncNumber"/>,
        /// and the SyncNumber will be incremented.
        /// </summary>
        internal List<Action> SyncActions { get; }

        private readonly Lazy<Capabilities> _caps;

        /// <summary>
        /// Host hardware capabilities.
        /// </summary>
        internal Capabilities Capabilities => _caps.Value;

        /// <summary>
        /// Signaled when shader cache begins and ends loading.
        /// Signals true when loading has started, false when ended.
        /// </summary>
        public event Action<bool> ShaderCacheStateChanged
        {
            add => Methods.ShaderCache.ShaderCacheStateChanged += value;
            remove => Methods.ShaderCache.ShaderCacheStateChanged -= value;
        }

        /// <summary>
        /// Signaled while shader cache is loading to indicate current progress.
        /// Provides current and total number of shaders loaded.
        /// </summary>
        public event Action<int, int> ShaderCacheProgressChanged
        {
            add => Methods.ShaderCache.ShaderCacheProgressChanged += value;
            remove => Methods.ShaderCache.ShaderCacheProgressChanged -= value;
        }

        /// <summary>
        /// Creates a new instance of the GPU emulation context.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        public GpuContext(IRenderer renderer)
        {
            Renderer = renderer;

            MemoryManager = new MemoryManager(this);

            Methods = new Methods(this);

            GPFifo = new GPFifoDevice(this);

            Synchronization = new SynchronizationManager();

            Window = new Window(this);

            _caps = new Lazy<Capabilities>(Renderer.GetCapabilities);

            HostInitalized = new ManualResetEvent(false);

            SyncActions = new List<Action>();
        }

        /// <summary>
        /// Initialize the GPU shader cache.
        /// </summary>
        public void InitializeShaderCache()
        {
            HostInitalized.WaitOne();

            Methods.ShaderCache.Initialize();
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
        public void SetVmm(Cpu.MemoryManager cpuMemory)
        {
            PhysicalMemory = new PhysicalMemory(cpuMemory);
        }

        /// <summary>
        /// Registers an action to be performed the next time a syncpoint is incremented.
        /// This will also ensure a host sync object is created, and <see cref="SyncNumber"/> is incremented.
        /// </summary>
        /// <param name="action">The action to be performed on sync object creation</param>
        public void RegisterSyncAction(Action action)
        {
            SyncActions.Add(action);
        }

        /// <summary>
        /// Creates a host sync object if there are any pending sync actions. The actions will then be called.
        /// If no actions are present, a host sync object is not created.
        /// </summary>
        public void CreateHostSyncIfNeeded()
        {
            if (SyncActions.Count > 0)
            {
                Renderer.CreateSync(SyncNumber);

                SyncNumber++;

                foreach (Action action in SyncActions)
                {
                    action();
                }

                SyncActions.Clear();
            }
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
            Renderer.Dispose();
            GPFifo.Dispose();
            HostInitalized.Dispose();
        }
    }
}