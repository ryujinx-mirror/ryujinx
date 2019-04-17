using Ryujinx.Graphics.Gal;
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

        private GalShaderType _shaderType;

        private ShaderHeader _header;

        private List<Operation> _operations;

        private Dictionary<ulong, Operand> _labels;

        public EmitterContext(GalShaderType shaderType, ShaderHeader header)
        {
            _shaderType = shaderType;
            _header     = header;

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
            if (_shaderType == GalShaderType.Fragment)
            {
                if (_header.OmapDepth)
                {
                    Operand dest = Attribute(AttributeConsts.FragmentOutputDepth);

                    Operand src = Register(_header.DepthRegister, RegisterType.Gpr);

                    this.Copy(dest, src);
                }

                int regIndex = 0;

                for (int attachment = 0; attachment < 8; attachment++)
                {
                    OutputMapTarget target = _header.OmapTargets[attachment];

                    for (int component = 0; component < 4; component++)
                    {
                        if (target.ComponentEnabled(component))
                        {
                            Operand dest = Attribute(AttributeConsts.FragmentOutputColorBase + regIndex * 4);

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