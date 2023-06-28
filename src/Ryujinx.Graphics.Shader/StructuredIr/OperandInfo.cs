using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class OperandInfo
    {
        public static AggregateType GetVarType(AstOperand operand)
        {
            if (operand.Type == OperandType.LocalVariable)
            {
                return operand.VarType;
            }
            else
            {
                return GetVarType(operand.Type);
            }
        }

        public static AggregateType GetVarType(OperandType type)
        {
            return type switch
            {
                OperandType.Argument => AggregateType.S32,
                OperandType.Constant => AggregateType.S32,
                OperandType.Undefined => AggregateType.S32,
                _ => throw new ArgumentException($"Invalid operand type \"{type}\"."),
            };
        }
    }
}
