using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static Ryujinx.Graphics.Gpu.Shader.ShaderCache;

namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    class ParallelDiskCacheLoader
    {
        private const int ThreadCount = 8;

        private readonly GpuContext _context;
        private readonly ShaderCacheHashTable _graphicsCache;
        private readonly ComputeShaderCacheHashTable _computeCache;
        private readonly DiskCacheHostStorage _hostStorage;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<ShaderCacheState, int, int> _stateChangeCallback;

        /// <summary>
        /// Indicates if the cache should be loaded.
        /// </summary>
        public bool Active => !_cancellationToken.IsCancellationRequested;

        private bool _needsHostRegen;

        /// <summary>
        /// Number of shaders that failed to compile from the cache.
        /// </summary>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// Program validation entry.
        /// </summary>
        private struct ProgramEntry
        {
            /// <summary>
            /// Cached shader program.
            /// </summary>
            public readonly CachedShaderProgram CachedProgram;

            /// <summary>
            /// Host program.
            /// </summary>
            public readonly IProgram HostProgram;

            /// <summary>
            /// Program index.
            /// </summary>
            public readonly int ProgramIndex;

            /// <summary>
            /// Indicates if the program is a compute shader.
            /// </summary>
            public readonly bool IsCompute;

            /// <summary>
            /// Indicates if the program is a host binary shader.
            /// </summary>
            public readonly bool IsBinary;

            /// <summary>
            /// Creates a new program validation entry.
            /// </summary>
            /// <param name="cachedProgram">Cached shader program</param>
            /// <param name="hostProgram">Host program</param>
            /// <param name="programIndex">Program index</param>
            /// <param name="isCompute">Indicates if the program is a compute shader</param>
            /// <param name="isBinary">Indicates if the program is a host binary shader</param>
            public ProgramEntry(
                CachedShaderProgram cachedProgram,
                IProgram hostProgram,
                int programIndex,
                bool isCompute,
                bool isBinary)
            {
                CachedProgram = cachedProgram;
                HostProgram = hostProgram;
                ProgramIndex = programIndex;
                IsCompute = isCompute;
                IsBinary = isBinary;
            }
        }

        /// <summary>
        /// Translated shader compilation entry.
        /// </summary>
        private struct ProgramCompilation
        {
            /// <summary>
            /// Translated shader stages.
            /// </summary>
            public readonly ShaderProgram[] TranslatedStages;

            /// <summary>
            /// Cached shaders.
            /// </summary>
            public readonly CachedShaderStage[] Shaders;

            /// <summary>
            /// Specialization state.
            /// </summary>
            public readonly ShaderSpecializationState SpecializationState;

            /// <summary>
            /// Program index.
            /// </summary>
            public readonly int ProgramIndex;

            /// <summary>
            /// Indicates if the program is a compute shader.
            /// </summary>
            public readonly bool IsCompute;

            /// <summary>
            /// Creates a new translated shader compilation entry.
            /// </summary>
            /// <param name="translatedStages">Translated shader stages</param>
            /// <param name="shaders">Cached shaders</param>
            /// <param name="specState">Specialization state</param>
            /// <param name="programIndex">Program index</param>
            /// <param name="isCompute">Indicates if the program is a compute shader</param>
            public ProgramCompilation(
                ShaderProgram[] translatedStages,
                CachedShaderStage[] shaders,
                ShaderSpecializationState specState,
                int programIndex,
                bool isCompute)
            {
                TranslatedStages = translatedStages;
                Shaders = shaders;
                SpecializationState = specState;
                ProgramIndex = programIndex;
                IsCompute = isCompute;
            }
        }

        /// <summary>
        /// Program translation entry.
        /// </summary>
        private struct AsyncProgramTranslation
        {
            /// <summary>
            /// Cached shader stages.
            /// </summary>
            public readonly CachedShaderStage[] Shaders;

            /// <summary>
            /// Specialization state.
            /// </summary>
            public readonly ShaderSpecializationState SpecializationState;

            /// <summary>
            /// Program index.
            /// </summary>
            public readonly int ProgramIndex;

            /// <summary>
            /// Indicates if the program is a compute shader.
            /// </summary>
            public readonly bool IsCompute;

            /// <summary>
            /// Creates a new program translation entry.
            /// </summary>
            /// <param name="shaders">Cached shader stages</param>
            /// <param name="specState">Specialization state</param>
            /// <param name="programIndex">Program index</param>
            /// <param name="isCompute">Indicates if the program is a compute shader</param>
            public AsyncProgramTranslation(
                CachedShaderStage[] shaders,
                ShaderSpecializationState specState,
                int programIndex,
                bool isCompute)
            {
                Shaders = shaders;
                SpecializationState = specState;
                ProgramIndex = programIndex;
                IsCompute = isCompute;
            }
        }

        private readonly Queue<ProgramEntry> _validationQueue;
        private readonly ConcurrentQueue<ProgramCompilation> _compilationQueue;
        private readonly BlockingCollection<AsyncProgramTranslation> _asyncTranslationQueue;
        private readonly SortedList<int, CachedShaderProgram> _programList;

        private int _backendParallelCompileThreads;
        private int _compiledCount;
        private int _totalCount;

        /// <summary>
        /// Creates a new parallel disk cache loader.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="graphicsCache">Graphics shader cache</param>
        /// <param name="computeCache">Compute shader cache</param>
        /// <param name="hostStorage">Disk cache host storage</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="stateChangeCallback">Function to be called when there is a state change, reporting state, compiled and total shaders count</param>
        public ParallelDiskCacheLoader(
            GpuContext context,
            ShaderCacheHashTable graphicsCache,
            ComputeShaderCacheHashTable computeCache,
            DiskCacheHostStorage hostStorage,
            CancellationToken cancellationToken,
            Action<ShaderCacheState, int, int> stateChangeCallback)
        {
            _context = context;
            _graphicsCache = graphicsCache;
            _computeCache = computeCache;
            _hostStorage = hostStorage;
            _cancellationToken = cancellationToken;
            _stateChangeCallback = stateChangeCallback;
            _validationQueue = new Queue<ProgramEntry>();
            _compilationQueue = new ConcurrentQueue<ProgramCompilation>();
            _asyncTranslationQueue = new BlockingCollection<AsyncProgramTranslation>(ThreadCount);
            _programList = new SortedList<int, CachedShaderProgram>();
            _backendParallelCompileThreads = Math.Min(Environment.ProcessorCount, 8); // Must be kept in sync with the backend code.
        }

        /// <summary>
        /// Loads all shaders from the cache.
        /// </summary>
        public void LoadShaders()
        {
            Thread[] workThreads = new Thread[ThreadCount];

            for (int index = 0; index < ThreadCount; index++)
            {
                workThreads[index] = new Thread(ProcessAsyncQueue)
                {
                    Name = $"Gpu.AsyncTranslationThread.{index}"
                };
            }

            int programCount = _hostStorage.GetProgramCount();

            _compiledCount = 0;
            _totalCount = programCount;

            _stateChangeCallback(ShaderCacheState.Start, 0, programCount);

            Logger.Info?.Print(LogClass.Gpu, $"Loading {programCount} shaders from the cache...");

            for (int index = 0; index < ThreadCount; index++)
            {
                workThreads[index].Start(_cancellationToken);
            }

            try
            {
                _hostStorage.LoadShaders(_context, this);
            }
            catch (DiskCacheLoadException diskCacheLoadException)
            {
                Logger.Warning?.Print(LogClass.Gpu, $"Error loading the shader cache. {diskCacheLoadException.Message}");

                // If we can't even access the file, then we also can't rebuild.
                if (diskCacheLoadException.Result != DiskCacheLoadResult.NoAccess)
                {
                    _needsHostRegen = true;
                }
            }
            catch (InvalidDataException invalidDataException)
            {
                Logger.Warning?.Print(LogClass.Gpu, $"Error decompressing the shader cache file. {invalidDataException.Message}");
                _needsHostRegen = true;
            }
            catch (IOException ioException)
            {
                Logger.Warning?.Print(LogClass.Gpu, $"Error reading the shader cache file. {ioException.Message}");
                _needsHostRegen = true;
            }

            _asyncTranslationQueue.CompleteAdding();

            for (int index = 0; index < ThreadCount; index++)
            {
                workThreads[index].Join();
            }

            CheckCompilationBlocking();

            if (_needsHostRegen)
            {
                // Rebuild both shared and host cache files.
                // Rebuilding shared is required because the shader information returned by the translator
                // might have changed, and so we have to reconstruct the file with the new information.
                try
                {
                    _hostStorage.ClearSharedCache();
                    _hostStorage.ClearHostCache(_context);

                    if (_programList.Count != 0)
                    {
                        Logger.Info?.Print(LogClass.Gpu, $"Rebuilding {_programList.Count} shaders...");

                        using var streams = _hostStorage.GetOutputStreams(_context);

                        foreach (var kv in _programList)
                        {
                            if (!Active)
                            {
                                break;
                            }

                            CachedShaderProgram program = kv.Value;
                            _hostStorage.AddShader(_context, program, program.HostProgram.GetBinary(), streams);
                        }

                        Logger.Info?.Print(LogClass.Gpu, $"Rebuilt {_programList.Count} shaders successfully.");
                    }
                    else
                    {
                        _hostStorage.ClearGuestCache();

                        Logger.Info?.Print(LogClass.Gpu, "Shader cache deleted due to corruption.");
                    }
                }
                catch (DiskCacheLoadException diskCacheLoadException)
                {
                    Logger.Warning?.Print(LogClass.Gpu, $"Error deleting the shader cache. {diskCacheLoadException.Message}");
                }
                catch (IOException ioException)
                {
                    Logger.Warning?.Print(LogClass.Gpu, $"Error deleting the shader cache file. {ioException.Message}");
                }
            }

            Logger.Info?.Print(LogClass.Gpu, "Shader cache loaded.");

            _stateChangeCallback(ShaderCacheState.Loaded, programCount, programCount);
        }

        /// <summary>
        /// Enqueues a host program for compilation.
        /// </summary>
        /// <param name="cachedProgram">Cached program</param>
        /// <param name="hostProgram">Host program to be compiled</param>
        /// <param name="programIndex">Program index</param>
        /// <param name="isCompute">Indicates if the program is a compute shader</param>
        public void QueueHostProgram(CachedShaderProgram cachedProgram, IProgram hostProgram, int programIndex, bool isCompute)
        {
            EnqueueForValidation(new ProgramEntry(cachedProgram, hostProgram, programIndex, isCompute, isBinary: true));
        }

        /// <summary>
        /// Enqueues a guest program for compilation.
        /// </summary>
        /// <param name="shaders">Cached shader stages</param>
        /// <param name="specState">Specialization state</param>
        /// <param name="programIndex">Program index</param>
        /// <param name="isCompute">Indicates if the program is a compute shader</param>
        public void QueueGuestProgram(CachedShaderStage[] shaders, ShaderSpecializationState specState, int programIndex, bool isCompute)
        {
            _asyncTranslationQueue.Add(new AsyncProgramTranslation(shaders, specState, programIndex, isCompute));
        }

        /// <summary>
        /// Check the state of programs that have already been compiled,
        /// and add to the cache if the compilation was successful.
        /// </summary>
        public void CheckCompilation()
        {
            ProcessCompilationQueue();

            // Process programs that already finished compiling.
            // If not yet compiled, do nothing. This avoids blocking to wait for shader compilation.
            while (_validationQueue.TryPeek(out ProgramEntry entry))
            {
                ProgramLinkStatus result = entry.HostProgram.CheckProgramLink(false);

                if (result != ProgramLinkStatus.Incomplete)
                {
                    ProcessCompiledProgram(ref entry, result);
                    _validationQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Waits until all programs finishes compiling, then adds the ones
        /// with successful compilation to the cache.
        /// </summary>
        private void CheckCompilationBlocking()
        {
            ProcessCompilationQueue();

            while (_validationQueue.TryDequeue(out ProgramEntry entry) && Active)
            {
                ProcessCompiledProgram(ref entry, entry.HostProgram.CheckProgramLink(true), asyncCompile: false);
            }
        }

        /// <summary>
        /// Process a compiled program result.
        /// </summary>
        /// <param name="entry">Compiled program entry</param>
        /// <param name="result">Compilation result</param>
        /// <param name="asyncCompile">For failed host compilations, indicates if a guest compilation should be done asynchronously</param>
        private void ProcessCompiledProgram(ref ProgramEntry entry, ProgramLinkStatus result, bool asyncCompile = true)
        {
            if (result == ProgramLinkStatus.Success)
            {
                // Compilation successful, add to memory cache.
                if (entry.IsCompute)
                {
                    _computeCache.Add(entry.CachedProgram);
                }
                else
                {
                    _graphicsCache.Add(entry.CachedProgram);
                }

                if (!entry.IsBinary)
                {
                    _needsHostRegen = true;
                }

                _programList.Add(entry.ProgramIndex, entry.CachedProgram);
                SignalCompiled();
            }
            else if (entry.IsBinary)
            {
                // If this is a host binary and compilation failed,
                // we still have a chance to recompile from the guest binary.
                CachedShaderProgram program = entry.CachedProgram;

                if (asyncCompile)
                {
                    QueueGuestProgram(program.Shaders, program.SpecializationState, entry.ProgramIndex, entry.IsCompute);
                }
                else
                {
                    RecompileFromGuestCode(program.Shaders, program.SpecializationState, entry.ProgramIndex, entry.IsCompute);
                    ProcessCompilationQueue();
                }
            }
            else
            {
                // Failed to compile from both host and guest binary.
                ErrorCount++;
                SignalCompiled();
            }
        }

        /// <summary>
        /// Processes the queue of translated guest programs that should be compiled on the host.
        /// </summary>
        private void ProcessCompilationQueue()
        {
            while (_compilationQueue.TryDequeue(out ProgramCompilation compilation) && Active)
            {
                ShaderSource[] shaderSources = new ShaderSource[compilation.TranslatedStages.Length];

                int fragmentOutputMap = -1;

                for (int index = 0; index < compilation.TranslatedStages.Length; index++)
                {
                    ShaderProgram shader = compilation.TranslatedStages[index];
                    shaderSources[index] = CreateShaderSource(shader);

                    if (shader.Info.Stage == ShaderStage.Fragment)
                    {
                        fragmentOutputMap = shader.Info.FragmentOutputMap;
                    }
                }

                IProgram hostProgram = _context.Renderer.CreateProgram(shaderSources, new ShaderInfo(fragmentOutputMap));
                CachedShaderProgram program = new CachedShaderProgram(hostProgram, compilation.SpecializationState, compilation.Shaders);

                EnqueueForValidation(new ProgramEntry(program, hostProgram, compilation.ProgramIndex, compilation.IsCompute, isBinary: false));
            }
        }

        /// <summary>
        /// Enqueues a program for validation, which will check if the program was compiled successfully.
        /// </summary>
        /// <param name="newEntry">Program entry to be validated</param>
        private void EnqueueForValidation(ProgramEntry newEntry)
        {
            _validationQueue.Enqueue(newEntry);

            // Do not allow more than N shader compilation in-flight, where N is the maximum number of threads
            // the driver will be using for parallel compilation.
            // Submitting more seems to cause NVIDIA OpenGL driver to crash.
            if (_validationQueue.Count >= _backendParallelCompileThreads && _validationQueue.TryDequeue(out ProgramEntry entry))
            {
                ProcessCompiledProgram(ref entry, entry.HostProgram.CheckProgramLink(true), asyncCompile: false);
            }
        }

        /// <summary>
        /// Processses the queue of programs that should be translated from guest code.
        /// </summary>
        /// <param name="state">Cancellation token</param>
        private void ProcessAsyncQueue(object state)
        {
            CancellationToken ct = (CancellationToken)state;

            try
            {
                foreach (AsyncProgramTranslation asyncCompilation in _asyncTranslationQueue.GetConsumingEnumerable(ct))
                {
                    RecompileFromGuestCode(
                        asyncCompilation.Shaders,
                        asyncCompilation.SpecializationState,
                        asyncCompilation.ProgramIndex,
                        asyncCompilation.IsCompute);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// Recompiles a program from guest code.
        /// </summary>
        /// <param name="shaders">Shader stages</param>
        /// <param name="specState">Specialization state</param>
        /// <param name="programIndex">Program index</param>
        /// <param name="isCompute">Indicates if the program is a compute shader</param>
        private void RecompileFromGuestCode(CachedShaderStage[] shaders, ShaderSpecializationState specState, int programIndex, bool isCompute)
        {
            try
            {
                if (isCompute)
                {
                    RecompileComputeFromGuestCode(shaders, specState, programIndex);
                }
                else
                {
                    RecompileGraphicsFromGuestCode(shaders, specState, programIndex);
                }
            }
            catch (DiskCacheLoadException diskCacheLoadException)
            {
                Logger.Error?.Print(LogClass.Gpu, $"Error translating guest shader. {diskCacheLoadException.Message}");

                ErrorCount++;
                SignalCompiled();
            }
        }

        /// <summary>
        /// Recompiles a graphics program from guest code.
        /// </summary>
        /// <param name="shaders">Shader stages</param>
        /// <param name="specState">Specialization state</param>
        /// <param name="programIndex">Program index</param>
        private void RecompileGraphicsFromGuestCode(CachedShaderStage[] shaders, ShaderSpecializationState specState, int programIndex)
        {
            ShaderSpecializationState newSpecState = new ShaderSpecializationState(specState.GraphicsState, specState.TransformFeedbackDescriptors);
            ResourceCounts counts = new ResourceCounts();

            TranslatorContext[] translatorContexts = new TranslatorContext[Constants.ShaderStages + 1];
            TranslatorContext nextStage = null;

            for (int stageIndex = Constants.ShaderStages - 1; stageIndex >= 0; stageIndex--)
            {
                CachedShaderStage shader = shaders[stageIndex + 1];

                if (shader != null)
                {
                    byte[] guestCode = shader.Code;
                    byte[] cb1Data = shader.Cb1Data;

                    DiskCacheGpuAccessor gpuAccessor = new DiskCacheGpuAccessor(_context, guestCode, cb1Data, specState, newSpecState, counts, stageIndex);
                    TranslatorContext currentStage = DecodeGraphicsShader(gpuAccessor, DefaultFlags, 0);

                    if (nextStage != null)
                    {
                        currentStage.SetNextStage(nextStage);
                    }

                    if (stageIndex == 0 && shaders[0] != null)
                    {
                        byte[] guestCodeA = shaders[0].Code;
                        byte[] cb1DataA = shaders[0].Cb1Data;

                        DiskCacheGpuAccessor gpuAccessorA = new DiskCacheGpuAccessor(_context, guestCodeA, cb1DataA, specState, newSpecState, counts, 0);
                        translatorContexts[0] = DecodeGraphicsShader(gpuAccessorA, DefaultFlags | TranslationFlags.VertexA, 0);
                    }

                    translatorContexts[stageIndex + 1] = currentStage;
                    nextStage = currentStage;
                }
            }

            List<ShaderProgram> translatedStages = new List<ShaderProgram>();

            for (int stageIndex = 0; stageIndex < Constants.ShaderStages; stageIndex++)
            {
                TranslatorContext currentStage = translatorContexts[stageIndex + 1];

                if (currentStage != null)
                {
                    ShaderProgram program;

                    byte[] guestCode = shaders[stageIndex + 1].Code;
                    byte[] cb1Data = shaders[stageIndex + 1].Cb1Data;

                    if (stageIndex == 0 && shaders[0] != null)
                    {
                        program = currentStage.Translate(translatorContexts[0]);

                        byte[] guestCodeA = shaders[0].Code;
                        byte[] cb1DataA = shaders[0].Cb1Data;

                        shaders[0] = new CachedShaderStage(null, guestCodeA, cb1DataA);
                        shaders[1] = new CachedShaderStage(program.Info, guestCode, cb1Data);
                    }
                    else
                    {
                        program = currentStage.Translate();

                        shaders[stageIndex + 1] = new CachedShaderStage(program.Info, guestCode, cb1Data);
                    }

                    if (program != null)
                    {
                        translatedStages.Add(program);
                    }
                }
            }

            _compilationQueue.Enqueue(new ProgramCompilation(translatedStages.ToArray(), shaders, newSpecState, programIndex, isCompute: false));
        }

        /// <summary>
        /// Recompiles a compute program from guest code.
        /// </summary>
        /// <param name="shaders">Shader stages</param>
        /// <param name="specState">Specialization state</param>
        /// <param name="programIndex">Program index</param>
        private void RecompileComputeFromGuestCode(CachedShaderStage[] shaders, ShaderSpecializationState specState, int programIndex)
        {
            CachedShaderStage shader = shaders[0];
            ResourceCounts counts = new ResourceCounts();
            ShaderSpecializationState newSpecState = new ShaderSpecializationState(specState.ComputeState);
            DiskCacheGpuAccessor gpuAccessor = new DiskCacheGpuAccessor(_context, shader.Code, shader.Cb1Data, specState, newSpecState, counts, 0);

            TranslatorContext translatorContext = DecodeComputeShader(gpuAccessor, 0);

            ShaderProgram program = translatorContext.Translate();

            shaders[0] = new CachedShaderStage(program.Info, shader.Code, shader.Cb1Data);

            _compilationQueue.Enqueue(new ProgramCompilation(new[] { program }, shaders, newSpecState, programIndex, isCompute: true));
        }

        /// <summary>
        /// Signals that compilation of a program has been finished successfully,
        /// or that it failed and guest recompilation has also been attempted.
        /// </summary>
        private void SignalCompiled()
        {
            _stateChangeCallback(ShaderCacheState.Loading, ++_compiledCount, _totalCount);
        }
    }
}