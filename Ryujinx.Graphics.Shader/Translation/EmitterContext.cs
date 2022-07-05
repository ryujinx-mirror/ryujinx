using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly List<Operation> _operations;
        private readonly Dictionary<ulong, Operand> _labels;

        public EmitterContext(DecodedProgram program, ShaderConfig config, bool isNonMain)
        {
            Program = program;
            Config = config;
            IsNonMain = isNonMain;
            _operations = new List<Operation>();
            _labels = new Dictionary<ulong, Operand>();
        }

        public T GetOp<T>() where T : unmanaged
        {
            Debug.Assert(Unsafe.SizeOf<T>() == sizeof(ulong));
            ulong op = CurrOp.RawOpCode;
            return Unsafe.As<ulong, T>(ref op);
        }

        public Operand Add(Instruction inst, Operand dest = null, params Operand[] sources)
        {
            Operation operation = new Operation(inst, dest, sources);

            Add(operation);

            return dest;
        }

        public (Operand, Operand) Add(Instruction inst, (Operand, Operand) dest, params Operand[] sources)
        {
            Operand[] dests = new[] { dest.Item1, dest.Item2 };

            Operation operation = new Operation(inst, 0, dests, sources);

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
            Operand dest,
            params Operand[] sources)
        {
            return CreateTextureOperation(inst, type, TextureFormat.Unknown, flags, handle, compIndex, dest, sources);
        }

        public TextureOperation CreateTextureOperation(
            Instruction inst,
            SamplerType type,
            TextureFormat format,
            TextureFlags flags,
            int handle,
            int compIndex,
            Operand dest,
            params Operand[] sources)
        {
            if (!flags.HasFlag(TextureFlags.Bindless))
            {
                Config.SetUsedTexture(inst, type, format, flags, TextureOperation.DefaultCbufSlot, handle);
            }

            return new TextureOperation(inst, type, format, flags, handle, compIndex, dest, sources);
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
            if (!_labels.TryGetValue(address, out Operand label))
            {
                label = Label();

                _labels.Add(address, label);
            }

            return label;
        }

        public void PrepareForVertexReturn()
        {
            if (Config.GpuAccessor.QueryViewportTransformDisable())
            {
                Operand x = Attribute(AttributeConsts.PositionX | AttributeConsts.LoadOutputMask);
                Operand y = Attribute(AttributeConsts.PositionY | AttributeConsts.LoadOutputMask);
                Operand xScale = Attribute(AttributeConsts.SupportBlockViewInverseX);
                Operand yScale = Attribute(AttributeConsts.SupportBlockViewInverseY);
                Operand negativeOne = ConstF(-1.0f);

                this.Copy(Attribute(AttributeConsts.PositionX), this.FPFusedMultiplyAdd(x, xScale, negativeOne));
                this.Copy(Attribute(AttributeConsts.PositionY), this.FPFusedMultiplyAdd(y, yScale, negativeOne));
            }
        }

        public void PrepareForVertexReturn(out Operand oldXLocal, out Operand oldYLocal, out Operand oldZLocal)
        {
            if (Config.GpuAccessor.QueryViewportTransformDisable())
            {
                oldXLocal = Local();
                this.Copy(oldXLocal, Attribute(AttributeConsts.PositionX | AttributeConsts.LoadOutputMask));
                oldYLocal = Local();
                this.Copy(oldYLocal, Attribute(AttributeConsts.PositionY | AttributeConsts.LoadOutputMask));
            }
            else
            {
                oldXLocal = null;
                oldYLocal = null;
            }

            // Will be used by Vulkan backend for depth mode emulation.
            oldZLocal = null;

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
            else if (Config.Stage == ShaderStage.Fragment)
            {
                GenerateAlphaToCoverageDitherDiscard();

                if (Config.OmapDepth)
                {
                    Operand dest = Attribute(AttributeConsts.FragmentOutputDepth);

                    Operand src = Register(Config.GetDepthRegister(), RegisterType.Gpr);

                    this.Copy(dest, src);
                }

                bool supportsBgra = Config.GpuAccessor.QueryHostSupportsBgraFormat();
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

                        int fragmentOutputColorAttr = AttributeConsts.FragmentOutputColorBase + rtIndex * 16;

                        Operand src = Register(regIndexBase + component, RegisterType.Gpr);

                        // Perform B <-> R swap if needed, for BGRA formats (not supported on OpenGL).
                        if (!supportsBgra && (component == 0 || component == 2))
                        {
                            Operand isBgra = Attribute(AttributeConsts.FragmentOutputIsBgraBase + rtIndex * 4);

                            Operand lblIsBgra = Label();
                            Operand lblEnd = Label();

                            this.BranchIfTrue(lblIsBgra, isBgra);

                            this.Copy(Attribute(fragmentOutputColorAttr + component * 4), src);
                            this.Branch(lblEnd);

                            MarkLabel(lblIsBgra);

                            this.Copy(Attribute(fragmentOutputColorAttr + (2 - component) * 4), src);

                            MarkLabel(lblEnd);
                        }
                        else
                        {
                            this.Copy(Attribute(fragmentOutputColorAttr + component * 4), src);
                        }
                    }

                    bool targetEnabled = (Config.OmapTargets & (0xf << (rtIndex * 4))) != 0;
                    if (targetEnabled)
                    {
                        Config.SetOutputUserAttribute(rtIndex, perPatch: false);
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

            Operand x = this.BitwiseAnd(this.FP32ConvertToU32(Attribute(AttributeConsts.PositionX)), Const(1));
            Operand y = this.BitwiseAnd(this.FP32ConvertToU32(Attribute(AttributeConsts.PositionY)), Const(1));
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