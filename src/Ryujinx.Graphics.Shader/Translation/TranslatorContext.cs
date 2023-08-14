using Ryujinx.Graphics.Shader.CodeGen;
using Ryujinx.Graphics.Shader.CodeGen.Glsl;
using Ryujinx.Graphics.Shader.CodeGen.Spirv;
using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation.Optimizations;
using Ryujinx.Graphics.Shader.Translation.Transforms;
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
        private readonly int _localMemorySize;

        public ulong Address { get; }
        public int Size { get; }
        public int Cb1DataSize => _program.Cb1DataSize;

        internal bool HasLayerInputAttribute { get; private set; }
        internal int GpLayerInputAttribute { get; private set; }

        internal AttributeUsage AttributeUsage => _program.AttributeUsage;

        internal ShaderDefinitions Definitions { get; }

        public ShaderStage Stage => Definitions.Stage;

        internal IGpuAccessor GpuAccessor { get; }

        internal TranslationOptions Options { get; }

        internal FeatureFlags UsedFeatures { get; private set; }

        public bool LayerOutputWritten { get; private set; }
        public int LayerOutputAttribute { get; private set; }

        internal TranslatorContext(
            ulong address,
            int size,
            int localMemorySize,
            ShaderDefinitions definitions,
            IGpuAccessor gpuAccessor,
            TranslationOptions options,
            DecodedProgram program)
        {
            Address = address;
            Size = size;
            _program = program;
            _localMemorySize = localMemorySize;
            Definitions = definitions;
            GpuAccessor = gpuAccessor;
            Options = options;
            UsedFeatures = program.UsedFeatures;
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

            List<Operation> ops = new(a.Length + b.Length);

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

        internal int GetDepthRegister()
        {
            // The depth register is always two registers after the last color output.
            return BitOperations.PopCount((uint)Definitions.OmapTargets) + 1;
        }

        public void SetLayerOutputAttribute(int attr)
        {
            LayerOutputWritten = true;
            LayerOutputAttribute = attr;
        }

        public void SetGeometryShaderLayerInputAttribute(int attr)
        {
            UsedFeatures |= FeatureFlags.RtLayer;
            HasLayerInputAttribute = true;
            GpLayerInputAttribute = attr;
        }

        public void SetLastInVertexPipeline()
        {
            Definitions.LastInVertexPipeline = true;
        }

        public void SetNextStage(TranslatorContext nextStage)
        {
            AttributeUsage.MergeFromtNextStage(
                Definitions.GpPassthrough,
                nextStage.UsedFeatures.HasFlag(FeatureFlags.FixedFuncAttr),
                nextStage.AttributeUsage);

            // We don't consider geometry shaders using the geometry shader passthrough feature
            // as being the last because when this feature is used, it can't actually modify any of the outputs,
            // so the stage that comes before it is the last one that can do modifications.
            if (nextStage.Definitions.Stage != ShaderStage.Fragment &&
                (nextStage.Definitions.Stage != ShaderStage.Geometry || !nextStage.Definitions.GpPassthrough))
            {
                Definitions.LastInVertexPipeline = false;
            }
        }

        public ShaderProgram Translate()
        {
            ResourceManager resourceManager = CreateResourceManager();

            bool usesLocalMemory = _program.UsedFeatures.HasFlag(FeatureFlags.LocalMemory);

            resourceManager.SetCurrentLocalMemory(_localMemorySize, usesLocalMemory);

            if (Stage == ShaderStage.Compute)
            {
                bool usesSharedMemory = _program.UsedFeatures.HasFlag(FeatureFlags.SharedMemory);

                resourceManager.SetCurrentSharedMemory(GpuAccessor.QueryComputeSharedMemorySize(), usesSharedMemory);
            }

            FunctionCode[] code = EmitShader(this, resourceManager, _program, initializeOutputs: true, out _);

            return Translate(code, resourceManager, UsedFeatures, _program.ClipDistancesWritten);
        }

        public ShaderProgram Translate(TranslatorContext other)
        {
            ResourceManager resourceManager = CreateResourceManager();

            bool usesLocalMemory = _program.UsedFeatures.HasFlag(FeatureFlags.LocalMemory);
            resourceManager.SetCurrentLocalMemory(_localMemorySize, usesLocalMemory);

            FunctionCode[] code = EmitShader(this, resourceManager, _program, initializeOutputs: false, out _);

            bool otherUsesLocalMemory = other._program.UsedFeatures.HasFlag(FeatureFlags.LocalMemory);
            resourceManager.SetCurrentLocalMemory(other._localMemorySize, otherUsesLocalMemory);

            FunctionCode[] otherCode = EmitShader(other, resourceManager, other._program, initializeOutputs: true, out int aStart);

            code = Combine(otherCode, code, aStart);

            return Translate(
                code,
                resourceManager,
                UsedFeatures | other.UsedFeatures,
                (byte)(_program.ClipDistancesWritten | other._program.ClipDistancesWritten));
        }

        private ShaderProgram Translate(FunctionCode[] functions, ResourceManager resourceManager, FeatureFlags usedFeatures, byte clipDistancesWritten)
        {
            var cfgs = new ControlFlowGraph[functions.Length];
            var frus = new RegisterUsage.FunctionRegisterUsage[functions.Length];

            for (int i = 0; i < functions.Length; i++)
            {
                cfgs[i] = ControlFlowGraph.Create(functions[i].Code);

                if (i != 0)
                {
                    frus[i] = RegisterUsage.RunPass(cfgs[i]);
                }
            }

            List<Function> funcs = new(functions.Length);

            for (int i = 0; i < functions.Length; i++)
            {
                funcs.Add(null);
            }

            HelperFunctionManager hfm = new(funcs, Definitions.Stage);

            for (int i = 0; i < functions.Length; i++)
            {
                var cfg = cfgs[i];

                int inArgumentsCount = 0;
                int outArgumentsCount = 0;

                if (i != 0)
                {
                    var fru = frus[i];

                    inArgumentsCount = fru.InArguments.Length;
                    outArgumentsCount = fru.OutArguments.Length;
                }

                if (cfg.Blocks.Length != 0)
                {
                    RegisterUsage.FixupCalls(cfg.Blocks, frus);

                    Dominance.FindDominators(cfg);
                    Dominance.FindDominanceFrontiers(cfg.Blocks);

                    Ssa.Rename(cfg.Blocks);

                    TransformContext context = new(
                        hfm,
                        cfg.Blocks,
                        resourceManager,
                        GpuAccessor,
                        Options.TargetLanguage,
                        Definitions.Stage,
                        ref usedFeatures);

                    Optimizer.RunPass(context);
                    TransformPasses.RunPass(context);
                }

                funcs[i] = new Function(cfg.Blocks, $"fun{i}", false, inArgumentsCount, outArgumentsCount);
            }

            var identification = ShaderIdentifier.Identify(funcs, GpuAccessor, Definitions.Stage, Definitions.InputTopology, out int layerInputAttr);

            return Generate(
                funcs,
                AttributeUsage,
                Definitions,
                resourceManager,
                usedFeatures,
                clipDistancesWritten,
                identification,
                layerInputAttr);
        }

        private ShaderProgram Generate(
            IReadOnlyList<Function> funcs,
            AttributeUsage attributeUsage,
            ShaderDefinitions definitions,
            ResourceManager resourceManager,
            FeatureFlags usedFeatures,
            byte clipDistancesWritten,
            ShaderIdentification identification = ShaderIdentification.None,
            int layerInputAttr = 0)
        {
            var sInfo = StructuredProgram.MakeStructuredProgram(
                funcs,
                attributeUsage,
                definitions,
                resourceManager,
                Options.Flags.HasFlag(TranslationFlags.DebugMode));

            var info = new ShaderProgramInfo(
                resourceManager.GetConstantBufferDescriptors(),
                resourceManager.GetStorageBufferDescriptors(),
                resourceManager.GetTextureDescriptors(),
                resourceManager.GetImageDescriptors(),
                identification,
                layerInputAttr,
                definitions.Stage,
                usedFeatures.HasFlag(FeatureFlags.FragCoordXY),
                usedFeatures.HasFlag(FeatureFlags.InstanceId),
                usedFeatures.HasFlag(FeatureFlags.DrawParameters),
                usedFeatures.HasFlag(FeatureFlags.RtLayer),
                clipDistancesWritten,
                definitions.OmapTargets);

            var hostCapabilities = new HostCapabilities(
                GpuAccessor.QueryHostReducedPrecision(),
                GpuAccessor.QueryHostSupportsFragmentShaderInterlock(),
                GpuAccessor.QueryHostSupportsFragmentShaderOrderingIntel(),
                GpuAccessor.QueryHostSupportsGeometryShaderPassthrough(),
                GpuAccessor.QueryHostSupportsShaderBallot(),
                GpuAccessor.QueryHostSupportsShaderBarrierDivergence(),
                GpuAccessor.QueryHostSupportsTextureShadowLod(),
                GpuAccessor.QueryHostSupportsViewportMask());

            var parameters = new CodeGenParameters(attributeUsage, definitions, resourceManager.Properties, hostCapabilities, GpuAccessor, Options.TargetApi);

            return Options.TargetLanguage switch
            {
                TargetLanguage.Glsl => new ShaderProgram(info, TargetLanguage.Glsl, GlslGenerator.Generate(sInfo, parameters)),
                TargetLanguage.Spirv => new ShaderProgram(info, TargetLanguage.Spirv, SpirvGenerator.Generate(sInfo, parameters)),
                _ => throw new NotImplementedException(Options.TargetLanguage.ToString()),
            };
        }

        private ResourceManager CreateResourceManager()
        {
            ResourceManager resourceManager = new(Definitions.Stage, GpuAccessor);

            if (!GpuAccessor.QueryHostSupportsTransformFeedback() && GpuAccessor.QueryTransformFeedbackEnabled())
            {
                StructureType tfeInfoStruct = new(new StructureField[]
                {
                    new StructureField(AggregateType.Array | AggregateType.U32, "base_offset", 4),
                    new StructureField(AggregateType.U32, "vertex_count")
                });

                BufferDefinition tfeInfoBuffer = new(BufferLayout.Std430, 1, Constants.TfeInfoBinding, "tfe_info", tfeInfoStruct);
                resourceManager.Properties.AddOrUpdateStorageBuffer(tfeInfoBuffer);

                StructureType tfeDataStruct = new(new StructureField[]
                {
                    new StructureField(AggregateType.Array | AggregateType.U32, "data", 0)
                });

                for (int i = 0; i < Constants.TfeBuffersCount; i++)
                {
                    int binding = Constants.TfeBufferBaseBinding + i;
                    BufferDefinition tfeDataBuffer = new(BufferLayout.Std430, 1, binding, $"tfe_data{i}", tfeDataStruct);
                    resourceManager.Properties.AddOrUpdateStorageBuffer(tfeDataBuffer);
                }
            }

            return resourceManager;
        }

        public ShaderProgram GenerateGeometryPassthrough()
        {
            int outputAttributesMask = AttributeUsage.UsedOutputAttributes;
            int layerOutputAttr = LayerOutputAttribute;

            OutputTopology outputTopology;
            int maxOutputVertices;

            switch (Definitions.InputTopology)
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

            var attributeUsage = new AttributeUsage(GpuAccessor);
            var resourceManager = new ResourceManager(ShaderStage.Geometry, GpuAccessor);

            var context = new EmitterContext();

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
                        }
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

            var definitions = new ShaderDefinitions(
                ShaderStage.Geometry,
                GpuAccessor.QueryGraphicsState(),
                false,
                1,
                outputTopology,
                maxOutputVertices);

            return Generate(new[] { function }, attributeUsage, definitions, resourceManager, FeatureFlags.RtLayer, 0);
        }
    }
}
