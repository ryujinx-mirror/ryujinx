using Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    static class TypeConversion
    {
        public static string ReinterpretCast(
            CodeGenContext context,
            IAstNode       node,
            VariableType   srcType,
            VariableType   dstType)
        {
            if (node is AstOperand operand && operand.Type == OperandType.Constant)
            {
                if (NumberFormatter.TryFormat(operand.Value, dstType, out string formatted))
                {
                    return formatted;
                }
            }

            string expr = InstGen.GetExpression(context, node);

            return ReinterpretCast(expr, node, srcType, dstType);
        }

        private static string ReinterpretCast(string expr, IAstNode node, VariableType srcType, VariableType dstType)
        {
            if (srcType == dstType)
            {
                return expr;
            }

            if (srcType == VariableType.F32)
            {
                switch (dstType)
                {
                    case VariableType.Bool: return $"(floatBitsToInt({expr}) != 0)";
                    case VariableType.S32:  return $"floatBitsToInt({expr})";
                    case VariableType.U32:  return $"floatBitsToUint({expr})";
                }
            }
            else if (dstType == VariableType.F32)
            {
                switch (srcType)
                {
                    case VariableType.Bool: return $"intBitsToFloat({ReinterpretBoolToInt(expr, node, VariableType.S32)})";
                    case VariableType.S32:  return $"intBitsToFloat({expr})";
                    case VariableType.U32:  return $"uintBitsToFloat({expr})";
                }
            }
            else if (srcType == VariableType.Bool)
            {
                return ReinterpretBoolToInt(expr, node, dstType);
            }
            else if (dstType == VariableType.Bool)
            {
                expr = InstGenHelper.Enclose(expr, node, Instruction.CompareNotEqual, isLhs: true);

                return $"({expr} != 0)";
            }
            else if (dstType == VariableType.S32)
            {
                return $"int({expr})";
            }
            else if (dstType == VariableType.U32)
            {
                return $"uint({expr})";
            }

            throw new ArgumentException($"Invalid reinterpret cast from \"{srcType}\" to \"{dstType}\".");
        }

        private static string ReinterpretBoolToInt(string expr, IAstNode node, VariableType dstType)
        {
            string trueExpr  = NumberFormatter.FormatInt(IrConsts.True,  dstType);
            string falseExpr = NumberFormatter.FormatInt(IrConsts.False, dstType);

            expr = InstGenHelper.Enclose(expr, node, Instruction.ConditionalSelect, isLhs: false);

            return $"({expr} ? {trueExpr} : {falseExpr})";
        }
    }
}