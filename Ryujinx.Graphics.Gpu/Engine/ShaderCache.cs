using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ryujinx.Graphics.Gpu.Engine
{
    class ShaderCache
    {
        private const int MaxProgramSize = 0x100000;

        private GpuContext _context;

        private ShaderDumper _dumper;

        private Dictionary<ulong, ComputeShader> _cpPrograms;

        private Dictionary<ShaderAddresses, GraphicsShader> _gpPrograms;

        public ShaderCache(GpuContext context)
        {
            _context = context;

            _dumper = new ShaderDumper(context);

            _cpPrograms = new Dictionary<ulong, ComputeShader>();

            _gpPrograms = new Dictionary<ShaderAddresses, GraphicsShader>();
        }

        public ComputeShader GetComputeShader(ulong gpuVa, int localSizeX, int localSizeY, int localSizeZ)
        {
            if (!_cpPrograms.TryGetValue(gpuVa, out ComputeShader cpShader))
            {
                ShaderProgram shader = TranslateComputeShader(gpuVa);

                shader.Replace(DefineNames.LocalSizeX, localSizeX.ToString(CultureInfo.InvariantCulture));
                shader.Replace(DefineNames.LocalSizeY, localSizeY.ToString(CultureInfo.InvariantCulture));
                shader.Replace(DefineNames.LocalSizeZ, localSizeZ.ToString(CultureInfo.InvariantCulture));

                IShader hostShader = _context.Renderer.CompileShader(shader);

                IProgram program = _context.Renderer.CreateProgram(new IShader[] { hostShader });

                cpShader = new ComputeShader(program, shader);

                _cpPrograms.Add(gpuVa, cpShader);
            }

            return cpShader;
        }

        public GraphicsShader GetGraphicsShader(ShaderAddresses addresses)
        {
            if (!_gpPrograms.TryGetValue(addresses, out GraphicsShader gpShader))
            {
                gpShader = new GraphicsShader();

                if (addresses.VertexA != 0)
                {
                    gpShader.Shader[0] = TranslateGraphicsShader(addresses.Vertex, addresses.VertexA);
                }
                else
                {
                    gpShader.Shader[0] = TranslateGraphicsShader(addresses.Vertex);
                }

                gpShader.Shader[1] = TranslateGraphicsShader(addresses.TessControl);
                gpShader.Shader[2] = TranslateGraphicsShader(addresses.TessEvaluation);
                gpShader.Shader[3] = TranslateGraphicsShader(addresses.Geometry);
                gpShader.Shader[4] = TranslateGraphicsShader(addresses.Fragment);

                BackpropQualifiers(gpShader);

                List<IShader> shaders = new List<IShader>();

                for (int stage = 0; stage < gpShader.Shader.Length; stage++)
                {
                    if (gpShader.Shader[stage] == null)
                    {
                        continue;
                    }

                    IShader shader = _context.Renderer.CompileShader(gpShader.Shader[stage]);

                    shaders.Add(shader);
                }

                gpShader.Interface = _context.Renderer.CreateProgram(shaders.ToArray());

                _gpPrograms.Add(addresses, gpShader);
            }

            return gpShader;
        }

        private ShaderProgram TranslateComputeShader(ulong gpuVa)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            ShaderProgram program;

            const TranslationFlags flags =
                TranslationFlags.Compute |
                TranslationFlags.Unspecialized;

            TranslationConfig translationConfig = new TranslationConfig(0x10000, _dumper.CurrentDumpIndex, flags);

            Span<byte> code = _context.MemoryAccessor.Read(gpuVa, MaxProgramSize);

            program = Translator.Translate(code, translationConfig);

            _dumper.Dump(gpuVa, compute : true);

            return program;
        }

        private ShaderProgram TranslateGraphicsShader(ulong gpuVa, ulong gpuVaA = 0)
        {
            if (gpuVa == 0)
            {
                return null;
            }

            ShaderProgram program;

            const TranslationFlags flags =
                TranslationFlags.DebugMode |
                TranslationFlags.Unspecialized;

            TranslationConfig translationConfig = new TranslationConfig(0x10000, _dumper.CurrentDumpIndex, flags);

            if (gpuVaA != 0)
            {
                Span<byte> codeA = _context.MemoryAccessor.Read(gpuVaA, MaxProgramSize);
                Span<byte> codeB = _context.MemoryAccessor.Read(gpuVa,  MaxProgramSize);

                program = Translator.Translate(codeA, codeB, translationConfig);

                _dumper.Dump(gpuVaA, compute: false);
                _dumper.Dump(gpuVa,  compute: false);
            }
            else
            {
                Span<byte> code = _context.MemoryAccessor.Read(gpuVa, MaxProgramSize);

                program = Translator.Translate(code, translationConfig);

                _dumper.Dump(gpuVa, compute: false);
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

            return program;
        }

        private void BackpropQualifiers(GraphicsShader program)
        {
            ShaderProgram fragmentShader = program.Shader[4];

            bool isFirst = true;

            for (int stage = 3; stage >= 0; stage--)
            {
                if (program.Shader[stage] == null)
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
                        program.Shader[stage].Replace($"{DefineNames.OutQualifierPrefixName}{attr}", iq);
                    }
                    else
                    {
                        program.Shader[stage].Replace($"{DefineNames.OutQualifierPrefixName}{attr} ", string.Empty);
                    }
                }

                isFirst = false;
            }
        }
    }
}