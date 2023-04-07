using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.CodeGen.Spirv;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;
using static Ryujinx.Graphics.Shader.Translation.Translator;

namespace Ryujinx.Graphics.Shader.Translation
{
    public class TranslatorContext
    {
        private readonly DecodedProgram _program;
        private ShaderConfig _config;

        public ulong Address { get; }

        public ShaderStage Stage => _config.Stage;
        public int Size => _config.Size;
        public int Cb1DataSize => _config.Cb1DataSize;
        public bool LayerOutputWritten => _config.LayerOutputWritten;

        public IGpuAccessor GpuAccessor => _config.GpuAccessor;

        internal TranslatorContext(ulong address, DecodedProgram program, ShaderConfig config)
        {
            Address = address;
            _program = program;
            _config = config;
        }

        private static bool IsLoadUserDefined(Operation operation)
        {
            // TODO: Check if sources count match and all sources are constant.
            return operation.Inst == Instruction.Load && (IoVariable)operation.GetSource(0).Value == IoVariable.UserDefined;
        }

        private static bool IsStoreUserDefined(Operation operation)
        {
            // TODO: Check if sources count match and all sources are constant.
            return operation.Inst == Instruction.Store && (IoVariable)operation.GetSource(0).Value == IoVariable.UserDefined;
        }

        private static FunctionCode[] Combine(FunctionCode[] a, FunctionCode[] b, int aStart)
        {
            // Here we combine two shaders.
            // For shader A:
            // - All user attribute stores on shader A are turned into copies to a
            // temporary variable. It's assumed that shader B will consume them.
            // - All return instructions are turned into branch instructions, the
            // branch target being the start of the shader B code.
            // For shader B:
            // - All user attribute loads on shader B are turned into copies from a
            // temporary variable, as long that attribute is written by shader A.
            FunctionCode[] output = new FunctionCode[a.Length + b.Length - 1];

            List<Operation> ops = new List<Operation>(a.Length + b.Length);

            Operand[] temps = new Operand[AttributeConsts.UserAttributesCount * 4];

            Operand lblB = Label();

            for (int index = aStart; index < a[0].Code.Length; index++)
            {
                Operation operation = a[0].Code[index];

                if (IsStoreUserDefined(operation))
                {
                    int tIndex = operation.GetSource(1).Value * 4 + operation.GetSource(2).Value;

                    Operand temp = temps[tIndex];

                    if (temp == null)
                    {
                        temp = Local();

                        temps[tIndex] = temp;
                    }

                    operation.Dest = temp;
                    operation.TurnIntoCopy(operation.GetSource(operation.SourcesCount - 1));
                }

                if (operation.Inst == Instruction.Return)
                {
                    ops.Add(new Operation(Instruction.Branch, lblB));
                }
                else
                {
                    ops.Add(operation);
                }
            }

            ops.Add(new Operation(Instruction.MarkLabel, lblB));

            for (int index = 0; index < b[0].Code.Length; index++)
            {
                Operation operation = b[0].Code[index];

                if (IsLoadUserDefined(operation))
                {
                    int tIndex = operation.GetSource(1).Value * 4 + operation.GetSource(2).Value;

                    Operand temp = temps[tIndex];

                    if (temp != null)
                    {
                        operation.TurnIntoCopy(temp);
                    }
                }

                ops.Add(operation);
            }

            output[0] = new FunctionCode(ops.ToArray());

            for (int i = 1; i < a.Length; i++)
            {
                output[i] = a[i];
            }

            for (int i = 1; i < b.Length; i++)
            {
                output[a.Length + i - 1] = b[i];
            }

            return output;
        }

        public void SetNextStage(TranslatorContext nextStage)
        {
            _config.MergeFromtNextStage(nextStage._config);
        }

        public void SetGeometryShaderLayerInputAttribute(int attr)
        {
            _config.SetGeometryShaderLayerInputAttribute(attr);
        }

        public void SetLastInVertexPipeline()
        {
            _config.SetLastInVertexPipeline();
        }

        public ShaderProgram Translate(TranslatorContext other = null)
        {
            FunctionCode[] code = EmitShader(_program, _config, initializeOutputs: other == null, out _);

            if (other != null)
            {
                other._config.MergeOutputUserAttributes(_config.UsedOutputAttributes, Enumerable.Empty<int>());

                FunctionCode[] otherCode = EmitShader(other._program, other._config, initializeOutputs: true, out int aStart);

                code = Combine(otherCode, code, aStart);

                _config.InheritFrom(other._config);
            }

            return Translator.Translate(code, _config);
        }

        public ShaderProgram GenerateGeometryPassthrough()
        {
            int outputAttributesMask = _config.UsedOutputAttributes;
            int layerOutputAttr = _config.LayerOutputAttribute;

            OutputTopology outputTopology;
            int maxOutputVertices;

            switch (GpuAccessor.QueryPrimitiveTopology())
            {
                case InputTopology.Points:
                    outputTopology = OutputTopology.PointList;
                    maxOutputVertices = 1;
                    break;
                case InputTopology.Lines:
                case InputTopology.LinesAdjacency:
                    outputTopology = OutputTopology.LineStrip;
                    maxOutputVertices = 2;
                    break;
                default:
                    outputTopology = OutputTopology.TriangleStrip;
                    maxOutputVertices = 3;
                    break;
            }

            ShaderConfig config = new ShaderConfig(ShaderStage.Geometry, outputTopology, maxOutputVertices, GpuAccessor, _config.Options);

            EmitterContext context = new EmitterContext(default, config, false);

            for (int v = 0; v < maxOutputVertices; v++)
            {
                int outAttrsMask = outputAttributesMask;

                while (outAttrsMask != 0)
                {
                    int attrIndex = BitOperations.TrailingZeroCount(outAttrsMask);

                    outAttrsMask &= ~(1 << attrIndex);

                    for (int c = 0; c < 4; c++)
                    {
                        int attr = AttributeConsts.UserAttributeBase + attrIndex * 16 + c * 4;

                        Operand value = context.Load(StorageKind.Input, IoVariable.UserDefined, Const(attrIndex), Const(v), Const(c));

                        if (attr == layerOutputAttr)
                        {
                            context.Store(StorageKind.Output, IoVariable.Layer, null, value);
                        }
                        else
                        {
                            context.Store(StorageKind.Output, IoVariable.UserDefined, null, Const(attrIndex), Const(c), value);
                            config.SetOutputUserAttribute(attrIndex);
                        }

                        config.SetInputUserAttribute(attrIndex, c);
                    }
                }

                for (int c = 0; c < 4; c++)
                {
                    Operand value = context.Load(StorageKind.Input, IoVariable.Position, Const(v), Const(c));

                    context.Store(StorageKind.Output, IoVariable.Position, null, Const(c), value);
                }

                context.EmitVertex();
            }

            context.EndPrimitive();

            var operations = context.GetOperations();
            var cfg = ControlFlowGraph.Create(operations);
            var function = new Function(cfg.Blocks, "main", false, 0, 0);

            var sInfo = StructuredProgram.MakeStructuredProgram(new[] { function }, config);

            var info = config.CreateProgramInfo();

            return config.Options.TargetLanguage switch
            {
                TargetLanguage.Glsl => new ShaderProgram(info, TargetLanguage.Glsl, GlslGenerator.Generate(sInfo, config)),
                TargetLanguage.Spirv => new ShaderProgram(info, TargetLanguage.Spirv, SpirvGenerator.Generate(sInfo, config)),
                _ => throw new NotImplementedException(config.Options.TargetLanguage.ToString())
            };
        }
    }
}
