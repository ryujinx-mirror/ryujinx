using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    class EmitterContext
    {
        public DecodedProgram Program { get; }
        public ShaderConfig Config { get; }

        public bool IsNonMain { get; }

        public Block CurrBlock { get; set; }
        public InstOp CurrOp { get; set; }

        public int OperationsCount => _operations.Count;

        private readonly struct BrxTarget
        {
            public readonly Operand Selector;
            public readonly int ExpectedValue;
            public readonly ulong NextTargetAddress;

            public BrxTarget(Operand selector, int expectedValue, ulong nextTargetAddress)
            {
                Selector = selector;
                ExpectedValue = expectedValue;
                NextTargetAddress = nextTargetAddress;
            }
        }

        private class BlockLabel
        {
            public readonly Operand Label;
            public BrxTarget BrxTarget;

            public BlockLabel(Operand label)
            {
                Label = label;
            }
        }

        private readonly List<Operation> _operations;
        private readonly Dictionary<ulong, BlockLabel> _labels;

        public EmitterContext()
        {
            _operations = new List<Operation>();
            _labels = new Dictionary<ulong, BlockLabel>();
        }

        public EmitterContext(DecodedProgram program, ShaderConfig config, bool isNonMain) : this()
        {
            Program = program;
            Config = config;
            IsNonMain = isNonMain;

            EmitStart();
        }

        private void EmitStart()
        {
            if (Config.Stage == ShaderStage.Vertex &&
                Config.Options.TargetApi == TargetApi.Vulkan &&
                (Config.Options.Flags & TranslationFlags.VertexA) == 0)
            {
                // Vulkan requires the point size to be always written on the shader if the primitive topology is points.
                this.Store(StorageKind.Output, IoVariable.PointSize, null, ConstF(Config.GpuAccessor.QueryPointSize()));
            }
        }

        public T GetOp<T>() where T : unmanaged
        {
            Debug.Assert(Unsafe.SizeOf<T>() == sizeof(ulong));
            ulong op = CurrOp.RawOpCode;
            return Unsafe.As<ulong, T>(ref op);
        }

        public Operand Add(Instruction inst, Operand dest = null, params Operand[] sources)
        {
            Operation operation = new(inst, dest, sources);

            _operations.Add(operation);

            return dest;
        }

        public Operand Add(Instruction inst, StorageKind storageKind, Operand dest = null, params Operand[] sources)
        {
            Operation operation = new(inst, storageKind, dest, sources);

            _operations.Add(operation);

            return dest;
        }

        public (Operand, Operand) Add(Instruction inst, (Operand, Operand) dest, params Operand[] sources)
        {
            Operand[] dests = new[] { dest.Item1, dest.Item2 };

            Operation operation = new(inst, 0, dests, sources);

            Add(operation);

            return dest;
        }

        public void Add(Operation operation)
        {
            _operations.Add(operation);
        }

        public TextureOperation CreateTextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFlags flags,
            int handle,
            int compIndex,
            Operand[] dests,
            params Operand[] sources)
        {
            return CreateTextureOperation(inst, type, TextureFormat.Unknown, flags, handle, compIndex, dests, sources);
        }

        public TextureOperation CreateTextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int handle,
            int compIndex,
            Operand[] dests,
            params Operand[] sources)
        {
            if (!flags.HasFlag(TextureFlags.Bindless))
            {
                Config.SetUsedTexture(inst, type, format, flags, TextureOperation.DefaultCbufSlot, handle);
            }

            return new TextureOperation(inst, type, format, flags, handle, compIndex, dests, sources);
        }

        public void FlagAttributeRead(int attribute)
        {
            if (Config.Stage == ShaderStage.Vertex && attribute == AttributeConsts.InstanceId)
            {
                Config.SetUsedFeature(FeatureFlags.InstanceId);
            }
            else if (Config.Stage == ShaderStage.Fragment)
            {
                switch (attribute)
                {
                    case AttributeConsts.PositionX:
                    case AttributeConsts.PositionY:
                        Config.SetUsedFeature(FeatureFlags.FragCoordXY);
                        break;
                }
            }
        }

        public void FlagAttributeWritten(int attribute)
        {
            if (Config.Stage == ShaderStage.Vertex)
            {
                switch (attribute)
                {
                    case AttributeConsts.ClipDistance0:
                    case AttributeConsts.ClipDistance1:
                    case AttributeConsts.ClipDistance2:
                    case AttributeConsts.ClipDistance3:
                    case AttributeConsts.ClipDistance4:
                    case AttributeConsts.ClipDistance5:
                    case AttributeConsts.ClipDistance6:
                    case AttributeConsts.ClipDistance7:
                        Config.SetClipDistanceWritten((attribute - AttributeConsts.ClipDistance0) / 4);
                        break;
                }
            }

            if (Config.Stage != ShaderStage.Fragment && attribute == AttributeConsts.Layer)
            {
                Config.SetUsedFeature(FeatureFlags.RtLayer);
            }
        }

        public void MarkLabel(Operand label)
        {
            Add(Instruction.MarkLabel, label);
        }

        public Operand GetLabel(ulong address)
        {
            return EnsureBlockLabel(address).Label;
        }

        public void SetBrxTarget(ulong address, Operand selector, int targetValue, ulong nextTargetAddress)
        {
            BlockLabel blockLabel = EnsureBlockLabel(address);
            Debug.Assert(blockLabel.BrxTarget.Selector == null);
            blockLabel.BrxTarget = new BrxTarget(selector, targetValue, nextTargetAddress);
        }

        public void EnterBlock(ulong address)
        {
            BlockLabel blockLabel = EnsureBlockLabel(address);

            MarkLabel(blockLabel.Label);

            BrxTarget brxTarget = blockLabel.BrxTarget;

            if (brxTarget.Selector != null)
            {
                this.BranchIfFalse(GetLabel(brxTarget.NextTargetAddress), this.ICompareEqual(brxTarget.Selector, Const(brxTarget.ExpectedValue)));
            }
        }

        private BlockLabel EnsureBlockLabel(ulong address)
        {
            if (!_labels.TryGetValue(address, out BlockLabel blockLabel))
            {
                blockLabel = new BlockLabel(Label());

                _labels.Add(address, blockLabel);
            }

            return blockLabel;
        }

        public void PrepareForVertexReturn()
        {
            if (!Config.GpuAccessor.QueryHostSupportsTransformFeedback() && Config.GpuAccessor.QueryTransformFeedbackEnabled())
            {
                Operand vertexCount = this.Load(StorageKind.StorageBuffer, Constants.TfeInfoBinding, Const(1));

                for (int tfbIndex = 0; tfbIndex < Constants.TfeBuffersCount; tfbIndex++)
                {
                    var locations = Config.GpuAccessor.QueryTransformFeedbackVaryingLocations(tfbIndex);
                    var stride = Config.GpuAccessor.QueryTransformFeedbackStride(tfbIndex);

                    Operand baseOffset = this.Load(StorageKind.StorageBuffer, Constants.TfeInfoBinding, Const(0), Const(tfbIndex));
                    Operand baseVertex = this.Load(StorageKind.Input, IoVariable.BaseVertex);
                    Operand baseInstance = this.Load(StorageKind.Input, IoVariable.BaseInstance);
                    Operand vertexIndex = this.Load(StorageKind.Input, IoVariable.VertexIndex);
                    Operand instanceIndex = this.Load(StorageKind.Input, IoVariable.InstanceIndex);

                    Operand outputVertexOffset = this.ISubtract(vertexIndex, baseVertex);
                    Operand outputInstanceOffset = this.ISubtract(instanceIndex, baseInstance);

                    Operand outputBaseVertex = this.IMultiply(outputInstanceOffset, vertexCount);

                    Operand vertexOffset = this.IMultiply(this.IAdd(outputBaseVertex, outputVertexOffset), Const(stride / 4));
                    baseOffset = this.IAdd(baseOffset, vertexOffset);

                    for (int j = 0; j < locations.Length; j++)
                    {
                        byte location = locations[j];
                        if (location == 0xff)
                        {
                            continue;
                        }

                        Operand offset = this.IAdd(baseOffset, Const(j));
                        Operand value = Instructions.AttributeMap.GenerateAttributeLoad(this, null, location * 4, isOutput: true, isPerPatch: false);

                        this.Store(StorageKind.StorageBuffer, Constants.TfeBufferBaseBinding + tfbIndex, Const(0), offset, value);
                    }
                }
            }

            if (Config.GpuAccessor.QueryViewportTransformDisable())
            {
                Operand x = this.Load(StorageKind.Output, IoVariable.Position, null, Const(0));
                Operand y = this.Load(StorageKind.Output, IoVariable.Position, null, Const(1));
                Operand xScale = this.Load(StorageKind.ConstantBuffer, SupportBuffer.Binding, Const((int)SupportBufferField.ViewportInverse), Const(0));
                Operand yScale = this.Load(StorageKind.ConstantBuffer, SupportBuffer.Binding, Const((int)SupportBufferField.ViewportInverse), Const(1));
                Operand negativeOne = ConstF(-1.0f);

                this.Store(StorageKind.Output, IoVariable.Position, null, Const(0), this.FPFusedMultiplyAdd(x, xScale, negativeOne));
                this.Store(StorageKind.Output, IoVariable.Position, null, Const(1), this.FPFusedMultiplyAdd(y, yScale, negativeOne));
            }

            if (Config.GpuAccessor.QueryTransformDepthMinusOneToOne() && !Config.GpuAccessor.QueryHostSupportsDepthClipControl())
            {
                Operand z = this.Load(StorageKind.Output, IoVariable.Position, null, Const(2));
                Operand w = this.Load(StorageKind.Output, IoVariable.Position, null, Const(3));
                Operand halfW = this.FPMultiply(w, ConstF(0.5f));

                this.Store(StorageKind.Output, IoVariable.Position, null, Const(2), this.FPFusedMultiplyAdd(z, ConstF(0.5f), halfW));
            }

            if (Config.Stage != ShaderStage.Geometry && Config.HasLayerInputAttribute)
            {
                Config.SetUsedFeature(FeatureFlags.RtLayer);

                int attrVecIndex = Config.GpLayerInputAttribute >> 2;
                int attrComponentIndex = Config.GpLayerInputAttribute & 3;

                Operand layer = this.Load(StorageKind.Output, IoVariable.UserDefined, null, Const(attrVecIndex), Const(attrComponentIndex));

                this.Store(StorageKind.Output, IoVariable.Layer, null, layer);
            }
        }

        public void PrepareForVertexReturn(out Operand oldXLocal, out Operand oldYLocal, out Operand oldZLocal)
        {
            if (Config.GpuAccessor.QueryViewportTransformDisable())
            {
                oldXLocal = Local();
                this.Copy(oldXLocal, this.Load(StorageKind.Output, IoVariable.Position, null, Const(0)));
                oldYLocal = Local();
                this.Copy(oldYLocal, this.Load(StorageKind.Output, IoVariable.Position, null, Const(1)));
            }
            else
            {
                oldXLocal = null;
                oldYLocal = null;
            }

            if (Config.GpuAccessor.QueryTransformDepthMinusOneToOne() && !Config.GpuAccessor.QueryHostSupportsDepthClipControl())
            {
                oldZLocal = Local();
                this.Copy(oldZLocal, this.Load(StorageKind.Output, IoVariable.Position, null, Const(2)));
            }
            else
            {
                oldZLocal = null;
            }

            PrepareForVertexReturn();
        }

        public void PrepareForReturn()
        {
            if (IsNonMain)
            {
                return;
            }

            if (Config.LastInVertexPipeline &&
                (Config.Stage == ShaderStage.Vertex || Config.Stage == ShaderStage.TessellationEvaluation) &&
                (Config.Options.Flags & TranslationFlags.VertexA) == 0)
            {
                PrepareForVertexReturn();
            }
            else if (Config.Stage == ShaderStage.Geometry)
            {
                void WritePositionOutput(int primIndex)
                {
                    Operand x = this.Load(StorageKind.Input, IoVariable.Position, Const(primIndex), Const(0));
                    Operand y = this.Load(StorageKind.Input, IoVariable.Position, Const(primIndex), Const(1));
                    Operand z = this.Load(StorageKind.Input, IoVariable.Position, Const(primIndex), Const(2));
                    Operand w = this.Load(StorageKind.Input, IoVariable.Position, Const(primIndex), Const(3));

                    this.Store(StorageKind.Output, IoVariable.Position, null, Const(0), x);
                    this.Store(StorageKind.Output, IoVariable.Position, null, Const(1), y);
                    this.Store(StorageKind.Output, IoVariable.Position, null, Const(2), z);
                    this.Store(StorageKind.Output, IoVariable.Position, null, Const(3), w);
                }

                void WriteUserDefinedOutput(int index, int primIndex)
                {
                    Operand x = this.Load(StorageKind.Input, IoVariable.UserDefined, Const(index), Const(primIndex), Const(0));
                    Operand y = this.Load(StorageKind.Input, IoVariable.UserDefined, Const(index), Const(primIndex), Const(1));
                    Operand z = this.Load(StorageKind.Input, IoVariable.UserDefined, Const(index), Const(primIndex), Const(2));
                    Operand w = this.Load(StorageKind.Input, IoVariable.UserDefined, Const(index), Const(primIndex), Const(3));

                    this.Store(StorageKind.Output, IoVariable.UserDefined, null, Const(index), Const(0), x);
                    this.Store(StorageKind.Output, IoVariable.UserDefined, null, Const(index), Const(1), y);
                    this.Store(StorageKind.Output, IoVariable.UserDefined, null, Const(index), Const(2), z);
                    this.Store(StorageKind.Output, IoVariable.UserDefined, null, Const(index), Const(3), w);
                }

                if (Config.GpPassthrough && !Config.GpuAccessor.QueryHostSupportsGeometryShaderPassthrough())
                {
                    int inputVertices = Config.GpuAccessor.QueryPrimitiveTopology().ToInputVertices();

                    for (int primIndex = 0; primIndex < inputVertices; primIndex++)
                    {
                        WritePositionOutput(primIndex);

                        int passthroughAttributes = Config.PassthroughAttributes;
                        while (passthroughAttributes != 0)
                        {
                            int index = BitOperations.TrailingZeroCount(passthroughAttributes);
                            WriteUserDefinedOutput(index, primIndex);
                            Config.SetOutputUserAttribute(index);
                            passthroughAttributes &= ~(1 << index);
                        }

                        this.EmitVertex();
                    }

                    this.EndPrimitive();
                }
            }
            else if (Config.Stage == ShaderStage.Fragment)
            {
                GenerateAlphaToCoverageDitherDiscard();

                bool supportsBgra = Config.GpuAccessor.QueryHostSupportsBgraFormat();

                if (Config.OmapDepth)
                {
                    Operand src = Register(Config.GetDepthRegister(), RegisterType.Gpr);

                    this.Store(StorageKind.Output, IoVariable.FragmentOutputDepth, null, src);
                }

                AlphaTestOp alphaTestOp = Config.GpuAccessor.QueryAlphaTestCompare();

                if (alphaTestOp != AlphaTestOp.Always && (Config.OmapTargets & 8) != 0)
                {
                    if (alphaTestOp == AlphaTestOp.Never)
                    {
                        this.Discard();
                    }
                    else
                    {
                        Instruction comparator = alphaTestOp switch
                        {
                            AlphaTestOp.Equal => Instruction.CompareEqual,
                            AlphaTestOp.Greater => Instruction.CompareGreater,
                            AlphaTestOp.GreaterOrEqual => Instruction.CompareGreaterOrEqual,
                            AlphaTestOp.Less => Instruction.CompareLess,
                            AlphaTestOp.LessOrEqual => Instruction.CompareLessOrEqual,
                            AlphaTestOp.NotEqual => Instruction.CompareNotEqual,
                            _ => 0,
                        };

                        Debug.Assert(comparator != 0, $"Invalid alpha test operation \"{alphaTestOp}\".");

                        Operand alpha = Register(3, RegisterType.Gpr);
                        Operand alphaRef = ConstF(Config.GpuAccessor.QueryAlphaTestReference());
                        Operand alphaPass = Add(Instruction.FP32 | comparator, Local(), alpha, alphaRef);
                        Operand alphaPassLabel = Label();

                        this.BranchIfTrue(alphaPassLabel, alphaPass);
                        this.Discard();
                        this.MarkLabel(alphaPassLabel);
                    }
                }

                int regIndexBase = 0;

                for (int rtIndex = 0; rtIndex < 8; rtIndex++)
                {
                    for (int component = 0; component < 4; component++)
                    {
                        bool componentEnabled = (Config.OmapTargets & (1 << (rtIndex * 4 + component))) != 0;
                        if (!componentEnabled)
                        {
                            continue;
                        }

                        Operand src = Register(regIndexBase + component, RegisterType.Gpr);

                        // Perform B <-> R swap if needed, for BGRA formats (not supported on OpenGL).
                        if (!supportsBgra && (component == 0 || component == 2))
                        {
                            Operand isBgra = this.Load(StorageKind.ConstantBuffer, SupportBuffer.Binding, Const((int)SupportBufferField.FragmentIsBgra), Const(rtIndex));

                            Operand lblIsBgra = Label();
                            Operand lblEnd = Label();

                            this.BranchIfTrue(lblIsBgra, isBgra);

                            this.Store(StorageKind.Output, IoVariable.FragmentOutputColor, null, Const(rtIndex), Const(component), src);
                            this.Branch(lblEnd);

                            MarkLabel(lblIsBgra);

                            this.Store(StorageKind.Output, IoVariable.FragmentOutputColor, null, Const(rtIndex), Const(2 - component), src);

                            MarkLabel(lblEnd);
                        }
                        else
                        {
                            this.Store(StorageKind.Output, IoVariable.FragmentOutputColor, null, Const(rtIndex), Const(component), src);
                        }
                    }

                    bool targetEnabled = (Config.OmapTargets & (0xf << (rtIndex * 4))) != 0;
                    if (targetEnabled)
                    {
                        Config.SetOutputUserAttribute(rtIndex);
                        regIndexBase += 4;
                    }
                }
            }
        }

        private void GenerateAlphaToCoverageDitherDiscard()
        {
            // If the feature is disabled, or alpha is not written, then we're done.
            if (!Config.GpuAccessor.QueryAlphaToCoverageDitherEnable() || (Config.OmapTargets & 8) == 0)
            {
                return;
            }

            // 11 11 11 10 10 10 10 00
            // 11 01 01 01 01 00 00 00
            Operand ditherMask = Const(unchecked((int)0xfbb99110u));

            Operand fragCoordX = this.Load(StorageKind.Input, IoVariable.FragmentCoord, null, Const(0));
            Operand fragCoordY = this.Load(StorageKind.Input, IoVariable.FragmentCoord, null, Const(1));

            Operand x = this.BitwiseAnd(this.FP32ConvertToU32(fragCoordX), Const(1));
            Operand y = this.BitwiseAnd(this.FP32ConvertToU32(fragCoordY), Const(1));
            Operand xy = this.BitwiseOr(x, this.ShiftLeft(y, Const(1)));

            Operand alpha = Register(3, RegisterType.Gpr);
            Operand scaledAlpha = this.FPMultiply(this.FPSaturate(alpha), ConstF(8));
            Operand quantizedAlpha = this.IMinimumU32(this.FP32ConvertToU32(scaledAlpha), Const(7));
            Operand shift = this.BitwiseOr(this.ShiftLeft(quantizedAlpha, Const(2)), xy);
            Operand opaque = this.BitwiseAnd(this.ShiftRightU32(ditherMask, shift), Const(1));

            Operand a2cDitherEndLabel = Label();

            this.BranchIfTrue(a2cDitherEndLabel, opaque);
            this.Discard();
            this.MarkLabel(a2cDitherEndLabel);
        }

        public Operation[] GetOperations()
        {
            return _operations.ToArray();
        }
    }
}
