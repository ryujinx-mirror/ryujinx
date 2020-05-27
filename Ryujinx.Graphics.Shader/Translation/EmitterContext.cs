using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    class EmitterContext
    {
        public Block  CurrBlock { get; set; }
        public OpCode CurrOp    { get; set; }

        public ShaderConfig Config { get; }

        private List<Operation> _operations;

        private Dictionary<ulong, Operand> _labels;

        public EmitterContext(ShaderConfig config)
        {
            Config = config;

            _operations = new List<Operation>();

            _labels = new Dictionary<ulong, Operand>();
        }

        public Operand Add(Instruction inst, Operand dest = null, params Operand[] sources)
        {
            Operation operation = new Operation(inst, dest, sources);

            Add(operation);

            return dest;
        }

        public void Add(Operation operation)
        {
            _operations.Add(operation);
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

        public void PrepareForReturn()
        {
            if (Config.Stage == ShaderStage.Vertex && (Config.Flags & TranslationFlags.VertexA) == 0)
            {
                // Here we attempt to implement viewport swizzle on the vertex shader.
                // Perform permutation and negation of the output gl_Position components.
                // Note that per-viewport swizzling can't be supported using this approach.
                int swizzleX = Config.GpuAccessor.QueryViewportSwizzle(0);
                int swizzleY = Config.GpuAccessor.QueryViewportSwizzle(1);
                int swizzleZ = Config.GpuAccessor.QueryViewportSwizzle(2);
                int swizzleW = Config.GpuAccessor.QueryViewportSwizzle(3);

                bool nonStandardSwizzle = swizzleX != 0 || swizzleY != 2 || swizzleZ != 4 || swizzleW != 6;

                if (!Config.GpuAccessor.QuerySupportsViewportSwizzle() && nonStandardSwizzle)
                {
                    Operand[] temp = new Operand[4];

                    temp[0] = this.Copy(Attribute(AttributeConsts.PositionX));
                    temp[1] = this.Copy(Attribute(AttributeConsts.PositionY));
                    temp[2] = this.Copy(Attribute(AttributeConsts.PositionZ));
                    temp[3] = this.Copy(Attribute(AttributeConsts.PositionW));

                    this.Copy(Attribute(AttributeConsts.PositionX), this.FPNegate(temp[(swizzleX >> 1) & 3], (swizzleX & 1) != 0));
                    this.Copy(Attribute(AttributeConsts.PositionY), this.FPNegate(temp[(swizzleY >> 1) & 3], (swizzleY & 1) != 0));
                    this.Copy(Attribute(AttributeConsts.PositionZ), this.FPNegate(temp[(swizzleZ >> 1) & 3], (swizzleZ & 1) != 0));
                    this.Copy(Attribute(AttributeConsts.PositionW), this.FPNegate(temp[(swizzleW >> 1) & 3], (swizzleW & 1) != 0));
                }
            }
            else if (Config.Stage == ShaderStage.Fragment)
            {
                if (Config.OmapDepth)
                {
                    Operand dest = Attribute(AttributeConsts.FragmentOutputDepth);

                    Operand src = Register(Config.GetDepthRegister(), RegisterType.Gpr);

                    this.Copy(dest, src);
                }

                int regIndex = 0;

                for (int attachment = 0; attachment < 8; attachment++)
                {
                    OmapTarget target = Config.OmapTargets[attachment];

                    for (int component = 0; component < 4; component++)
                    {
                        if (target.ComponentEnabled(component))
                        {
                            Operand dest = Attribute(AttributeConsts.FragmentOutputColorBase + attachment * 16 + component * 4);

                            Operand src = Register(regIndex, RegisterType.Gpr);

                            this.Copy(dest, src);

                            regIndex++;
                        }
                    }
                }
            }
        }

        public Operation[] GetOperations()
        {
            return _operations.ToArray();
        }
    }
}