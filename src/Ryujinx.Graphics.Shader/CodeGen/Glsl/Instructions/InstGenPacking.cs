using Ryujinx.Graphics.Shader.StructuredIr;
using System;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions.InstGenHelper;
using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenPacking
    {
        public static string PackDouble2x32(CodeGenContext context, AstOperation operation)
        {
            IAstNode src0 = operation.GetSource(0);
            IAstNode src1 = operation.GetSource(1);

            string src0Expr = GetSourceExpr(context, src0, GetSrcVarType(operation.Inst, 0));
            string src1Expr = GetSourceExpr(context, src1, GetSrcVarType(operation.Inst, 1));

            return $"packDouble2x32(uvec2({src0Expr}, {src1Expr}))";
        }

        public static string PackHalf2x16(CodeGenContext context, AstOperation operation)
        {
            IAstNode src0 = operation.GetSource(0);
            IAstNode src1 = operation.GetSource(1);

            string src0Expr = GetSourceExpr(context, src0, GetSrcVarType(operation.Inst, 0));
            string src1Expr = GetSourceExpr(context, src1, GetSrcVarType(operation.Inst, 1));

            return $"packHalf2x16(vec2({src0Expr}, {src1Expr}))";
        }

        public static string UnpackDouble2x32(CodeGenContext context, AstOperation operation)
        {
            IAstNode src = operation.GetSource(0);

            string srcExpr = GetSourceExpr(context, src, GetSrcVarType(operation.Inst, 0));

            return $"unpackDouble2x32({srcExpr}){GetMask(operation.Index)}";
        }

        public static string UnpackHalf2x16(CodeGenContext context, AstOperation operation)
        {
            IAstNode src = operation.GetSource(0);

            string srcExpr = GetSourceExpr(context, src, GetSrcVarType(operation.Inst, 0));

            return $"unpackHalf2x16({srcExpr}){GetMask(operation.Index)}";
        }

        private static string GetMask(int index)
        {
            return $".{"xy".AsSpan(index, 1)}";
        }
    }
}
