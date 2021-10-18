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
            return type switch
            {
                OperandType.Attribute => VariableType.F32,
                OperandType.AttributePerPatch => VariableType.F32,
                OperandType.Constant => VariableType.S32,
                OperandType.ConstantBuffer => VariableType.F32,
                OperandType.Undefined => VariableType.S32,
                _ => throw new ArgumentException($"Invalid operand type \"{type}\".")
            };
        }
    }
}