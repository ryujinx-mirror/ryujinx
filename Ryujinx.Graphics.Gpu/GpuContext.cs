using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.GPFifo;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Gpu.Synchronization;
using System;
using System.Collections.Concurrent;
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

        /// <summary>
        /// Queue with deferred actions that must run on the render thread.
        /// </summary>
        internal Queue<Action> DeferredActions { get; }

        /// <summary>
        /// Registry with physical memories that can be used with this GPU context, keyed by owner process ID.
        /// </summary>
        internal ConcurrentDictionary<long, PhysicalMemory> PhysicalMemoryRegistry { get; }

        /// <summary>
        /// Host hardware capabilities.
        /// </summary>
        internal Capabilities Capabilities => _caps.Value;

        /// <summary>
        /// Event for signalling shader cache loading progress.
        /// </summary>
        public event Action<ShaderCacheState, int, int> ShaderCacheStateChanged;

        private readonly Lazy<Capabilities> _caps;

        /// <summary>
        /// Creates a new instance of the GPU emulation context.
        /// </summary>
        /// <param name="renderer">Host renderer</param>
        public GpuContext(IRenderer renderer)
        {
            Renderer = renderer;

            GPFifo = new GPFifoDevice(this);

            Synchronization = new SynchronizationManager();

            Window = new Window(this);

            HostInitalized = new ManualResetEvent(false);

            SyncActions = new List<Action>();

            DeferredActions = new Queue<Action>();

            PhysicalMemoryRegistry = new ConcurrentDictionary<long, PhysicalMemory>();

            _caps = new Lazy<Capabilities>(Renderer.GetCapabilities);
        }

        /// <summary>
        /// Creates a new GPU channel.
        /// </summary>
        /// <returns>The GPU channel</returns>
        public GpuChannel CreateChannel()
        {
            return new GpuChannel(this);
        }

        /// <summary>
        /// Creates a new GPU memory manager.
        /// </summary>
        /// <param name="pid">ID of the process that owns the memory manager</param>
        /// <returns>The memory manager</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pid"/> is invalid</exception>
        public MemoryManager CreateMemoryManager(long pid)
        {
            if (!PhysicalMemoryRegistry.TryGetValue(pid, out var physicalMemory))
            {
                throw new ArgumentException("The PID is invalid or the process was not registered", nameof(pid));
            }

            return new MemoryManager(physicalMemory);
        }

        /// <summary>
        /// Registers virtual memory used by a process for GPU memory access, caching and read/write tracking.
        /// </summary>
        /// <param name="pid">ID of the process that owns <paramref name="cpuMemory"/></param>
        /// <param name="cpuMemory">Virtual memory owned by the process</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pid"/> was already registered</exception>
        public void RegisterProcess(long pid, Cpu.IVirtualMemoryManagerTracked cpuMemory)
        {
            var physicalMemory = new PhysicalMemory(this, cpuMemory);
            if (!PhysicalMemoryRegistry.TryAdd(pid, physicalMemory))
            {
                throw new ArgumentException("The PID was already registered", nameof(pid));
            }

            physicalMemory.ShaderCache.ShaderCacheStateChanged += ShaderCacheStateUpdate;
        }

        /// <summary>
        /// Unregisters a process, indicating that its memory will no longer be used, and that caches can be freed.
        /// </summary>
        /// <param name="pid">ID of the process</param>
        public void UnregisterProcess(long pid)
        {
            if (PhysicalMemoryRegistry.TryRemove(pid, out var physicalMemory))
            {
                physicalMemory.ShaderCache.ShaderCacheStateChanged -= ShaderCacheStateUpdate;
                physicalMemory.Dispose();
            }
        }

        /// <summary>
        /// Shader cache state update handler.
        /// </summary>
        /// <param name="state">Current state of the shader cache load process</param>
        /// <param name="current">Number of the current shader being processed</param>
        /// <param name="total">Total number of shaders to process</param>
        private void ShaderCacheStateUpdate(ShaderCacheState state, int current, int total)
        {
            ShaderCacheStateChanged?.Invoke(state, current, total);
        }

        /// <summary>
        /// Initialize the GPU shader cache.
        /// </summary>
        public void InitializeShaderCache()
        {
            HostInitalized.WaitOne();

            foreach (var physicalMemory in PhysicalMemoryRegistry.Values)
            {
                physicalMemory.ShaderCache.Initialize();
            }
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
        /// Performs deferred actions.
        /// This is useful for actions that must run on the render thread, such as resource disposal.
        /// </summary>
        internal void RunDeferredActions()
        {
            while (DeferredActions.TryDequeue(out Action action))
            {
                action();
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
            Renderer.Dispose();
            GPFifo.Dispose();
            HostInitalized.Dispose();

            // Has to be disposed before processing deferred actions, as it will produce some.
            foreach (var physicalMemory in PhysicalMemoryRegistry.Values)
            {
                physicalMemory.Dispose();
            }

            PhysicalMemoryRegistry.Clear();

            RunDeferredActions();
        }
    }
}