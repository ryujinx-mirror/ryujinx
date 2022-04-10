using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader.Cache;
using Ryujinx.Graphics.Gpu.Shader.DiskCache;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Memory cache of shader code.
    /// </summary>
    class ShaderCache : IDisposable
    {
        /// <summary>
        /// Default flags used on the shader translation process.
        /// </summary>
        public const TranslationFlags DefaultFlags = TranslationFlags.DebugMode;

        private struct TranslatedShader
        {
            public readonly CachedShaderStage Shader;
            public readonly ShaderProgram Program;

            public TranslatedShader(CachedShaderStage shader, ShaderProgram program)
            {
                Shader = shader;
                Program = program;
            }
        }

        private struct TranslatedShaderVertexPair
        {
            public readonly CachedShaderStage VertexA;
            public readonly CachedShaderStage VertexB;
            public readonly ShaderProgram Program;

            public TranslatedShaderVertexPair(CachedShaderStage vertexA, CachedShaderStage vertexB, ShaderProgram program)
            {
                VertexA = vertexA;
                VertexB = vertexB;
                Program = program;
            }
        }

        private readonly GpuContext _context;

        private readonly ShaderDumper _dumper;

        private readonly Dictionary<ulong, CachedShaderProgram> _cpPrograms;
        private readonly Dictionary<ShaderAddresses, CachedShaderProgram> _gpPrograms;

        private struct ProgramToSave
        {
            public readonly CachedShaderProgram CachedProgram;
            public readonly IProgram HostProgram;

            public ProgramToSave(CachedShaderProgram cachedProgram, IProgram hostProgram)
            {
                CachedProgram = cachedProgram;
                HostProgram = hostProgram;
            }
        }

        private Queue<ProgramToSave> _programsToSaveQueue;

        private readonly ComputeShaderCacheHashTable _computeShaderCache;
        private readonly ShaderCacheHashTable _graphicsShaderCache;
        private readonly DiskCacheHostStorage _diskCacheHostStorage;
        private readonly BackgroundDiskCacheWriter _cacheWriter;

        /// <summary>
        /// Event for signalling shader cache loading progress.
        /// </summary>
        public event Action<ShaderCacheState, int, int> ShaderCacheStateChanged;

        /// <summary>
        /// Creates a new instance of the shader cache.
        /// </summary>
        /// <param name="context">GPU context that the shader cache belongs to</param>
        public ShaderCache(GpuContext context)
        {
            _context = context;

            _dumper = new ShaderDumper();

            _cpPrograms = new Dictionary<ulong, CachedShaderProgram>();
            _gpPrograms = new Dictionary<ShaderAddresses, CachedShaderProgram>();

            _programsToSaveQueue = new Queue<ProgramToSave>();

            string diskCacheTitleId = GraphicsConfig.EnableShaderCache && GraphicsConfig.TitleId != null
                ? CacheHelper.GetBaseCacheDirectory(GraphicsConfig.TitleId)
                : null;

            _computeShaderCache = new ComputeShaderCacheHashTable();
            _graphicsShaderCache = new ShaderCacheHashTable();
            _diskCacheHostStorage = new DiskCacheHostStorage(diskCacheTitleId);

            if (_diskCacheHostStorage.CacheEnabled)
            {
                _cacheWriter = new BackgroundDiskCacheWriter(context, _diskCacheHostStorage);
            }
        }

        /// <summary>
        /// Processes the queue of shaders that must save their binaries to the disk cache.
        /// </summary>
        public void ProcessShaderCacheQueue()
        {
            // Check to see if the binaries for previously compiled shaders are ready, and save them out.

            while (_programsToSaveQueue.TryPeek(out ProgramToSave programToSave))
            {
                ProgramLinkStatus result = programToSave.HostProgram.CheckProgramLink(false);

                if (result != ProgramLinkStatus.Incomplete)
                {
                    if (result == ProgramLinkStatus.Success)
                    {
                        _cacheWriter.AddShader(programToSave.CachedProgram, programToSave.HostProgram.GetBinary());
                    }

                    _programsToSaveQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Initialize the cache.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the shader cache initialization process</param>
        internal void Initialize(CancellationToken cancellationToken)
        {
            if (_diskCacheHostStorage.CacheEnabled)
            {
                if (!_diskCacheHostStorage.CacheExists())
                {
                    // If we don't have a shader cache on the new format, try to perform migration from the old shader cache.
                    Logger.Info?.Print(LogClass.Gpu, "No shader cache found, trying to migrate from legacy shader cache...");

                    int migrationCount = Migration.MigrateFromLegacyCache(_context, _diskCacheHostStorage);

                    Logger.Info?.Print(LogClass.Gpu, $"Migrated {migrationCount} shaders.");
                }

                ParallelDiskCacheLoader loader = new ParallelDiskCacheLoader(
                    _context,
                    _graphicsShaderCache,
                    _computeShaderCache,
                    _diskCacheHostStorage,
                    cancellationToken,
                    ShaderCacheStateUpdate);

                loader.LoadShaders();

                int errorCount = loader.ErrorCount;
                if (errorCount != 0)
                {
                    Logger.Warning?.Print(LogClass.Gpu, $"Failed to load {errorCount} shaders from the disk cache.");
                }
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
        /// Gets a compute shader from the cache.
        /// </summary>
        /// <remarks>
        /// This automatically translates, compiles and adds the code to the cache if not present.
        /// </remarks>
        /// <param name="channel">GPU channel</param>
        /// <param name="poolState">Texture pool state</param>
        /// <param name="computeState">Compute engine state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <returns>Compiled compute shader code</returns>
        public CachedShaderProgram GetComputeShader(
            GpuChannel channel,
            GpuChannelPoolState poolState,
            GpuChannelComputeState computeState,
            ulong gpuVa)
        {
            if (_cpPrograms.TryGetValue(gpuVa, out var cpShader) && IsShaderEqual(channel, poolState, cpShader, gpuVa))
            {
                return cpShader;
            }

            if (_computeShaderCache.TryFind(channel, poolState, gpuVa, out cpShader, out byte[] cachedGuestCode))
            {
                _cpPrograms[gpuVa] = cpShader;
                return cpShader;
            }

            ShaderSpecializationState specState = new ShaderSpecializationState(computeState);
            GpuAccessorState gpuAccessorState = new GpuAccessorState(poolState, computeState, default, specState);
            GpuAccessor gpuAccessor = new GpuAccessor(_context, channel, gpuAccessorState);

            TranslatorContext translatorContext = DecodeComputeShader(gpuAccessor, gpuVa);

            TranslatedShader translatedShader = TranslateShader(_dumper, channel, translatorContext, cachedGuestCode);

            IProgram hostProgram = _context.Renderer.CreateProgram(new ShaderSource[] { CreateShaderSource(translatedShader.Program) }, new ShaderInfo(-1));

            cpShader = new CachedShaderProgram(hostProgram, specState, translatedShader.Shader);

            _computeShaderCache.Add(cpShader);
            EnqueueProgramToSave(new ProgramToSave(cpShader, hostProgram));
            _cpPrograms[gpuVa] = cpShader;

            return cpShader;
        }

        /// <summary>
        /// Gets a graphics shader program from the shader cache.
        /// This includes all the specified shader stages.
        /// </summary>
        /// <remarks>
        /// This automatically translates, compiles and adds the code to the cache if not present.
        /// </remarks>
        /// <param name="state">GPU state</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="poolState">Texture pool state</param>
        /// <param name="graphicsState">3D engine state</param>
        /// <param name="addresses">Addresses of the shaders for each stage</param>
        /// <returns>Compiled graphics shader code</returns>
        public CachedShaderProgram GetGraphicsShader(
            ref ThreedClassState state,
            GpuChannel channel,
            GpuChannelPoolState poolState,
            GpuChannelGraphicsState graphicsState,
            ShaderAddresses addresses)
        {
            if (_gpPrograms.TryGetValue(addresses, out var gpShaders) && IsShaderEqual(channel, poolState, gpShaders, addresses))
            {
                return gpShaders;
            }

            if (_graphicsShaderCache.TryFind(channel, poolState, addresses, out gpShaders, out var cachedGuestCode))
            {
                _gpPrograms[addresses] = gpShaders;
                return gpShaders;
            }

            TransformFeedbackDescriptor[] transformFeedbackDescriptors = GetTransformFeedbackDescriptors(ref state);

            ShaderSpecializationState specState = new ShaderSpecializationState(graphicsState, transformFeedbackDescriptors);
            GpuAccessorState gpuAccessorState = new GpuAccessorState(poolState, default, graphicsState, specState, transformFeedbackDescriptors);

            ReadOnlySpan<ulong> addressesSpan = addresses.AsSpan();

            TranslatorContext[] translatorContexts = new TranslatorContext[Constants.ShaderStages + 1];
            TranslatorContext nextStage = null;

            for (int stageIndex = Constants.ShaderStages - 1; stageIndex >= 0; stageIndex--)
            {
                ulong gpuVa = addressesSpan[stageIndex + 1];

                if (gpuVa != 0)
                {
                    GpuAccessor gpuAccessor = new GpuAccessor(_context, channel, gpuAccessorState, stageIndex);
                    TranslatorContext currentStage = DecodeGraphicsShader(gpuAccessor, DefaultFlags, gpuVa);

                    if (nextStage != null)
                    {
                        currentStage.SetNextStage(nextStage);
                    }

                    if (stageIndex == 0 && addresses.VertexA != 0)
                    {
                        translatorContexts[0] = DecodeGraphicsShader(gpuAccessor, DefaultFlags | TranslationFlags.VertexA, addresses.VertexA);
                    }

                    translatorContexts[stageIndex + 1] = currentStage;
                    nextStage = currentStage;
                }
            }

            CachedShaderStage[] shaders = new CachedShaderStage[Constants.ShaderStages + 1];
            List<ShaderSource> shaderSources = new List<ShaderSource>();

            for (int stageIndex = 0; stageIndex < Constants.ShaderStages; stageIndex++)
            {
                TranslatorContext currentStage = translatorContexts[stageIndex + 1];

                if (currentStage != null)
                {
                    ShaderProgram program;

                    if (stageIndex == 0 && translatorContexts[0] != null)
                    {
                        TranslatedShaderVertexPair translatedShader = TranslateShader(
                            _dumper,
                            channel,
                            currentStage,
                            translatorContexts[0],
                            cachedGuestCode.VertexACode,
                            cachedGuestCode.VertexBCode);

                        shaders[0] = translatedShader.VertexA;
                        shaders[1] = translatedShader.VertexB;
                        program = translatedShader.Program;
                    }
                    else
                    {
                        byte[] code = cachedGuestCode.GetByIndex(stageIndex);

                        TranslatedShader translatedShader = TranslateShader(_dumper, channel, currentStage, code);

                        shaders[stageIndex + 1] = translatedShader.Shader;
                        program = translatedShader.Program;
                    }

                    if (program != null)
                    {
                        shaderSources.Add(CreateShaderSource(program));
                    }
                }
            }

            int fragmentOutputMap = shaders[5]?.Info.FragmentOutputMap ?? -1;
            IProgram hostProgram = _context.Renderer.CreateProgram(shaderSources.ToArray(), new ShaderInfo(fragmentOutputMap));

            gpShaders = new CachedShaderProgram(hostProgram, specState, shaders);

            _graphicsShaderCache.Add(gpShaders);
            EnqueueProgramToSave(new ProgramToSave(gpShaders, hostProgram));
            _gpPrograms[addresses] = gpShaders;

            return gpShaders;
        }

        /// <summary>
        /// Creates a shader source for use with the backend from a translated shader program.
        /// </summary>
        /// <param name="program">Translated shader program</param>
        /// <returns>Shader source</returns>
        public static ShaderSource CreateShaderSource(ShaderProgram program)
        {
            return new ShaderSource(program.Code, program.BinaryCode, program.Info.Stage, program.Language);
        }

        /// <summary>
        /// Puts a program on the queue of programs to be saved on the disk cache.
        /// </summary>
        /// <remarks>
        /// This will not do anything if disk shader cache is disabled.
        /// </remarks>
        /// <param name="programToSave">Program to be saved on disk</param>
        private void EnqueueProgramToSave(ProgramToSave programToSave)
        {
            if (_diskCacheHostStorage.CacheEnabled)
            {
                _programsToSaveQueue.Enqueue(programToSave);
            }
        }

        /// <summary>
        /// Gets transform feedback state from the current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <returns>Four transform feedback descriptors for the enabled TFBs, or null if TFB is disabled</returns>
        private static TransformFeedbackDescriptor[] GetTransformFeedbackDescriptors(ref ThreedClassState state)
        {
            bool tfEnable = state.TfEnable;
            if (!tfEnable)
            {
                return null;
            }

            TransformFeedbackDescriptor[] descs = new TransformFeedbackDescriptor[Constants.TotalTransformFeedbackBuffers];

            for (int i = 0; i < Constants.TotalTransformFeedbackBuffers; i++)
            {
                var tf = state.TfState[i];

                descs[i] = new TransformFeedbackDescriptor(
                    tf.BufferIndex,
                    tf.Stride,
                    tf.VaryingsCount,
                    ref state.TfVaryingLocations[i]);
            }

            return descs;
        }

        /// <summary>
        /// Checks if compute shader code in memory is equal to the cached shader.
        /// </summary>
        /// <param name="channel">GPU channel using the shader</param>
        /// <param name="poolState">GPU channel state to verify shader compatibility</param>
        /// <param name="cpShader">Cached compute shader</param>
        /// <param name="gpuVa">GPU virtual address of the shader code in memory</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private static bool IsShaderEqual(
            GpuChannel channel,
            GpuChannelPoolState poolState,
            CachedShaderProgram cpShader,
            ulong gpuVa)
        {
            if (IsShaderEqual(channel.MemoryManager, cpShader.Shaders[0], gpuVa))
            {
                return cpShader.SpecializationState.MatchesCompute(channel, poolState);
            }

            return false;
        }

        /// <summary>
        /// Checks if graphics shader code from all stages in memory are equal to the cached shaders.
        /// </summary>
        /// <param name="channel">GPU channel using the shader</param>
        /// <param name="poolState">GPU channel state to verify shader compatibility</param>
        /// <param name="gpShaders">Cached graphics shaders</param>
        /// <param name="addresses">GPU virtual addresses of all enabled shader stages</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private static bool IsShaderEqual(
            GpuChannel channel,
            GpuChannelPoolState poolState,
            CachedShaderProgram gpShaders,
            ShaderAddresses addresses)
        {
            ReadOnlySpan<ulong> addressesSpan = addresses.AsSpan();

            for (int stageIndex = 0; stageIndex < gpShaders.Shaders.Length; stageIndex++)
            {
                CachedShaderStage shader = gpShaders.Shaders[stageIndex];

                ulong gpuVa = addressesSpan[stageIndex];

                if (!IsShaderEqual(channel.MemoryManager, shader, gpuVa))
                {
                    return false;
                }
            }

            return gpShaders.SpecializationState.MatchesGraphics(channel, poolState);
        }

        /// <summary>
        /// Checks if the code of the specified cached shader is different from the code in memory.
        /// </summary>
        /// <param name="memoryManager">Memory manager used to access the GPU memory where the shader is located</param>
        /// <param name="shader">Cached shader to compare with</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private static bool IsShaderEqual(MemoryManager memoryManager, CachedShaderStage shader, ulong gpuVa)
        {
            if (shader == null)
            {
                return true;
            }

            ReadOnlySpan<byte> memoryCode = memoryManager.GetSpan(gpuVa, shader.Code.Length);

            return memoryCode.SequenceEqual(shader.Code);
        }

        /// <summary>
        /// Decode the binary Maxwell shader code to a translator context.
        /// </summary>
        /// <param name="gpuAccessor">GPU state accessor</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <returns>The generated translator context</returns>
        public static TranslatorContext DecodeComputeShader(IGpuAccessor gpuAccessor, ulong gpuVa)
        {
            var options = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, DefaultFlags | TranslationFlags.Compute);
            return Translator.CreateContext(gpuVa, gpuAccessor, options);
        }

        /// <summary>
        /// Decode the binary Maxwell shader code to a translator context.
        /// </summary>
        /// <remarks>
        /// This will combine the "Vertex A" and "Vertex B" shader stages, if specified, into one shader.
        /// </remarks>
        /// <param name="gpuAccessor">GPU state accessor</param>
        /// <param name="flags">Flags that controls shader translation</param>
        /// <param name="gpuVa">GPU virtual address of the shader code</param>
        /// <returns>The generated translator context</returns>
        public static TranslatorContext DecodeGraphicsShader(IGpuAccessor gpuAccessor, TranslationFlags flags, ulong gpuVa)
        {
            var options = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, flags);
            return Translator.CreateContext(gpuVa, gpuAccessor, options);
        }

        /// <summary>
        /// Translates a previously generated translator context to something that the host API accepts.
        /// </summary>
        /// <param name="dumper">Optional shader code dumper</param>
        /// <param name="channel">GPU channel using the shader</param>
        /// <param name="currentStage">Translator context of the stage to be translated</param>
        /// <param name="vertexA">Optional translator context of the shader that should be combined</param>
        /// <param name="codeA">Optional Maxwell binary code of the Vertex A shader, if present</param>
        /// <param name="codeB">Optional Maxwell binary code of the Vertex B or current stage shader, if present on cache</param>
        /// <returns>Compiled graphics shader code</returns>
        private static TranslatedShaderVertexPair TranslateShader(
            ShaderDumper dumper,
            GpuChannel channel,
            TranslatorContext currentStage,
            TranslatorContext vertexA,
            byte[] codeA,
            byte[] codeB)
        {
            ulong cb1DataAddress = channel.BufferManager.GetGraphicsUniformBufferAddress(0, 1);

            var memoryManager = channel.MemoryManager;

            codeA ??= memoryManager.GetSpan(vertexA.Address, vertexA.Size).ToArray();
            codeB ??= memoryManager.GetSpan(currentStage.Address, currentStage.Size).ToArray();
            byte[] cb1DataA = memoryManager.Physical.GetSpan(cb1DataAddress, vertexA.Cb1DataSize).ToArray();
            byte[] cb1DataB = memoryManager.Physical.GetSpan(cb1DataAddress, currentStage.Cb1DataSize).ToArray();

            ShaderDumpPaths pathsA = default;
            ShaderDumpPaths pathsB = default;

            if (dumper != null)
            {
                pathsA = dumper.Dump(codeA, compute: false);
                pathsB = dumper.Dump(codeB, compute: false);
            }

            ShaderProgram program = currentStage.Translate(vertexA);

            pathsB.Prepend(program);
            pathsA.Prepend(program);

            CachedShaderStage vertexAStage = new CachedShaderStage(null, codeA, cb1DataA);
            CachedShaderStage vertexBStage = new CachedShaderStage(program.Info, codeB, cb1DataB);

            return new TranslatedShaderVertexPair(vertexAStage, vertexBStage, program);
        }

        /// <summary>
        /// Translates a previously generated translator context to something that the host API accepts.
        /// </summary>
        /// <param name="dumper">Optional shader code dumper</param>
        /// <param name="channel">GPU channel using the shader</param>
        /// <param name="context">Translator context of the stage to be translated</param>
        /// <param name="code">Optional Maxwell binary code of the current stage shader, if present on cache</param>
        /// <returns>Compiled graphics shader code</returns>
        private static TranslatedShader TranslateShader(ShaderDumper dumper, GpuChannel channel, TranslatorContext context, byte[] code)
        {
            var memoryManager = channel.MemoryManager;

            ulong cb1DataAddress = context.Stage == ShaderStage.Compute
                ? channel.BufferManager.GetComputeUniformBufferAddress(1)
                : channel.BufferManager.GetGraphicsUniformBufferAddress(StageToStageIndex(context.Stage), 1);

            byte[] cb1Data = memoryManager.Physical.GetSpan(cb1DataAddress, context.Cb1DataSize).ToArray();
            code ??= memoryManager.GetSpan(context.Address, context.Size).ToArray();

            ShaderDumpPaths paths = dumper?.Dump(code, context.Stage == ShaderStage.Compute) ?? default;
            ShaderProgram program = context.Translate();

            paths.Prepend(program);

            return new TranslatedShader(new CachedShaderStage(program.Info, code, cb1Data), program);
        }

        /// <summary>
        /// Gets the index of a stage from a <see cref="ShaderStage"/>.
        /// </summary>
        /// <param name="stage">Stage to get the index from</param>
        /// <returns>Stage index</returns>
        private static int StageToStageIndex(ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.TessellationControl => 1,
                ShaderStage.TessellationEvaluation => 2,
                ShaderStage.Geometry => 3,
                ShaderStage.Fragment => 4,
                _ => 0
            };
        }

        /// <summary>
        /// Disposes the shader cache, deleting all the cached shaders.
        /// It's an error to use the shader cache after disposal.
        /// </summary>
        public void Dispose()
        {
            foreach (CachedShaderProgram program in _graphicsShaderCache.GetPrograms())
            {
                program.Dispose();
            }

            foreach (CachedShaderProgram program in _computeShaderCache.GetPrograms())
            {
                program.Dispose();
            }

            _cacheWriter?.Dispose();
        }
    }
}
