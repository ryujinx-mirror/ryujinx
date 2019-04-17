using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstOperand : AstNode
    {
        public HashSet<IAstNode> Defs { get; }
        public HashSet<IAstNode> Uses { get; }

        public OperandType Type { get; }

        public VariableType VarType { get; set; }

        public int Value { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        private AstOperand()
        {
            Defs = new HashSet<IAstNode>();
            Uses = new HashSet<IAstNode>();

            VarType = VariableType.S32;
        }

        public AstOperand(Operand operand) : this()
        {
            Type = operand.Type;

            if (Type == OperandType.ConstantBuffer)
            {
                CbufSlot   = operand.GetCbufSlot();
                CbufOffset = operand.GetCbufOffset();
            }
            else
            {
                Value = operand.Value;
            }
        }

        public AstOperand(OperandType type, int value = 0)  : this()
        {
            Type  = type;
            Value = value;
        }
    }
}