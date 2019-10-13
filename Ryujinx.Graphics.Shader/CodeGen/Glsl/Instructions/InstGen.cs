using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using System;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions.InstGenHelper;
using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGen
    {
        public static string GetExpression(CodeGenContext context, IAstNode node)
        {
            if (node is AstOperation operation)
            {
                return GetExpression(context, operation);
            }
            else if (node is AstOperand operand)
            {
                return context.OperandManager.GetExpression(operand, context.Config.Stage);
            }

            throw new ArgumentException($"Invalid node type \"{node?.GetType().Name ?? "null"}\".");
        }

        private static string GetExpression(CodeGenContext context, AstOperation operation)
        {
            Instruction inst = operation.Inst;

            InstInfo info = GetInstructionInfo(inst);

            if ((info.Type & InstType.Call) != 0)
            {
                int arity = (int)(info.Type & InstType.ArityMask);

                string args = string.Empty;

                for (int argIndex = 0; argIndex < arity; argIndex++)
                {
                    if (argIndex != 0)
                    {
                        args += ", ";
                    }

                    VariableType dstType = GetSrcVarType(inst, argIndex);

                    args += GetSoureExpr(context, operation.GetSource(argIndex), dstType);
                }

                return info.OpName + "(" + args + ")";
            }
            else if ((info.Type & InstType.Op) != 0)
            {
                string op = info.OpName;

                int arity = (int)(info.Type & InstType.ArityMask);

                string[] expr = new string[arity];

                for (int index = 0; index < arity; index++)
                {
                    IAstNode src = operation.GetSource(index);

                    string srcExpr = GetSoureExpr(context, src, GetSrcVarType(inst, index));

                    bool isLhs = arity == 2 && index == 0;

                    expr[index] = Enclose(srcExpr, src, inst, info, isLhs);
                }

                switch (arity)
                {
                    case 0:
                        return op;

                    case 1:
                        return op + expr[0];

                    case 2:
                        return $"{expr[0]} {op} {expr[1]}";

                    case 3:
                        return $"{expr[0]} {op[0]} {expr[1]} {op[1]} {expr[2]}";
                }
            }
            else if ((info.Type & InstType.Special) != 0)
            {
                switch (inst)
                {
                    case Instruction.LoadAttribute:
                        return InstGenMemory.LoadAttribute(context, operation);

                    case Instruction.LoadConstant:
                        return InstGenMemory.LoadConstant(context, operation);

                    case Instruction.LoadLocal:
                        return InstGenMemory.LoadLocal(context, operation);

                    case Instruction.LoadStorage:
                        return InstGenMemory.LoadStorage(context, operation);

                    case Instruction.PackHalf2x16:
                        return InstGenPacking.PackHalf2x16(context, operation);

                    case Instruction.StoreLocal:
                        return InstGenMemory.StoreLocal(context, operation);

                    case Instruction.StoreStorage:
                        return InstGenMemory.StoreStorage(context, operation);

                    case Instruction.TextureSample:
                        return InstGenMemory.TextureSample(context, operation);

                    case Instruction.TextureSize:
                        return InstGenMemory.TextureSize(context, operation);

                    case Instruction.UnpackHalf2x16:
                        return InstGenPacking.UnpackHalf2x16(context, operation);
                }
            }

            return "0";

            throw new InvalidOperationException($"Unexpected instruction type \"{info.Type}\".");
        }
    }
}