using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader.Cache;
using Ryujinx.Graphics.Gpu.Shader.Cache.Definition;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Memory cache of shader code.
    /// </summary>
    class ShaderCache : IDisposable
    {
        private const TranslationFlags DefaultFlags = TranslationFlags.DebugMode;

        private readonly GpuContext _context;

        private readonly ShaderDumper _dumper;

        private readonly Dictionary<ulong, List<ShaderBundle>> _cpPrograms;
        private readonly Dictionary<ShaderAddresses, List<ShaderBundle>> _gpPrograms;

        private CacheManager _cacheManager;

        private Dictionary<Hash128, ShaderBundle> _gpProgramsDiskCache;
        private Dictionary<Hash128, ShaderBundle> _cpProgramsDiskCache;

        /// <summary>
        /// Version of the codegen (to be changed when codegen or guest format change).
        /// </summary>
        private const ulong ShaderCodeGenVersion = 2538;

        // Progress reporting helpers
        private volatile int _shaderCount;
        private volatile int _totalShaderCount;
        public event Action<ShaderCacheState, int, int> ShaderCacheStateChanged;

        /// <summary>
        /// Creates a new instance of the shader cache.
        /// </summary>
        /// <param name="context">GPU context that the shader cache belongs to</param>
        public ShaderCache(GpuContext context)
        {
            _context = context;

            _dumper = new ShaderDumper();

            _cpPrograms = new Dictionary<ulong, List<ShaderBundle>>();
            _gpPrograms = new Dictionary<ShaderAddresses, List<ShaderBundle>>();
            _gpProgramsDiskCache = new Dictionary<Hash128, ShaderBundle>();
            _cpProgramsDiskCache = new Dictionary<Hash128, ShaderBundle>();
        }

        /// <summary>
        /// Initialize the cache.
        /// </summary>
        internal void Initialize()
        {
            if (GraphicsConfig.EnableShaderCache && GraphicsConfig.TitleId != null)
            {
                _cacheManager = new CacheManager(CacheGraphicsApi.OpenGL, CacheHashType.XxHash128, "glsl", GraphicsConfig.TitleId, ShaderCodeGenVersion);

                bool isReadOnly = _cacheManager.IsReadOnly;

                HashSet<Hash128> invalidEntries = null;

                if (isReadOnly)
                {
                    Logger.Warning?.Print(LogClass.Gpu, "Loading shader cache in read-only mode (cache in use by another program!)");
                }
                else
                {
                    invalidEntries = new HashSet<Hash128>();
                }

                ReadOnlySpan<Hash128> guestProgramList = _cacheManager.GetGuestProgramList();

                using AutoResetEvent progressReportEvent = new AutoResetEvent(false);

                _shaderCount = 0;
                _totalShaderCount = guestProgramList.Length;

                ShaderCacheStateChanged?.Invoke(ShaderCacheState.Start, _shaderCount, _totalShaderCount);
                Thread progressReportThread = null;

                if (guestProgramList.Length > 0)
                {
                    progressReportThread = new Thread(ReportProgress)
                    {
                        Name = "ShaderCache.ProgressReporter",
                        Priority = ThreadPriority.Lowest,
                        IsBackground = true
                    };

                    progressReportThread.Start(progressReportEvent);
                }

                // Make sure these are initialized before doing compilation.
                Capabilities caps = _context.Capabilities;

                int maxTaskCount = Math.Min(Environment.ProcessorCount, 8);
                int programIndex = 0;
                List<ShaderCompileTask> activeTasks = new List<ShaderCompileTask>();

                using AutoResetEvent taskDoneEvent = new AutoResetEvent(false);

                // This thread dispatches tasks to do shader translation, and creates programs that OpenGL will link in the background.
                // The program link status is checked in a non-blocking manner so that multiple shaders can be compiled at once.

                while (programIndex < guestProgramList.Length || activeTasks.Count > 0)
                {
                    if (activeTasks.Count < maxTaskCount && programIndex < guestProgramList.Length)
                    {
                        // Begin a new shader compilation.
                        Hash128 key = guestProgramList[programIndex];

                        byte[] hostProgramBinary = _cacheManager.GetHostProgramByHash(ref key);
                        bool hasHostCache = hostProgramBinary != null;

                        IProgram hostProgram = null;

                        // If the program sources aren't in the cache, compile from saved guest program.
                        byte[] guestProgram = _cacheManager.GetGuestProgramByHash(ref key);

                        if (guestProgram == null)
                        {
                            Logger.Error?.Print(LogClass.Gpu, $"Ignoring orphan shader hash {key} in cache (is the cache incomplete?)");

                            // Should not happen, but if someone messed with the cache it's better to catch it.
                            invalidEntries?.Add(key);

                            _shaderCount = ++programIndex;

                            continue;
                        }

                        ReadOnlySpan<byte> guestProgramReadOnlySpan = guestProgram;

                        ReadOnlySpan<GuestShaderCacheEntry> cachedShaderEntries = GuestShaderCacheEntry.Parse(ref guestProgramReadOnlySpan, out GuestShaderCacheHeader fileHeader);

                        if (cachedShaderEntries[0].Header.Stage == ShaderStage.Compute)
                        {
                            Debug.Assert(cachedShaderEntries.Length == 1);

                            GuestShaderCacheEntry entry = cachedShaderEntries[0];

                            HostShaderCacheEntry[] hostShaderEntries = null;

                            // Try loading host shader binary.
                            if (hasHostCache)
                            {
                                hostShaderEntries = HostShaderCacheEntry.Parse(hostProgramBinary, out ReadOnlySpan<byte> hostProgramBinarySpan);
                                hostProgramBinary = hostProgramBinarySpan.ToArray();
                                hostProgram = _context.Renderer.LoadProgramBinary(hostProgramBinary);
                            }

                            ShaderCompileTask task = new ShaderCompileTask(taskDoneEvent);
                            activeTasks.Add(task);

                            task.OnCompiled(hostProgram, (bool isHostProgramValid, ShaderCompileTask task) =>
                            {
                                ShaderProgram program = null;
                                ShaderProgramInfo shaderProgramInfo = null;

                                if (isHostProgramValid)
                                {
                                    // Reconstruct code holder.

                                    program = new ShaderProgram(entry.Header.Stage, "");
                                    shaderProgramInfo = hostShaderEntries[0].ToShaderProgramInfo();

                                    ShaderCodeHolder shader = new ShaderCodeHolder(program, shaderProgramInfo, entry.Code);

                                    _cpProgramsDiskCache.Add(key, new ShaderBundle(hostProgram, shader));

                                    return true;
                                }
                                else
                                {
                                    // If the host program was rejected by the gpu driver or isn't in cache, try to build from program sources again.

                                    Task compileTask = Task.Run(() =>
                                    {
                                        var binaryCode = new Memory<byte>(entry.Code);

                                        var gpuAccessor = new CachedGpuAccessor(
                                            _context,
                                            binaryCode,
                                            binaryCode.Slice(binaryCode.Length - entry.Header.Cb1DataSize),
                                            entry.Header.GpuAccessorHeader,
                                            entry.TextureDescriptors);

                                        var options = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, DefaultFlags | TranslationFlags.Compute);
                                        program = Translator.CreateContext(0, gpuAccessor, options).Translate(out shaderProgramInfo);
                                    });

                                    task.OnTask(compileTask, (bool _, ShaderCompileTask task) =>
                                    {
                                        if (task.IsFaulted)
                                        {
                                            Logger.Warning?.Print(LogClass.Gpu, $"Host shader {key} is corrupted or incompatible, discarding...");

                                            _cacheManager.RemoveProgram(ref key);
                                            return true; // Exit early, the decoding step failed.
                                        }

                                        ShaderCodeHolder shader = new ShaderCodeHolder(program, shaderProgramInfo, entry.Code);

                                        Logger.Info?.Print(LogClass.Gpu, $"Host shader {key} got invalidated, rebuilding from guest...");

                                        // Compile shader and create program as the shader program binary got invalidated.
                                        shader.HostShader = _context.Renderer.CompileShader(ShaderStage.Compute, program.Code);
                                        hostProgram = _context.Renderer.CreateProgram(new IShader[] { shader.HostShader }, null);

                                        task.OnCompiled(hostProgram, (bool isNewProgramValid, ShaderCompileTask task) =>
                                        {
                                            // As the host program was invalidated, save the new entry in the cache.
                                            hostProgramBinary = HostShaderCacheEntry.Create(hostProgram.GetBinary(), new ShaderCodeHolder[] { shader });

                                            if (!isReadOnly)
                                            {
                                                if (hasHostCache)
                                                {
                                                    _cacheManager.ReplaceHostProgram(ref key, hostProgramBinary);
                                                }
                                                else
                                                {
                                                    Logger.Warning?.Print(LogClass.Gpu, $"Add missing host shader {key} in cache (is the cache incomplete?)");

                                                    _cacheManager.AddHostProgram(ref key, hostProgramBinary);
                                                }
                                            }

                                            _cpProgramsDiskCache.Add(key, new ShaderBundle(hostProgram, shader));

                                            return true;
                                        });

                                        return false; // Not finished: still need to compile the host program.
                                    });

                                    return false; // Not finished: translating the program.
                                }
                            });
                        }
                        else
                        {
                            Debug.Assert(cachedShaderEntries.Length == Constants.ShaderStages);

                            ShaderCodeHolder[] shaders = new ShaderCodeHolder[cachedShaderEntries.Length];
                            List<ShaderProgram> shaderPrograms = new List<ShaderProgram>();

                            TransformFeedbackDescriptor[] tfd = CacheHelper.ReadTransformFeedbackInformation(ref guestProgramReadOnlySpan, fileHeader);

                            TranslationFlags flags = DefaultFlags;

                            if (tfd != null)
                            {
                                flags |= TranslationFlags.Feedback;
                            }

                            TranslationCounts counts = new TranslationCounts();

                            HostShaderCacheEntry[] hostShaderEntries = null;

                            // Try loading host shader binary.
                            if (hasHostCache)
                            {
                                hostShaderEntries = HostShaderCacheEntry.Parse(hostProgramBinary, out ReadOnlySpan<byte> hostProgramBinarySpan);
                                hostProgramBinary = hostProgramBinarySpan.ToArray();
                                hostProgram = _context.Renderer.LoadProgramBinary(hostProgramBinary);
                            }

                            ShaderCompileTask task = new ShaderCompileTask(taskDoneEvent);
                            activeTasks.Add(task);

                            GuestShaderCacheEntry[] entries = cachedShaderEntries.ToArray();

                            task.OnCompiled(hostProgram, (bool isHostProgramValid, ShaderCompileTask task) =>
                            {
                                Task compileTask = Task.Run(() =>
                                {
                                    TranslatorContext[] shaderContexts = null;

                                    if (!isHostProgramValid)
                                    {
                                        shaderContexts = new TranslatorContext[1 + entries.Length];

                                        for (int i = 0; i < entries.Length; i++)
                                        {
                                            GuestShaderCacheEntry entry = entries[i];

                                            if (entry == null)
                                            {
                                                continue;
                                            }

                                            var binaryCode = new Memory<byte>(entry.Code);

                                            var gpuAccessor = new CachedGpuAccessor(
                                                _context,
                                                binaryCode,
                                                binaryCode.Slice(binaryCode.Length - entry.Header.Cb1DataSize),
                                                entry.Header.GpuAccessorHeader,
                                                entry.TextureDescriptors);

                                            var options = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, flags);

                                            shaderContexts[i + 1] = Translator.CreateContext(0, gpuAccessor, options, counts);

                                            if (entry.Header.SizeA != 0)
                                            {
                                                var options2 = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, flags | TranslationFlags.VertexA);

                                                shaderContexts[0] = Translator.CreateContext((ulong)entry.Header.Size, gpuAccessor, options2, counts);
                                            }
                                        }
                                    }

                                    // Reconstruct code holder.
                                    for (int i = 0; i < entries.Length; i++)
                                    {
                                        GuestShaderCacheEntry entry = entries[i];

                                        if (entry == null)
                                        {
                                            continue;
                                        }

                                        ShaderProgram program;
                                        ShaderProgramInfo shaderProgramInfo;

                                        if (isHostProgramValid)
                                        {
                                            program = new ShaderProgram(entry.Header.Stage, "");
                                            shaderProgramInfo = hostShaderEntries[i].ToShaderProgramInfo();
                                        }
                                        else
                                        {
                                            int stageIndex = i + 1;

                                            TranslatorContext currentStage = shaderContexts[stageIndex];
                                            TranslatorContext nextStage = GetNextStageContext(shaderContexts, stageIndex);
                                            TranslatorContext vertexA = stageIndex == 1 ? shaderContexts[0] : null;

                                            program = currentStage.Translate(out shaderProgramInfo, nextStage, vertexA);
                                        }

                                        // NOTE: Vertex B comes first in the shader cache.
                                        byte[] code = entry.Code.AsSpan().Slice(0, entry.Header.Size - entry.Header.Cb1DataSize).ToArray();
                                        byte[] code2 = entry.Header.SizeA != 0 ? entry.Code.AsSpan().Slice(entry.Header.Size, entry.Header.SizeA).ToArray() : null;

                                        shaders[i] = new ShaderCodeHolder(program, shaderProgramInfo, code, code2);

                                        shaderPrograms.Add(program);
                                    }
                                });

                                task.OnTask(compileTask, (bool _, ShaderCompileTask task) =>
                                {
                                    if (task.IsFaulted)
                                    {
                                        Logger.Warning?.Print(LogClass.Gpu, $"Host shader {key} is corrupted or incompatible, discarding...");

                                        _cacheManager.RemoveProgram(ref key);
                                        return true; // Exit early, the decoding step failed.
                                    }

                                    // If the host program was rejected by the gpu driver or isn't in cache, try to build from program sources again.
                                    if (!isHostProgramValid)
                                    {
                                        Logger.Info?.Print(LogClass.Gpu, $"Host shader {key} got invalidated, rebuilding from guest...");

                                        List<IShader> hostShaders = new List<IShader>();

                                        // Compile shaders and create program as the shader program binary got invalidated.
                                        for (int stage = 0; stage < Constants.ShaderStages; stage++)
                                        {
                                            ShaderProgram program = shaders[stage]?.Program;

                                            if (program == null)
                                            {
                                                continue;
                                            }

                                            IShader hostShader = _context.Renderer.CompileShader(program.Stage, program.Code);

                                            shaders[stage].HostShader = hostShader;

                                            hostShaders.Add(hostShader);
                                        }

                                        hostProgram = _context.Renderer.CreateProgram(hostShaders.ToArray(), tfd);

                                        task.OnCompiled(hostProgram, (bool isNewProgramValid, ShaderCompileTask task) =>
                                        {
                                            // As the host program was invalidated, save the new entry in the cache.
                                            hostProgramBinary = HostShaderCacheEntry.Create(hostProgram.GetBinary(), shaders);

                                            if (!isReadOnly)
                                            {
                                                if (hasHostCache)
                                                {
                                                    _cacheManager.ReplaceHostProgram(ref key, hostProgramBinary);
                                                }
                                                else
                                                {
                                                    Logger.Warning?.Print(LogClass.Gpu, $"Add missing host shader {key} in cache (is the cache incomplete?)");

                                                    _cacheManager.AddHostProgram(ref key, hostProgramBinary);
                                                }
                                            }

                                            _gpProgramsDiskCache.Add(key, new ShaderBundle(hostProgram, shaders));

                                            return true;
                                        });

                                        return false; // Not finished: still need to compile the host program.
                                    }
                                    else
                                    {
                                        _gpProgramsDiskCache.Add(key, new ShaderBundle(hostProgram, shaders));

                                        return true;
                                    }
                                });

                                return false; // Not finished: translating the program.
                            });
                        }

                        _shaderCount = ++programIndex;
                    }

                    // Process the queue.
                    for (int i = 0; i < activeTasks.Count; i++)
                    {
                        ShaderCompileTask task = activeTasks[i];

                        if (task.IsDone())
                        {
                            activeTasks.RemoveAt(i--);
                        }
                    }

                    if (activeTasks.Count == maxTaskCount)
                    {
                        // Wait for a task to be done, or for 1ms.
                        // Host shader compilation cannot signal when it is done,
                        // so the 1ms timeout is required to poll status.

                        taskDoneEvent.WaitOne(1);
                    }
                }

                if (!isReadOnly)
                {
                    // Remove entries that are broken in the cache
                    _cacheManager.RemoveManifestEntries(invalidEntries);
                    _cacheManager.FlushToArchive();
                    _cacheManager.Synchronize();
                }

                progressReportEvent.Set();
                progressReportThread?.Join();

                ShaderCacheStateChanged?.Invoke(ShaderCacheState.Loaded, _shaderCount, _totalShaderCount);

                Logger.Info?.Print(LogClass.Gpu, $"Shader cache loaded {_shaderCount} entries.");
            }
        }

        /// <summary>
        /// Raises ShaderCacheStateChanged events periodically.
        /// </summary>
        private void ReportProgress(object state)
        {
            const int refreshRate = 50; // ms

            AutoResetEvent endEvent = (AutoResetEvent)state;

            int count = 0;

            do
            {
                int newCount = _shaderCount;

                if (count != newCount)
                {
                    ShaderCacheStateChanged?.Invoke(ShaderCacheState.Loading, newCount, _totalShaderCount);
                    count = newCount;
                }
            }
            while (!endEvent.WaitOne(refreshRate));
        }

        /// <summary>
        /// Gets a compute shader from the cache.
        /// </summary>
        /// <remarks>
        /// This automatically translates, compiles and adds the code to the cache if not present.
        /// </remarks>
        /// <param name="channel">GPU channel</param>
        /// <param name="gas">GPU accessor state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        public ShaderBundle GetComputeShader(
            GpuChannel channel,
            GpuAccessorState gas,
            ulong gpuVa,
            int localSizeX,
            int localSizeY,
            int localSizeZ,
            int localMemorySize,
            int sharedMemorySize)
        {
            bool isCached = _cpPrograms.TryGetValue(gpuVa, out List<ShaderBundle> list);

            if (isCached)
            {
                foreach (ShaderBundle cachedCpShader in list)
                {
                    if (IsShaderEqual(channel.MemoryManager, cachedCpShader, gpuVa))
                    {
                        return cachedCpShader;
                    }
                }
            }

            TranslatorContext[] shaderContexts = new TranslatorContext[1];

            shaderContexts[0] = DecodeComputeShader(
                channel,
                gas,
                gpuVa,
                localSizeX,
                localSizeY,
                localSizeZ,
                localMemorySize,
                sharedMemorySize);

            bool isShaderCacheEnabled = _cacheManager != null;
            bool isShaderCacheReadOnly = false;

            Hash128 programCodeHash = default;
            GuestShaderCacheEntry[] shaderCacheEntries = null;

            // Current shader cache doesn't support bindless textures
            if (shaderContexts[0].UsedFeatures.HasFlag(FeatureFlags.Bindless))
            {
                isShaderCacheEnabled = false;
            }

            if (isShaderCacheEnabled)
            {
                isShaderCacheReadOnly = _cacheManager.IsReadOnly;

                // Compute hash and prepare data for shader disk cache comparison.
                shaderCacheEntries = CacheHelper.CreateShaderCacheEntries(channel, shaderContexts);
                programCodeHash = CacheHelper.ComputeGuestHashFromCache(shaderCacheEntries);
            }

            ShaderBundle cpShader;

            // Search for the program hash in loaded shaders.
            if (!isShaderCacheEnabled || !_cpProgramsDiskCache.TryGetValue(programCodeHash, out cpShader))
            {
                if (isShaderCacheEnabled)
                {
                    Logger.Debug?.Print(LogClass.Gpu, $"Shader {programCodeHash} not in cache, compiling!");
                }

                // The shader isn't currently cached, translate it and compile it.
                ShaderCodeHolder shader = TranslateShader(_dumper, channel.MemoryManager, shaderContexts[0], null, null);

                shader.HostShader = _context.Renderer.CompileShader(ShaderStage.Compute, shader.Program.Code);

                IProgram hostProgram = _context.Renderer.CreateProgram(new IShader[] { shader.HostShader }, null);

                hostProgram.CheckProgramLink(true);

                byte[] hostProgramBinary = HostShaderCacheEntry.Create(hostProgram.GetBinary(), new ShaderCodeHolder[] { shader });

                cpShader = new ShaderBundle(hostProgram, shader);

                if (isShaderCacheEnabled)
                {
                    _cpProgramsDiskCache.Add(programCodeHash, cpShader);

                    if (!isShaderCacheReadOnly)
                    {
                        _cacheManager.SaveProgram(ref programCodeHash, CacheHelper.CreateGuestProgramDump(shaderCacheEntries), hostProgramBinary);
                    }
                }
            }

            if (!isCached)
            {
                list = new List<ShaderBundle>();

                _cpPrograms.Add(gpuVa, list);
            }

            list.Add(cpShader);

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
        /// <param name="gas">GPU accessor state</param>
        /// <param name="addresses">Addresses of the shaders for each stage</param>
        /// <returns>Compiled graphics shader code</returns>
        public ShaderBundle GetGraphicsShader(ref ThreedClassState state, GpuChannel channel, GpuAccessorState gas, ShaderAddresses addresses)
        {
            bool isCached = _gpPrograms.TryGetValue(addresses, out List<ShaderBundle> list);

            if (isCached)
            {
                foreach (ShaderBundle cachedGpShaders in list)
                {
                    if (IsShaderEqual(channel.MemoryManager, cachedGpShaders, addresses))
                    {
                        return cachedGpShaders;
                    }
                }
            }

            TranslatorContext[] shaderContexts = new TranslatorContext[Constants.ShaderStages + 1];

            TransformFeedbackDescriptor[] tfd = GetTransformFeedbackDescriptors(ref state);

            TranslationFlags flags = DefaultFlags;

            if (tfd != null)
            {
                flags |= TranslationFlags.Feedback;
            }

            TranslationCounts counts = new TranslationCounts();

            if (addresses.VertexA != 0)
            {
                shaderContexts[0] = DecodeGraphicsShader(channel, gas, counts, flags | TranslationFlags.VertexA, ShaderStage.Vertex, addresses.VertexA);
            }

            shaderContexts[1] = DecodeGraphicsShader(channel, gas, counts, flags, ShaderStage.Vertex, addresses.Vertex);
            shaderContexts[2] = DecodeGraphicsShader(channel, gas, counts, flags, ShaderStage.TessellationControl, addresses.TessControl);
            shaderContexts[3] = DecodeGraphicsShader(channel, gas, counts, flags, ShaderStage.TessellationEvaluation, addresses.TessEvaluation);
            shaderContexts[4] = DecodeGraphicsShader(channel, gas, counts, flags, ShaderStage.Geometry, addresses.Geometry);
            shaderContexts[5] = DecodeGraphicsShader(channel, gas, counts, flags, ShaderStage.Fragment, addresses.Fragment);

            bool isShaderCacheEnabled = _cacheManager != null;
            bool isShaderCacheReadOnly = false;

            Hash128 programCodeHash = default;
            GuestShaderCacheEntry[] shaderCacheEntries = null;

            // Current shader cache doesn't support bindless textures
            for (int i = 0; i < shaderContexts.Length; i++)
            {
                if (shaderContexts[i] != null && shaderContexts[i].UsedFeatures.HasFlag(FeatureFlags.Bindless))
                {
                    isShaderCacheEnabled = false;
                    break;
                }
            }

            if (isShaderCacheEnabled)
            {
                isShaderCacheReadOnly = _cacheManager.IsReadOnly;

                // Compute hash and prepare data for shader disk cache comparison.
                shaderCacheEntries = CacheHelper.CreateShaderCacheEntries(channel, shaderContexts);
                programCodeHash = CacheHelper.ComputeGuestHashFromCache(shaderCacheEntries, tfd);
            }

            ShaderBundle gpShaders;

            // Search for the program hash in loaded shaders.
            if (!isShaderCacheEnabled || !_gpProgramsDiskCache.TryGetValue(programCodeHash, out gpShaders))
            {
                if (isShaderCacheEnabled)
                {
                    Logger.Debug?.Print(LogClass.Gpu, $"Shader {programCodeHash} not in cache, compiling!");
                }

                // The shader isn't currently cached, translate it and compile it.
                ShaderCodeHolder[] shaders = new ShaderCodeHolder[Constants.ShaderStages];

                for (int stageIndex = 0; stageIndex < Constants.ShaderStages; stageIndex++)
                {
                    shaders[stageIndex] = TranslateShader(_dumper, channel.MemoryManager, shaderContexts, stageIndex + 1);
                }

                List<IShader> hostShaders = new List<IShader>();

                for (int stage = 0; stage < Constants.ShaderStages; stage++)
                {
                    ShaderProgram program = shaders[stage]?.Program;

                    if (program == null)
                    {
                        continue;
                    }

                    IShader hostShader = _context.Renderer.CompileShader(program.Stage, program.Code);

                    shaders[stage].HostShader = hostShader;

                    hostShaders.Add(hostShader);
                }

                IProgram hostProgram = _context.Renderer.CreateProgram(hostShaders.ToArray(), tfd);

                hostProgram.CheckProgramLink(true);

                byte[] hostProgramBinary = HostShaderCacheEntry.Create(hostProgram.GetBinary(), shaders);

                gpShaders = new ShaderBundle(hostProgram, shaders);

                if (isShaderCacheEnabled)
                {
                    _gpProgramsDiskCache.Add(programCodeHash, gpShaders);

                    if (!isShaderCacheReadOnly)
                    {
                        _cacheManager.SaveProgram(ref programCodeHash, CacheHelper.CreateGuestProgramDump(shaderCacheEntries, tfd), hostProgramBinary);
                    }
                }
            }

            if (!isCached)
            {
                list = new List<ShaderBundle>();

                _gpPrograms.Add(addresses, list);
            }

            list.Add(gpShaders);

            return gpShaders;
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

                int length = (int)Math.Min((uint)tf.VaryingsCount, 0x80);

                var varyingLocations = MemoryMarshal.Cast<uint, byte>(state.TfVaryingLocations[i].ToSpan()).Slice(0, length);

                descs[i] = new TransformFeedbackDescriptor(tf.BufferIndex, tf.Stride, varyingLocations.ToArray());
            }

            return descs;
        }

        /// <summary>
        /// Checks if compute shader code in memory is equal to the cached shader.
        /// </summary>
        /// <param name="memoryManager">Memory manager used to access the GPU memory where the shader is located</param>
        /// <param name="cpShader">Cached compute shader</param>
        /// <param name="gpuVa">GPU virtual address of the shader code in memory</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private static bool IsShaderEqual(MemoryManager memoryManager, ShaderBundle cpShader, ulong gpuVa)
        {
            return IsShaderEqual(memoryManager, cpShader.Shaders[0], gpuVa);
        }

        /// <summary>
        /// Checks if graphics shader code from all stages in memory are equal to the cached shaders.
        /// </summary>
        /// <param name="memoryManager">Memory manager used to access the GPU memory where the shader is located</param>
        /// <param name="gpShaders">Cached graphics shaders</param>
        /// <param name="addresses">GPU virtual addresses of all enabled shader stages</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private static bool IsShaderEqual(MemoryManager memoryManager, ShaderBundle gpShaders, ShaderAddresses addresses)
        {
            for (int stage = 0; stage < gpShaders.Shaders.Length; stage++)
            {
                ShaderCodeHolder shader = gpShaders.Shaders[stage];

                ulong gpuVa = 0;

                switch (stage)
                {
                    case 0: gpuVa = addresses.Vertex;         break;
                    case 1: gpuVa = addresses.TessControl;    break;
                    case 2: gpuVa = addresses.TessEvaluation; break;
                    case 3: gpuVa = addresses.Geometry;       break;
                    case 4: gpuVa = addresses.Fragment;       break;
                }

                if (!IsShaderEqual(memoryManager, shader, gpuVa, addresses.VertexA))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the code of the specified cached shader is different from the code in memory.
        /// </summary>
        /// <param name="memoryManager">Memory manager used to access the GPU memory where the shader is located</param>
        /// <param name="shader">Cached shader to compare with</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="gpuVaA">Optional GPU virtual address of the "Vertex A" binary shader code</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private static bool IsShaderEqual(MemoryManager memoryManager, ShaderCodeHolder shader, ulong gpuVa, ulong gpuVaA = 0)
        {
            if (shader == null)
            {
                return true;
            }

            ReadOnlySpan<byte> memoryCode = memoryManager.GetSpan(gpuVa, shader.Code.Length);

            bool equals = memoryCode.SequenceEqual(shader.Code);

            if (equals && shader.Code2 != null)
            {
                memoryCode = memoryManager.GetSpan(gpuVaA, shader.Code2.Length);

                equals = memoryCode.SequenceEqual(shader.Code2);
            }

            return equals;
        }

        /// <summary>
        /// Decode the binary Maxwell shader code to a translator context.
        /// </summary>
        /// <param name="channel">GPU channel</param>
        /// <param name="gas">GPU accessor state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>The generated translator context</returns>
        private TranslatorContext DecodeComputeShader(
            GpuChannel channel,
            GpuAccessorState gas,
            ulong gpuVa,
            int localSizeX,
            int localSizeY,
            int localSizeZ,
            int localMemorySize,
            int sharedMemorySize)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            GpuAccessor gpuAccessor = new GpuAccessor(_context, channel, gas, localSizeX, localSizeY, localSizeZ, localMemorySize, sharedMemorySize);

            var options = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, DefaultFlags | TranslationFlags.Compute);
            return Translator.CreateContext(gpuVa, gpuAccessor, options);
        }

        /// <summary>
        /// Decode the binary Maxwell shader code to a translator context.
        /// </summary>
        /// <remarks>
        /// This will combine the "Vertex A" and "Vertex B" shader stages, if specified, into one shader.
        /// </remarks>
        /// <param name="channel">GPU channel</param>
        /// <param name="gas">GPU accessor state</param>
        /// <param name="counts">Cumulative shader resource counts</param>
        /// <param name="flags">Flags that controls shader translation</param>
        /// <param name="stage">Shader stage</param>
        /// <param name="gpuVa">GPU virtual address of the shader code</param>
        /// <returns>The generated translator context</returns>
        private TranslatorContext DecodeGraphicsShader(
            GpuChannel channel,
            GpuAccessorState gas,
            TranslationCounts counts,
            TranslationFlags flags,
            ShaderStage stage,
            ulong gpuVa)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            GpuAccessor gpuAccessor = new GpuAccessor(_context, channel, gas, (int)stage - 1);

            var options = new TranslationOptions(TargetLanguage.Glsl, TargetApi.OpenGL, flags);
            return Translator.CreateContext(gpuVa, gpuAccessor, options, counts);
        }

        /// <summary>
        /// Translates a previously generated translator context to something that the host API accepts.
        /// </summary>
        /// <param name="dumper">Optional shader code dumper</param>
        /// <param name="memoryManager">Memory manager used to access the GPU memory where the shader is located</param>
        /// <param name="stages">Translator context of all available shader stages</param>
        /// <param name="stageIndex">Index on the stages array to translate</param>
        /// <returns>Compiled graphics shader code</returns>
        private static ShaderCodeHolder TranslateShader(
            ShaderDumper dumper,
            MemoryManager memoryManager,
            TranslatorContext[] stages,
            int stageIndex)
        {
            TranslatorContext currentStage = stages[stageIndex];
            TranslatorContext nextStage = GetNextStageContext(stages, stageIndex);
            TranslatorContext vertexA = stageIndex == 1 ? stages[0] : null;

            return TranslateShader(dumper, memoryManager, currentStage, nextStage, vertexA);
        }

        /// <summary>
        /// Gets the next shader stage context, from an array of contexts and index of the current stage.
        /// </summary>
        /// <param name="stages">Translator context of all available shader stages</param>
        /// <param name="stageIndex">Index on the stages array to translate</param>
        /// <returns>The translator context of the next stage, or null if inexistent</returns>
        private static TranslatorContext GetNextStageContext(TranslatorContext[] stages, int stageIndex)
        {
            for (int nextStageIndex = stageIndex + 1; nextStageIndex < stages.Length; nextStageIndex++)
            {
                if (stages[nextStageIndex] != null)
                {
                    return stages[nextStageIndex];
                }
            }

            return null;
        }

        /// <summary>
        /// Translates a previously generated translator context to something that the host API accepts.
        /// </summary>
        /// <param name="dumper">Optional shader code dumper</param>
        /// <param name="memoryManager">Memory manager used to access the GPU memory where the shader is located</param>
        /// <param name="currentStage">Translator context of the stage to be translated</param>
        /// <param name="nextStage">Translator context of the next active stage, if existent</param>
        /// <param name="vertexA">Optional translator context of the shader that should be combined</param>
        /// <returns>Compiled graphics shader code</returns>
        private static ShaderCodeHolder TranslateShader(
            ShaderDumper dumper,
            MemoryManager memoryManager,
            TranslatorContext currentStage,
            TranslatorContext nextStage,
            TranslatorContext vertexA)
        {
            if (currentStage == null)
            {
                return null;
            }

            if (vertexA != null)
            {
                byte[] codeA = memoryManager.GetSpan(vertexA.Address, vertexA.Size).ToArray();
                byte[] codeB = memoryManager.GetSpan(currentStage.Address, currentStage.Size).ToArray();

                ShaderDumpPaths pathsA = default;
                ShaderDumpPaths pathsB = default;

                if (dumper != null)
                {
                    pathsA = dumper.Dump(codeA, compute: false);
                    pathsB = dumper.Dump(codeB, compute: false);
                }

                ShaderProgram program = currentStage.Translate(out ShaderProgramInfo shaderProgramInfo, nextStage, vertexA);

                pathsB.Prepend(program);
                pathsA.Prepend(program);

                return new ShaderCodeHolder(program, shaderProgramInfo, codeB, codeA);
            }
            else
            {
                byte[] code = memoryManager.GetSpan(currentStage.Address, currentStage.Size).ToArray();

                ShaderDumpPaths paths = dumper?.Dump(code, currentStage.Stage == ShaderStage.Compute) ?? default;

                ShaderProgram program = currentStage.Translate(out ShaderProgramInfo shaderProgramInfo, nextStage);

                paths.Prepend(program);

                return new ShaderCodeHolder(program, shaderProgramInfo, code);
            }
        }

        /// <summary>
        /// Disposes the shader cache, deleting all the cached shaders.
        /// It's an error to use the shader cache after disposal.
        /// </summary>
        public void Dispose()
        {
            foreach (List<ShaderBundle> list in _cpPrograms.Values)
            {
                foreach (ShaderBundle bundle in list)
                {
                    bundle.Dispose();
                }
            }

            foreach (List<ShaderBundle> list in _gpPrograms.Values)
            {
                foreach (ShaderBundle bundle in list)
                {
                    bundle.Dispose();
                }
            }

            _cacheManager?.Dispose();
        }
    }
}
