using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class OperandInfo
    {
        public static VariableType GetVarType(AstOperand operand)
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

        public static VariableType GetVarType(OperandType type)
        {
            switch (type)
            {
                case OperandType.Attribute:      return VariableType.F32;
                case OperandType.Constant:       return VariableType.S32;
                case OperandType.ConstantBuffer: return VariableType.F32;
                case OperandType.Undefined:      return VariableType.S32;
            }

            throw new ArgumentException($"Invalid operand type \"{type}\".");
        }
    }
}