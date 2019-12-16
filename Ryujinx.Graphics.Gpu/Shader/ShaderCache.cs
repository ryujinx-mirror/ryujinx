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
    class ShaderCache
    {
        private const int MaxProgramSize = 0x100000;

        private const TranslationFlags DefaultFlags = TranslationFlags.DebugMode;

        private GpuContext _context;

        private ShaderDumper _dumper;

        private Dictionary<ulong, List<ComputeShader>> _cpPrograms;

        private Dictionary<ShaderAddresses, List<GraphicsShader>> _gpPrograms;

        public ShaderCache(GpuContext context)
        {
            _context = context;

            _dumper = new ShaderDumper(context);

            _cpPrograms = new Dictionary<ulong, List<ComputeShader>>();

            _gpPrograms = new Dictionary<ShaderAddresses, List<GraphicsShader>>();
        }

        public ComputeShader GetComputeShader(ulong gpuVa, int sharedMemorySize, int localSizeX, int localSizeY, int localSizeZ)
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

            CachedShader shader = TranslateComputeShader(gpuVa, sharedMemorySize, localSizeX, localSizeY, localSizeZ);

            IShader hostShader = _context.Renderer.CompileShader(shader.Program);

            IProgram hostProgram = _context.Renderer.CreateProgram(new IShader[] { hostShader });

            ulong address = _context.MemoryManager.Translate(gpuVa);

            ComputeShader cpShader = new ComputeShader(hostProgram, shader);

            if (!isCached)
            {
                list = new List<ComputeShader>();

                _cpPrograms.Add(gpuVa, list);
            }

            list.Add(cpShader);

            return cpShader;
        }

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
                gpShaders.Shader[0] = TranslateGraphicsShader(state, ShaderStage.Vertex, addresses.Vertex, addresses.VertexA);
            }
            else
            {
                gpShaders.Shader[0] = TranslateGraphicsShader(state, ShaderStage.Vertex, addresses.Vertex);
            }

            gpShaders.Shader[1] = TranslateGraphicsShader(state, ShaderStage.TessellationControl,    addresses.TessControl);
            gpShaders.Shader[2] = TranslateGraphicsShader(state, ShaderStage.TessellationEvaluation, addresses.TessEvaluation);
            gpShaders.Shader[3] = TranslateGraphicsShader(state, ShaderStage.Geometry,               addresses.Geometry);
            gpShaders.Shader[4] = TranslateGraphicsShader(state, ShaderStage.Fragment,               addresses.Fragment);

            BackpropQualifiers(gpShaders);

            List<IShader> hostShaders = new List<IShader>();

            for (int stage = 0; stage < gpShaders.Shader.Length; stage++)
            {
                ShaderProgram program = gpShaders.Shader[stage].Program;

                if (program == null)
                {
                    continue;
                }

                IShader hostShader = _context.Renderer.CompileShader(program);

                gpShaders.Shader[stage].Shader = hostShader;

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

        private bool IsShaderDifferent(ComputeShader cpShader, ulong gpuVa)
        {
            return IsShaderDifferent(cpShader.Shader, gpuVa);
        }

        private bool IsShaderDifferent(GraphicsShader gpShaders, ShaderAddresses addresses)
        {
            for (int stage = 0; stage < gpShaders.Shader.Length; stage++)
            {
                CachedShader shader = gpShaders.Shader[stage];

                if (shader.Code == null)
                {
                    continue;
                }

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

        private bool IsShaderDifferent(CachedShader shader, ulong gpuVa)
        {
            for (int index = 0; index < shader.Code.Length; index++)
            {
                if (_context.MemoryAccessor.ReadInt32(gpuVa + (ulong)index * 4) != shader.Code[index])
                {
                    return true;
                }
            }

            return false;
        }

        private CachedShader TranslateComputeShader(ulong gpuVa, int sharedMemorySize, int localSizeX, int localSizeY, int localSizeZ)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            QueryInfoCallback queryInfo = (QueryInfoName info, int index) =>
            {
                switch (info)
                {
                    case QueryInfoName.ComputeLocalSizeX:
                        return localSizeX;
                    case QueryInfoName.ComputeLocalSizeY:
                        return localSizeY;
                    case QueryInfoName.ComputeLocalSizeZ:
                        return localSizeZ;
                    case QueryInfoName.ComputeSharedMemorySize:
                        return sharedMemorySize;
                }

                return QueryInfoCommon(info);
            };

            ShaderProgram program;

            Span<byte> code = _context.MemoryAccessor.Read(gpuVa, MaxProgramSize);

            program = Translator.Translate(code, queryInfo, DefaultFlags | TranslationFlags.Compute);

            int[] codeCached = MemoryMarshal.Cast<byte, int>(code.Slice(0, program.Size)).ToArray();

            _dumper.Dump(code, compute: true, out string fullPath, out string codePath);

            if (fullPath != null && codePath != null)
            {
                program.Prepend("// " + codePath);
                program.Prepend("// " + fullPath);
            }

            return new CachedShader(program, codeCached);
        }

        private CachedShader TranslateGraphicsShader(GpuState state, ShaderStage stage, ulong gpuVa, ulong gpuVaA = 0)
        {
            if (gpuVa == 0)
            {
                return new CachedShader(null, null);
            }

            QueryInfoCallback queryInfo = (QueryInfoName info, int index) =>
            {
                switch (info)
                {
                    case QueryInfoName.IsTextureBuffer:
                        return Convert.ToInt32(QueryIsTextureBuffer(state, (int)stage - 1, index));
                    case QueryInfoName.IsTextureRectangle:
                        return Convert.ToInt32(QueryIsTextureRectangle(state, (int)stage - 1, index));
                    case QueryInfoName.PrimitiveTopology:
                        return (int)GetPrimitiveTopology();
                    case QueryInfoName.ViewportTransformEnable:
                        return Convert.ToInt32(_context.Methods.GetViewportTransformEnable(state));
                }

                return QueryInfoCommon(info);
            };

            ShaderProgram program;

            int[] codeCached = null;

            if (gpuVaA != 0)
            {
                Span<byte> codeA = _context.MemoryAccessor.Read(gpuVaA, MaxProgramSize);
                Span<byte> codeB = _context.MemoryAccessor.Read(gpuVa,  MaxProgramSize);

                program = Translator.Translate(codeA, codeB, queryInfo, DefaultFlags);

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
                Span<byte> code = _context.MemoryAccessor.Read(gpuVa, MaxProgramSize);

                program = Translator.Translate(code, queryInfo, DefaultFlags);

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

        private void BackpropQualifiers(GraphicsShader program)
        {
            ShaderProgram fragmentShader = program.Shader[4].Program;

            bool isFirst = true;

            for (int stage = 3; stage >= 0; stage--)
            {
                if (program.Shader[stage].Program == null)
                {
                    continue;
                }

                // We need to iterate backwards, since we do name replacement,
                // and it would otherwise replace a subset of the longer names.
                for (int attr = 31; attr >= 0; attr--)
                {
                    string iq = fragmentShader?.Info.InterpolationQualifiers[attr].ToGlslQualifier() ?? string.Empty;

                    if (isFirst && iq != string.Empty)
                    {
                        program.Shader[stage].Program.Replace($"{DefineNames.OutQualifierPrefixName}{attr}", iq);
                    }
                    else
                    {
                        program.Shader[stage].Program.Replace($"{DefineNames.OutQualifierPrefixName}{attr} ", string.Empty);
                    }
                }

                isFirst = false;
            }
        }

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

        private bool QueryIsTextureBuffer(GpuState state, int stageIndex, int index)
        {
            return GetTextureDescriptor(state, stageIndex, index).UnpackTextureTarget() == TextureTarget.TextureBuffer;
        }

        private bool QueryIsTextureRectangle(GpuState state, int stageIndex, int index)
        {
            var descriptor = GetTextureDescriptor(state, stageIndex, index);

            TextureTarget target = descriptor.UnpackTextureTarget();

            bool is2DTexture = target == TextureTarget.Texture2D ||
                               target == TextureTarget.Texture2DRect;

            return !descriptor.UnpackTextureCoordNormalized() && is2DTexture;
        }

        private Image.TextureDescriptor GetTextureDescriptor(GpuState state, int stageIndex, int index)
        {
            return _context.Methods.TextureManager.GetGraphicsTextureDescriptor(state, stageIndex, index);
        }

        private int QueryInfoCommon(QueryInfoName info)
        {
            switch (info)
            {
                case QueryInfoName.MaximumViewportDimensions:
                    return _context.Capabilities.MaximumViewportDimensions;
                case QueryInfoName.StorageBufferOffsetAlignment:
                    return _context.Capabilities.StorageBufferOffsetAlignment;
                case QueryInfoName.SupportsNonConstantTextureOffset:
                    return Convert.ToInt32(_context.Capabilities.SupportsNonConstantTextureOffset);
            }

            return 0;
        }
    }
}