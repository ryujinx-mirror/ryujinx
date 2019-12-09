using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class ShaderCache
    {
        private const int MaxProgramSize = 0x100000;

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

        public GraphicsShader GetGraphicsShader(ShaderAddresses addresses, bool dividePosXY)
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

            TranslationFlags flags =
                TranslationFlags.DebugMode |
                TranslationFlags.Unspecialized;

            if (dividePosXY)
            {
                flags |= TranslationFlags.DividePosXY;
            }

            if (addresses.VertexA != 0)
            {
                gpShaders.Shader[0] = TranslateGraphicsShader(flags, addresses.Vertex, addresses.VertexA);
            }
            else
            {
                gpShaders.Shader[0] = TranslateGraphicsShader(flags, addresses.Vertex);
            }

            gpShaders.Shader[1] = TranslateGraphicsShader(flags, addresses.TessControl);
            gpShaders.Shader[2] = TranslateGraphicsShader(flags, addresses.TessEvaluation);
            gpShaders.Shader[3] = TranslateGraphicsShader(flags, addresses.Geometry);
            gpShaders.Shader[4] = TranslateGraphicsShader(flags, addresses.Fragment);

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

            ShaderProgram program;

            const TranslationFlags flags =
                TranslationFlags.Compute   |
                TranslationFlags.DebugMode |
                TranslationFlags.Unspecialized;

            Span<byte> code = _context.MemoryAccessor.Read(gpuVa, MaxProgramSize);

            program = Translator.Translate(code, GetShaderCapabilities(), flags);

            int[] codeCached = MemoryMarshal.Cast<byte, int>(code.Slice(0, program.Size)).ToArray();

            program.Replace(DefineNames.SharedMemorySize, (sharedMemorySize / 4).ToString(CultureInfo.InvariantCulture));

            program.Replace(DefineNames.LocalSizeX, localSizeX.ToString(CultureInfo.InvariantCulture));
            program.Replace(DefineNames.LocalSizeY, localSizeY.ToString(CultureInfo.InvariantCulture));
            program.Replace(DefineNames.LocalSizeZ, localSizeZ.ToString(CultureInfo.InvariantCulture));

            _dumper.Dump(code, compute: true, out string fullPath, out string codePath);

            if (fullPath != null && codePath != null)
            {
                program.Prepend("// " + codePath);
                program.Prepend("// " + fullPath);
            }

            return new CachedShader(program, codeCached);
        }

        private CachedShader TranslateGraphicsShader(TranslationFlags flags, ulong gpuVa, ulong gpuVaA = 0)
        {
            if (gpuVa == 0)
            {
                return new CachedShader(null, null);
            }

            ShaderProgram program;

            int[] codeCached = null;

            if (gpuVaA != 0)
            {
                Span<byte> codeA = _context.MemoryAccessor.Read(gpuVaA, MaxProgramSize);
                Span<byte> codeB = _context.MemoryAccessor.Read(gpuVa,  MaxProgramSize);

                program = Translator.Translate(codeA, codeB, GetShaderCapabilities(), flags);

                // TODO: We should also check "codeA" into account.
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

                program = Translator.Translate(code, GetShaderCapabilities(), flags);

                codeCached = MemoryMarshal.Cast<byte, int>(code.Slice(0, program.Size)).ToArray();

                _dumper.Dump(code, compute: false, out string fullPath, out string codePath);

                if (fullPath != null && codePath != null)
                {
                    program.Prepend("// " + codePath);
                    program.Prepend("// " + fullPath);
                }
            }

            if (program.Stage == ShaderStage.Geometry)
            {
                PrimitiveType primitiveType = _context.Methods.PrimitiveType;

                string inPrimitive = "points";

                switch (primitiveType)
                {
                    case PrimitiveType.Points:
                        inPrimitive = "points";
                        break;
                    case PrimitiveType.Lines:
                    case PrimitiveType.LineLoop:
                    case PrimitiveType.LineStrip:
                        inPrimitive = "lines";
                        break;
                    case PrimitiveType.LinesAdjacency:
                    case PrimitiveType.LineStripAdjacency:
                        inPrimitive = "lines_adjacency";
                        break;
                    case PrimitiveType.Triangles:
                    case PrimitiveType.TriangleStrip:
                    case PrimitiveType.TriangleFan:
                        inPrimitive = "triangles";
                        break;
                    case PrimitiveType.TrianglesAdjacency:
                    case PrimitiveType.TriangleStripAdjacency:
                        inPrimitive = "triangles_adjacency";
                        break;
                }

                program.Replace(DefineNames.InputTopologyName, inPrimitive);
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

        private ShaderCapabilities GetShaderCapabilities()
        {
            return new ShaderCapabilities(
                _context.Capabilities.MaximumViewportDimensions,
                _context.Capabilities.MaximumComputeSharedMemorySize,
                _context.Capabilities.StorageBufferOffsetAlignment);
        }
    }
}