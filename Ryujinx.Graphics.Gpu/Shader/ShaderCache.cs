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
        /// <param name="state">Current GPU state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        public ComputeShader GetComputeShader(
            GpuState state,
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
                state,
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
        /// <param name="state">Current GPU state</param>
        /// <param name="gpuVa">GPU virtual address of the binary shader code</param>
        /// <param name="localSizeX">Local group size X of the computer shader</param>
        /// <param name="localSizeY">Local group size Y of the computer shader</param>
        /// <param name="localSizeZ">Local group size Z of the computer shader</param>
        /// <param name="localMemorySize">Local memory size of the compute shader</param>
        /// <param name="sharedMemorySize">Shared memory size of the compute shader</param>
        /// <returns>Compiled compute shader code</returns>
        private CachedShader TranslateComputeShader(
            GpuState state,
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
                    QueryInfoName.ComputeLocalSizeX
                        => localSizeX,
                    QueryInfoName.ComputeLocalSizeY
                        => localSizeY,
                    QueryInfoName.ComputeLocalSizeZ
                        => localSizeZ,
                    QueryInfoName.ComputeLocalMemorySize
                        => localMemorySize,
                    QueryInfoName.ComputeSharedMemorySize
                        => sharedMemorySize,
                    QueryInfoName.IsTextureBuffer
                        => Convert.ToInt32(QueryIsTextureBuffer(state, 0, index, compute: true)),
                    QueryInfoName.IsTextureRectangle
                        => Convert.ToInt32(QueryIsTextureRectangle(state, 0, index, compute: true)),
                    QueryInfoName.TextureFormat
                        => (int)QueryTextureFormat(state, 0, index, compute: true),
                    _
                        => QueryInfoCommon(info)
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
                    QueryInfoName.IsTextureBuffer
                        => Convert.ToInt32(QueryIsTextureBuffer(state, (int)stage - 1, index, compute: false)),
                    QueryInfoName.IsTextureRectangle
                        => Convert.ToInt32(QueryIsTextureRectangle(state, (int)stage - 1, index, compute: false)),
                    QueryInfoName.PrimitiveTopology
                        => (int)QueryPrimitiveTopology(),
                    QueryInfoName.TextureFormat
                        => (int)QueryTextureFormat(state, (int)stage - 1, index, compute: false),
                    _
                        => QueryInfoCommon(info)
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
        private InputTopology QueryPrimitiveTopology()
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
        /// <param name="handle">Index of the texture (this is the shader "fake" handle)</param>
        /// <param name="compute">Indicates whenever the texture descriptor is for the compute or graphics engine</param>
        /// <returns>True if the texture is a buffer texture, false otherwise</returns>
        private bool QueryIsTextureBuffer(GpuState state, int stageIndex, int handle, bool compute)
        {
            return GetTextureDescriptor(state, stageIndex, handle, compute).UnpackTextureTarget() == TextureTarget.TextureBuffer;
        }

        /// <summary>
        /// Check if the target of a given texture is texture rectangle.
        /// This is required as 2D textures and rectangle textures shares the same sampler type on binary shader code,
        /// but not on GLSL.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage</param>
        /// <param name="handle">Index of the texture (this is the shader "fake" handle)</param>
        /// <param name="compute">Indicates whenever the texture descriptor is for the compute or graphics engine</param>
        /// <returns>True if the texture is a rectangle texture, false otherwise</returns>
        private bool QueryIsTextureRectangle(GpuState state, int stageIndex, int handle, bool compute)
        {
            var descriptor = GetTextureDescriptor(state, stageIndex, handle, compute);

            TextureTarget target = descriptor.UnpackTextureTarget();

            bool is2DTexture = target == TextureTarget.Texture2D ||
                               target == TextureTarget.Texture2DRect;

            return !descriptor.UnpackTextureCoordNormalized() && is2DTexture;
        }

        /// <summary>
        /// Queries the format of a given texture.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage. This is ignored if <paramref name="compute"/> is true</param>
        /// <param name="handle">Index of the texture (this is the shader "fake" handle)</param>
        /// <param name="compute">Indicates whenever the texture descriptor is for the compute or graphics engine</param>
        /// <returns>The texture format</returns>
        private TextureFormat QueryTextureFormat(GpuState state, int stageIndex, int handle, bool compute)
        {
            return QueryTextureFormat(GetTextureDescriptor(state, stageIndex, handle, compute));
        }

        /// <summary>
        /// Queries the format of a given texture.
        /// </summary>
        /// <param name="descriptor">Descriptor of the texture from the texture pool</param>
        /// <returns>The texture format</returns>
        private static TextureFormat QueryTextureFormat(TextureDescriptor descriptor)
        {
            if (!FormatTable.TryGetTextureFormat(descriptor.UnpackFormat(), descriptor.UnpackSrgb(), out FormatInfo formatInfo))
            {
                return TextureFormat.Unknown;
            }

            return formatInfo.Format switch
            {
                Format.R8Unorm           => TextureFormat.R8Unorm,
                Format.R8Snorm           => TextureFormat.R8Snorm,
                Format.R8Uint            => TextureFormat.R8Uint,
                Format.R8Sint            => TextureFormat.R8Sint,
                Format.R16Float          => TextureFormat.R16Float,
                Format.R16Unorm          => TextureFormat.R16Unorm,
                Format.R16Snorm          => TextureFormat.R16Snorm,
                Format.R16Uint           => TextureFormat.R16Uint,
                Format.R16Sint           => TextureFormat.R16Sint,
                Format.R32Float          => TextureFormat.R32Float,
                Format.R32Uint           => TextureFormat.R32Uint,
                Format.R32Sint           => TextureFormat.R32Sint,
                Format.R8G8Unorm         => TextureFormat.R8G8Unorm,
                Format.R8G8Snorm         => TextureFormat.R8G8Snorm,
                Format.R8G8Uint          => TextureFormat.R8G8Uint,
                Format.R8G8Sint          => TextureFormat.R8G8Sint,
                Format.R16G16Float       => TextureFormat.R16G16Float,
                Format.R16G16Unorm       => TextureFormat.R16G16Unorm,
                Format.R16G16Snorm       => TextureFormat.R16G16Snorm,
                Format.R16G16Uint        => TextureFormat.R16G16Uint,
                Format.R16G16Sint        => TextureFormat.R16G16Sint,
                Format.R32G32Float       => TextureFormat.R32G32Float,
                Format.R32G32Uint        => TextureFormat.R32G32Uint,
                Format.R32G32Sint        => TextureFormat.R32G32Sint,
                Format.R8G8B8A8Unorm     => TextureFormat.R8G8B8A8Unorm,
                Format.R8G8B8A8Snorm     => TextureFormat.R8G8B8A8Snorm,
                Format.R8G8B8A8Uint      => TextureFormat.R8G8B8A8Uint,
                Format.R8G8B8A8Sint      => TextureFormat.R8G8B8A8Sint,
                Format.R16G16B16A16Float => TextureFormat.R16G16B16A16Float,
                Format.R16G16B16A16Unorm => TextureFormat.R16G16B16A16Unorm,
                Format.R16G16B16A16Snorm => TextureFormat.R16G16B16A16Snorm,
                Format.R16G16B16A16Uint  => TextureFormat.R16G16B16A16Uint,
                Format.R16G16B16A16Sint  => TextureFormat.R16G16B16A16Sint,
                Format.R32G32B32A32Float => TextureFormat.R32G32B32A32Float,
                Format.R32G32B32A32Uint  => TextureFormat.R32G32B32A32Uint,
                Format.R32G32B32A32Sint  => TextureFormat.R32G32B32A32Sint,
                Format.R10G10B10A2Unorm  => TextureFormat.R10G10B10A2Unorm,
                Format.R10G10B10A2Uint   => TextureFormat.R10G10B10A2Uint,
                Format.R11G11B10Float    => TextureFormat.R11G11B10Float,
                _                        => TextureFormat.Unknown
            };
        }

        /// <summary>
        /// Gets the texture descriptor for a given texture on the pool.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Index of the shader stage. This is ignored if <paramref name="compute"/> is true</param>
        /// <param name="handle">Index of the texture (this is the shader "fake" handle)</param>
        /// <param name="compute">Indicates whenever the texture descriptor is for the compute or graphics engine</param>
        /// <returns>Texture descriptor</returns>
        private TextureDescriptor GetTextureDescriptor(GpuState state, int stageIndex, int handle, bool compute)
        {
            if (compute)
            {
                return _context.Methods.TextureManager.GetComputeTextureDescriptor(state, handle);
            }
            else
            {
                return _context.Methods.TextureManager.GetGraphicsTextureDescriptor(state, stageIndex, handle);
            }
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
                QueryInfoName.StorageBufferOffsetAlignment
                    => _context.Capabilities.StorageBufferOffsetAlignment,
                QueryInfoName.SupportsNonConstantTextureOffset
                    => Convert.ToInt32(_context.Capabilities.SupportsNonConstantTextureOffset),
                _
                    => 0
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