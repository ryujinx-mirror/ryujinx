using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.GAL.Multithreading.Commands;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Buffer;
using Ryujinx.Graphics.GAL.Multithreading.Commands.Renderer;
using Ryujinx.Graphics.GAL.Multithreading.Model;
using Ryujinx.Graphics.GAL.Multithreading.Resources;
using Ryujinx.Graphics.GAL.Multithreading.Resources.Programs;
using Ryujinx.Graphics.Shader;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Graphics.GAL.Multithreading
{
    /// <summary>
    /// The ThreadedRenderer is a layer that can be put in front of any Renderer backend to make
    /// its processing happen on a separate thread, rather than intertwined with the GPU emulation.
    /// A new thread is created to handle the GPU command processing, separate from the renderer thread.
    /// Calls to the renderer, pipeline and resources are queued to happen on the renderer thread.
    /// </summary>
    public class ThreadedRenderer : IRenderer
    {
        private const int SpanPoolBytes = 4 * 1024 * 1024;
        private const int MaxRefsPerCommand = 2;
        private const int QueueCount = 10000;

        private int _elementSize;
        private IRenderer _baseRenderer;
        private Thread _gpuThread;
        private bool _disposed;
        private bool _running;

        private AutoResetEvent _frameComplete = new AutoResetEvent(true);

        private ManualResetEventSlim _galWorkAvailable;
        private CircularSpanPool _spanPool;

        private ManualResetEventSlim _invokeRun;

        private bool _lastSampleCounterClear = true;

        private byte[] _commandQueue;
        private object[] _refQueue;

        private int _consumerPtr;
        private int _commandCount;

        private int _producerPtr;
        private int _lastProducedPtr;
        private int _invokePtr;

        private int _refProducerPtr;
        private int _refConsumerPtr;

        public event EventHandler<ScreenCaptureImageInfo> ScreenCaptured;

        internal BufferMap Buffers { get; }
        internal SyncMap Sync { get; }
        internal CircularSpanPool SpanPool { get; }
        internal ProgramQueue Programs { get; }

        public IPipeline Pipeline { get; }
        public IWindow Window { get; }

        public IRenderer BaseRenderer => _baseRenderer;

        public bool PreferThreading => _baseRenderer.PreferThreading;

        public ThreadedRenderer(IRenderer renderer)
        {
            _baseRenderer = renderer;

            renderer.ScreenCaptured += (object sender, ScreenCaptureImageInfo info) => ScreenCaptured?.Invoke(this, info);

            Pipeline = new ThreadedPipeline(this, renderer.Pipeline);
            Window = new ThreadedWindow(this, renderer.Window);
            Buffers = new BufferMap();
            Sync = new SyncMap();
            Programs = new ProgramQueue(renderer);

            _galWorkAvailable = new ManualResetEventSlim(false);
            _invokeRun = new ManualResetEventSlim();
            _spanPool = new CircularSpanPool(this, SpanPoolBytes);
            SpanPool = _spanPool;

            _elementSize = BitUtils.AlignUp(CommandHelper.GetMaxCommandSize(), 4);

            _commandQueue = new byte[_elementSize * QueueCount];
            _refQueue = new object[MaxRefsPerCommand * QueueCount];
        }

        public void RunLoop(Action gpuLoop)
        {
            _running = true;

            _gpuThread = new Thread(() => {
                gpuLoop();
                _running = false;
                _galWorkAvailable.Set();
            });

            _gpuThread.Name = "GPU.MainThread";
            _gpuThread.Start();

            RenderLoop();
        }

        public void RenderLoop()
        {
            // Power through the render queue until the Gpu thread work is done.

            while (_running && !_disposed)
            {
                _galWorkAvailable.Wait();
                _galWorkAvailable.Reset();

                // The other thread can only increase the command count.
                // We can assume that if it is above 0, it will stay there or get higher.

                while (_commandCount > 0)
                {
                    int commandPtr = _consumerPtr;

                    Span<byte> command = new Span<byte>(_commandQueue, commandPtr * _elementSize, _elementSize);

                    // Run the command.

                    CommandHelper.RunCommand(command, this, _baseRenderer);

                    if (Interlocked.CompareExchange(ref _invokePtr, -1, commandPtr) == commandPtr)
                    {
                        _invokeRun.Set();
                    }

                    _consumerPtr = (_consumerPtr + 1) % QueueCount;

                    Interlocked.Decrement(ref _commandCount);
                }
            }
        }

        internal SpanRef<T> CopySpan<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            return _spanPool.Insert(data);
        }

        private TableRef<T> Ref<T>(T reference)
        {
            return new TableRef<T>(this, reference);
        }

        internal ref T New<T>() where T : struct
        {
            while (_producerPtr == (_consumerPtr + QueueCount - 1) % QueueCount)
            {
                // If incrementing the producer pointer would overflow, we need to wait.
                // _consumerPtr can only move forward, so there's no race to worry about here.

                Thread.Sleep(1);
            }

            int taken = _producerPtr;
            _lastProducedPtr = taken;

            _producerPtr = (_producerPtr + 1) % QueueCount;

            Span<byte> memory = new Span<byte>(_commandQueue, taken * _elementSize, _elementSize);
            ref T result = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(memory));

            memory[memory.Length - 1] = (byte)((IGALCommand)result).CommandType;

            return ref result;
        }

        internal int AddTableRef(object obj)
        {
            // The reference table is sized so that it will never overflow, so long as the references are taken after the command is allocated.

            int index = _refProducerPtr;

            _refQueue[index] = obj;

            _refProducerPtr = (_refProducerPtr + 1) % _refQueue.Length;

            return index;
        }

        internal object RemoveTableRef(int index)
        {
            Debug.Assert(index == _refConsumerPtr);

            object result = _refQueue[_refConsumerPtr];
            _refQueue[_refConsumerPtr] = null;

            _refConsumerPtr = (_refConsumerPtr + 1) % _refQueue.Length;

            return result;
        }

        internal void QueueCommand()
        {
            int result = Interlocked.Increment(ref _commandCount);

            if (result == 1)
            {
                _galWorkAvailable.Set();
            }
        }

        internal void InvokeCommand()
        {
            _invokeRun.Reset();
            _invokePtr = _lastProducedPtr;

            QueueCommand();

            // Wait for the command to complete.
            _invokeRun.Wait();
        }

        internal void WaitForFrame()
        {
            _frameComplete.WaitOne();
        }

        internal void SignalFrame()
        {
            _frameComplete.Set();
        }

        internal bool IsGpuThread()
        {
            return Thread.CurrentThread == _gpuThread;
        }

        public void BackgroundContextAction(Action action, bool alwaysBackground = false)
        {
            if (IsGpuThread() && !alwaysBackground)
            {
                // The action must be performed on the render thread.
                New<ActionCommand>().Set(Ref(action));
                InvokeCommand();
            }
            else
            {
                _baseRenderer.BackgroundContextAction(action, true);
            }
        }

        public BufferHandle CreateBuffer(int size)
        {
            BufferHandle handle = Buffers.CreateBufferHandle();
            New<CreateBufferCommand>().Set(handle, size);
            QueueCommand();

            return handle;
        }

        public IProgram CreateProgram(ShaderSource[] shaders, ShaderInfo info)
        {
            var program = new ThreadedProgram(this);
            SourceProgramRequest request = new SourceProgramRequest(program, shaders, info);
            Programs.Add(request);

            New<CreateProgramCommand>().Set(Ref((IProgramRequest)request));
            QueueCommand();

            return program;
        }

        public ISampler CreateSampler(SamplerCreateInfo info)
        {
            var sampler = new ThreadedSampler(this);
            New<CreateSamplerCommand>().Set(Ref(sampler), info);
            QueueCommand();

            return sampler;
        }

        public void CreateSync(ulong id)
        {
            Sync.CreateSyncHandle(id);
            New<CreateSyncCommand>().Set(id);
            QueueCommand();
        }

        public ITexture CreateTexture(TextureCreateInfo info, float scale)
        {
            if (IsGpuThread())
            {
                var texture = new ThreadedTexture(this, info, scale);
                New<CreateTextureCommand>().Set(Ref(texture), info, scale);
                QueueCommand();

                return texture;
            }
            else
            {
                var texture = new ThreadedTexture(this, info, scale);
                texture.Base = _baseRenderer.CreateTexture(info, scale);

                return texture;
            }
        }

        public void DeleteBuffer(BufferHandle buffer)
        {
            New<BufferDisposeCommand>().Set(buffer);
            QueueCommand();
        }

        public ReadOnlySpan<byte> GetBufferData(BufferHandle buffer, int offset, int size)
        {
            if (IsGpuThread())
            {
                ResultBox<PinnedSpan<byte>> box = new ResultBox<PinnedSpan<byte>>();
                New<BufferGetDataCommand>().Set(buffer, offset, size, Ref(box));
                InvokeCommand();

                return box.Result.Get();
            }
            else
            {
                return _baseRenderer.GetBufferData(Buffers.MapBufferBlocking(buffer), offset, size);
            }
        }

        public Capabilities GetCapabilities()
        {
            ResultBox<Capabilities> box = new ResultBox<Capabilities>();
            New<GetCapabilitiesCommand>().Set(Ref(box));
            InvokeCommand();

            return box.Result;
        }

        /// <summary>
        /// Initialize the base renderer. Must be called on the render thread.
        /// </summary>
        /// <param name="logLevel">Log level to use</param>
        public void Initialize(GraphicsDebugLevel logLevel)
        {
            _baseRenderer.Initialize(logLevel);
        }

        public IProgram LoadProgramBinary(byte[] programBinary, bool hasFragmentShader, ShaderInfo info)
        {
            var program = new ThreadedProgram(this);

            BinaryProgramRequest request = new BinaryProgramRequest(program, programBinary, hasFragmentShader, info);
            Programs.Add(request);

            New<CreateProgramCommand>().Set(Ref((IProgramRequest)request));
            QueueCommand();

            return program;
        }

        public void PreFrame()
        {
            New<PreFrameCommand>();
            QueueCommand();
        }

        public ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler, bool hostReserved)
        {
            ThreadedCounterEvent evt = new ThreadedCounterEvent(this, type, _lastSampleCounterClear);
            New<ReportCounterCommand>().Set(Ref(evt), type, Ref(resultHandler), hostReserved);
            QueueCommand();

            if (type == CounterType.SamplesPassed)
            {
                _lastSampleCounterClear = false;
            }

            return evt;
        }

        public void ResetCounter(CounterType type)
        {
            New<ResetCounterCommand>().Set(type);
            QueueCommand();
            _lastSampleCounterClear = true;
        }

        public void Screenshot()
        {
            _baseRenderer.Screenshot();
        }

        public void SetBufferData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data)
        {
            New<BufferSetDataCommand>().Set(buffer, offset, CopySpan(data));
            QueueCommand();
        }

        public void UpdateCounters()
        {
            New<UpdateCountersCommand>();
            QueueCommand();
        }

        public void WaitSync(ulong id)
        {
            Sync.WaitSyncAvailability(id);

            _baseRenderer.WaitSync(id);
        }

        public void Dispose()
        {
            // Dispose must happen from the render thread, after all commands have completed.

            // Stop the GPU thread.
            _disposed = true;
            _gpuThread.Join();

            // Dispose the renderer.
            _baseRenderer.Dispose();

            // Dispose events.
            _frameComplete.Dispose();
            _galWorkAvailable.Dispose();
            _invokeRun.Dispose();

            Sync.Dispose();
        }
    }
}
