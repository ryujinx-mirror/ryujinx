using Ryujinx.Graphics.Shader.StructuredIr;

using static Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions.InstGenHelper;
using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class InstGenBallot
    {
        public static string Ballot(CodeGenContext context, AstOperation operation)
        {
            VariableType dstType = GetSrcVarType(operation.Inst, 0);

            string arg = GetSoureExpr(context, operation.GetSource(0), dstType);

            if (context.Config.GpuAccessor.QueryHostSupportsShaderBallot())
            {
                return $"unpackUint2x32(ballotARB({arg})).x";
            }
            else
            {
                return $"subgroupBallot({arg}).x";
            }
        }
    }
}