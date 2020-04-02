using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader
{
    using TextureDescriptor = Image.TextureDescriptor;

    /// <summary>
    /// Memory cache of shader code.
    /// </summary>
    class ShaderCache : IDisposable
    {
        private const int MaxProgramSize = 0x100000;

        private const TranslationFlags DefaultFlags = TranslationFlags.DebugMode;

        private GpuContext _context;

        private ShaderDumper _dumper;

        private Dictionary<ulong, List<ComputeShader>> _cpPrograms;

        private Dictionary<ShaderAddresses, List<GraphicsShader>> _gpPrograms;

        /// <summary>
        /// Creates a new instance of the shader cache.
        /// </summary>
        /// <param name="context">GPU context that the shader cache belongs to</param>
        public ShaderCache(GpuContext context)
        {
            _context = context;

            _dumper = new ShaderDumper();

            _cpPrograms = new Dictionary<ulong, List<ComputeShader>>();

            _gpPrograms = new Dictionary<ShaderAddresses, List<GraphicsShader>>();
        }

        /// <summary>
        /// Gets a compute shader from the cache.
        /// </summary>
        /// <remarks>
        /// This automatically translates, compiles and adds the code to the cache if not present.
        /// </remarks>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        public ComputeShader GetComputeShader(
            ulong gpuVa,
            int localSizeX,
            int localSizeY,
            int localSizeZ,
            int localMemorySize,
            int sharedMemorySize)
        {
            bool isCached = _cpPrograms.TryGetValue(gpuVa, out List<ComputeShader> list);

            if (isCached)
            {
                foreach (ComputeShader cachedCpShader in list)
                {
                    if (!IsShaderDifferent(cachedCpShader, gpuVa))
                    {
                        return cachedCpShader;
                    }
                }
            }

            CachedShader shader = TranslateComputeShader(
                gpuVa,
                localSizeX,
                localSizeY,
                localSizeZ,
                localMemorySize,
                sharedMemorySize);

            shader.HostShader = _context.Renderer.CompileShader(shader.Program);

            IProgram hostProgram = _context.Renderer.CreateProgram(new IShader[] { shader.HostShader });

            ComputeShader cpShader = new ComputeShader(hostProgram, shader);

            if (!isCached)
            {
                list = new List<ComputeShader>();

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
        /// <param name="state">Current GPU state</param>
        /// <param name="addresses">Addresses of the shaders for each stage</param>
        /// <returns>Compiled graphics shader code</returns>
        public GraphicsShader GetGraphicsShader(GpuState state, ShaderAddresses addresses)
        {
            bool isCached = _gpPrograms.TryGetValue(addresses, out List<GraphicsShader> list);

            if (isCached)
            {
                foreach (GraphicsShader cachedGpShaders in list)
                {
                    if (!IsShaderDifferent(cachedGpShaders, addresses))
                    {
                        return cachedGpShaders;
                    }
                }
            }

            GraphicsShader gpShaders = new GraphicsShader();

            if (addresses.VertexA != 0)
            {
                gpShaders.Shaders[0] = TranslateGraphicsShader(state, ShaderStage.Vertex, addresses.Vertex, addresses.VertexA);
            }
            else
            {
                gpShaders.Shaders[0] = TranslateGraphicsShader(state, ShaderStage.Vertex, addresses.Vertex);
            }

            gpShaders.Shaders[1] = TranslateGraphicsShader(state, ShaderStage.TessellationControl,    addresses.TessControl);
            gpShaders.Shaders[2] = TranslateGraphicsShader(state, ShaderStage.TessellationEvaluation, addresses.TessEvaluation);
            gpShaders.Shaders[3] = TranslateGraphicsShader(state, ShaderStage.Geometry,               addresses.Geometry);
            gpShaders.Shaders[4] = TranslateGraphicsShader(state, ShaderStage.Fragment,               addresses.Fragment);

            List<IShader> hostShaders = new List<IShader>();

            for (int stage = 0; stage < gpShaders.Shaders.Length; stage++)
            {
                ShaderProgram program = gpShaders.Shaders[stage]?.Program;

                if (program == null)
                {
                    continue;
                }

                IShader hostShader = _context.Renderer.CompileShader(program);

                gpShaders.Shaders[stage].HostShader = hostShader;

                hostShaders.Add(hostShader);
            }

            gpShaders.HostProgram = _context.Renderer.CreateProgram(hostShaders.ToArray());

            if (!isCached)
            {
                list = new List<GraphicsShader>();

                _gpPrograms.Add(addresses, list);
            }

            list.Add(gpShaders);

            return gpShaders;
        }

        /// <summary>
        /// Checks if compute shader code in memory is different from the cached shader.
        /// </summary>
        /// <param name="cpShader">Cached compute shader</param>
        /// <param name="gpuVa">GPU virtual address of the shader code in memory</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private bool IsShaderDifferent(ComputeShader cpShader, ulong gpuVa)
        {
            return IsShaderDifferent(cpShader.Shader, gpuVa);
        }

        /// <summary>
        /// Checks if graphics shader code from all stages in memory is different from the cached shaders.
        /// </summary>
        /// <param name="gpShaders">Cached graphics shaders</param>
        /// <param name="addresses">GPU virtual addresses of all enabled shader stages</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private bool IsShaderDifferent(GraphicsShader gpShaders, ShaderAddresses addresses)
        {
            for (int stage = 0; stage < gpShaders.Shaders.Length; stage++)
            {
                CachedShader shader = gpShaders.Shaders[stage];

                ulong gpuVa = 0;

                switch (stage)
                {
                    case 0: gpuVa = addresses.Vertex;         break;
                    case 1: gpuVa = addresses.TessControl;    break;
                    case 2: gpuVa = addresses.TessEvaluation; break;
                    case 3: gpuVa = addresses.Geometry;       break;
                    case 4: gpuVa = addresses.Fragment;       break;
                }

                if (IsShaderDifferent(shader, gpuVa))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the code of the specified cached shader is different from the code in memory.
        /// </summary>
        /// <param name="shader">Cached shader to compare with</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <returns>True if the code is different, false otherwise</returns>
        private bool IsShaderDifferent(CachedShader shader, ulong gpuVa)
        {
            if (shader == null)
            {
                return false;
            }

            ReadOnlySpan<byte> memoryCode = _context.MemoryAccessor.GetSpan(gpuVa, (ulong)shader.Code.Length * 4);

            return !MemoryMarshal.Cast<byte, int>(memoryCode).SequenceEqual(shader.Code);
        }

        /// <summary>
        /// Translates the binary Maxwell shader code to something that the host API accepts.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        private CachedShader TranslateComputeShader(
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

            int QueryInfo(QueryInfoName info, int index)
            {
                return info switch
                {
                    QueryInfoName.ComputeLocalSizeX => localSizeX,
                    QueryInfoName.ComputeLocalSizeY => localSizeY,
                    QueryInfoName.ComputeLocalSizeZ => localSizeZ,
                    QueryInfoName.ComputeLocalMemorySize => localMemorySize,
                    QueryInfoName.ComputeSharedMemorySize => sharedMemorySize,
                    _ => QueryInfoCommon(info)
                };
            }

            TranslatorCallbacks callbacks = new TranslatorCallbacks(QueryInfo, PrintLog);

            ShaderProgram program;

            ReadOnlySpan<byte> code = _context.MemoryAccessor.GetSpan(gpuVa, MaxProgramSize);

            program = Translator.Translate(code, callbacks, DefaultFlags | TranslationFlags.Compute);

            int[] codeCached = MemoryMarshal.Cast<byte, int>(code.Slice(0, program.Size)).ToArray();

            _dumper.Dump(code, compute: true, out string fullPath, out string codePath);

            if (fullPath != null && codePath != null)
            {
                program.Prepend("// " + codePath);
                program.Prepend("// " + fullPath);
            }

            return new CachedShader(program, codeCached);
        }

        /// <summary>
        /// Translates the binary Maxwell shader code to something that the host API accepts.
        /// </summary>
        /// <remarks>
        /// This will combine the "Vertex A" and "Vertex B" shader stages, if specified, into one shader.
        /// </remarks>
        /// <param name="state">Current GPU state</param>
        /// <param name="stage">Shader stage</param>
        /// <param name="gpuVa">GPU virtual address of the shader code</param>
        /// <param name="gpuVaA">Optional GPU virtual address of the "Vertex A" shader code</param>
        /// <returns>Compiled graphics shader code</returns>
        private CachedShader TranslateGraphicsShader(GpuState state, ShaderStage stage, ulong gpuVa, ulong gpuVaA = 0)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            int QueryInfo(QueryInfoName info, int index)
            {
                return info switch
                {
                    QueryInfoName.IsTextureBuffer => Convert.ToInt32(QueryIsTextureBuffer(state, (int)stage - 1, index)),
                    QueryInfoName.IsTextureRectangle => Convert.ToInt32(QueryIsTextureRectangle(state, (int)stage - 1, index)),
                    QueryInfoName.PrimitiveTopology => (int)GetPrimitiveTopology(),
                    _ => QueryInfoCommon(info)
                };
            }

            TranslatorCallbacks callbacks = new TranslatorCallbacks(QueryInfo, PrintLog);

            ShaderProgram program;

            int[] codeCached = null;

            if (gpuVaA != 0)
            {
                ReadOnlySpan<byte> codeA = _context.MemoryAccessor.GetSpan(gpuVaA, MaxProgramSize);
                ReadOnlySpan<byte> codeB = _context.MemoryAccessor.GetSpan(gpuVa,  MaxProgramSize);

                program = Translator.Translate(codeA, codeB, callbacks, DefaultFlags);

                // TODO: We should also take "codeA" into account.
                codeCached = MemoryMarshal.Cast<byte, int>(codeB.Slice(0, program.Size)).ToArray();

                _dumper.Dump(codeA, compute: false, out string fullPathA, out string codePathA);
                _dumper.Dump(codeB, compute: false, out string fullPathB, out string codePathB);

                if (fullPathA != null && fullPathB != null && codePathA != null && codePathB != null)
                {
                    program.Prepend("// " + codePathB);
                    program.Prepend("// " + fullPathB);
                    program.Prepend("// " + codePathA);
                    program.Prepend("// " + fullPathA);
                }
            }
            else
            {
                ReadOnlySpan<byte> code = _context.MemoryAccessor.GetSpan(gpuVa, MaxProgramSize);

                program = Translator.Translate(code, callbacks, DefaultFlags);

                codeCached = MemoryMarshal.Cast<byte, int>(code.Slice(0, program.Size)).ToArray();

                _dumper.Dump(code, compute: false, out string fullPath, out string codePath);

                if (fullPath != null && codePath != null)
                {
                    program.Prepend("// " + codePath);
                    program.Prepend("// " + fullPath);
                }
            }

            ulong address = _context.MemoryManager.Translate(gpuVa);

            return new CachedShader(program, codeCached);
        }

        /// <summary>
        /// Gets the primitive topology for the current draw.
        /// This is required by geometry shaders.
        /// </summary>
        /// <returns>Primitive topology</returns>
        private InputTopology GetPrimitiveTopology()
        {
            switch (_context.Methods.PrimitiveType)
            {
                case PrimitiveType.Points:
                    return InputTopology.Points;
                case PrimitiveType.Lines:
                case PrimitiveType.LineLoop:
                case PrimitiveType.LineStrip:
                    return InputTopology.Lines;
                case PrimitiveType.LinesAdjacency:
                case PrimitiveType.LineStripAdjacency:
                    return InputTopology.LinesAdjacency;
                case PrimitiveType.Triangles:
                case PrimitiveType.TriangleStrip:
                case PrimitiveType.TriangleFan:
                    return InputTopology.Triangles;
                case PrimitiveType.TrianglesAdjacency:
                case PrimitiveType.TriangleStripAdjacency:
                    return InputTopology.TrianglesAdjacency;
            }

            return InputTopology.Points;
        }

        /// <summary>
        /// Check if the target of a given texture is texture buffer.
        /// This is required as 1D textures and buffer textures shares the same sampler type on binary shader code,
        /// but not on GLSL.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage</param>
        /// <param name="index">Index of the texture (this is the shader "fake" handle)</param>
        /// <returns>True if the texture is a buffer texture, false otherwise</returns>
        private bool QueryIsTextureBuffer(GpuState state, int stageIndex, int index)
        {
            return GetTextureDescriptor(state, stageIndex, index).UnpackTextureTarget() == TextureTarget.TextureBuffer;
        }

        /// <summary>
        /// Check if the target of a given texture is texture rectangle.
        /// This is required as 2D textures and rectangle textures shares the same sampler type on binary shader code,
        /// but not on GLSL.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage</param>
        /// <param name="index">Index of the texture (this is the shader "fake" handle)</param>
        /// <returns>True if the texture is a rectangle texture, false otherwise</returns>
        private bool QueryIsTextureRectangle(GpuState state, int stageIndex, int index)
        {
            var descriptor = GetTextureDescriptor(state, stageIndex, index);

            TextureTarget target = descriptor.UnpackTextureTarget();

            bool is2DTexture = target == TextureTarget.Texture2D ||
                               target == TextureTarget.Texture2DRect;

            return !descriptor.UnpackTextureCoordNormalized() && is2DTexture;
        }

        /// <summary>
        /// Gets the texture descriptor for a given texture on the pool.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage</param>
        /// <param name="index">Index of the texture (this is the shader "fake" handle)</param>
        /// <returns>Texture descriptor</returns>
        private TextureDescriptor GetTextureDescriptor(GpuState state, int stageIndex, int index)
        {
            return _context.Methods.TextureManager.GetGraphicsTextureDescriptor(state, stageIndex, index);
        }

        /// <summary>
        /// Returns information required by both compute and graphics shader compilation.
        /// </summary>
        /// <param name="info">Information queried</param>
        /// <returns>Requested information</returns>
        private int QueryInfoCommon(QueryInfoName info)
        {
            return info switch
            {
                QueryInfoName.StorageBufferOffsetAlignment => _context.Capabilities.StorageBufferOffsetAlignment,
                QueryInfoName.SupportsNonConstantTextureOffset => Convert.ToInt32(_context.Capabilities.SupportsNonConstantTextureOffset),
                _ => 0
            };
        }

        /// <summary>
        /// Prints a warning from the shader code translator.
        /// </summary>
        /// <param name="message">Warning message</param>
        private static void PrintLog(string message)
        {
            Logger.PrintWarning(LogClass.Gpu, $"Shader translator: {message}");
        }

        /// <summary>
        /// Disposes the shader cache, deleting all the cached shaders.
        /// It's an error to use the shader cache after disposal.
        /// </summary>
        public void Dispose()
        {
            foreach (List<ComputeShader> list in _cpPrograms.Values)
            {
                foreach (ComputeShader shader in list)
                {
                    shader.HostProgram.Dispose();
                    shader.Shader?.HostShader.Dispose();
                }
            }

            foreach (List<GraphicsShader> list in _gpPrograms.Values)
            {
                foreach (GraphicsShader shader in list)
                {
                    shader.HostProgram.Dispose();

                    foreach (CachedShader cachedShader in shader.Shaders)
                    {
                        cachedShader?.HostShader.Dispose();
                    }
                }
            }
        }
    }
}