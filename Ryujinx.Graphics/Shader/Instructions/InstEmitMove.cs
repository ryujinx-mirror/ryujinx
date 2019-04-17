using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Mov(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            context.Copy(GetDest(context), GetSrcB(context));
        }

        public static void Sel(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand pred = GetPredicate39(context);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand res = context.ConditionalSelect(pred, srcA, srcB);

            context.Copy(GetDest(context), res);
        }
    }
}