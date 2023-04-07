using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitAluHelper;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Pset(EmitterContext context)
        {
            InstPset op = context.GetOp<InstPset>();

            Operand srcA = context.BitwiseNot(Register(op.Src2Pred, RegisterType.Predicate), op.Src2PredInv);
            Operand srcB = context.BitwiseNot(Register(op.Src1Pred, RegisterType.Predicate), op.Src1PredInv);
            Operand srcC = context.BitwiseNot(Register(op.SrcPred, RegisterType.Predicate), op.SrcPredInv);

            Operand res = GetPredLogicalOp(context, op.BoolOpAB, srcA, srcB);
            res = GetPredLogicalOp(context, op.BoolOpC, res, srcC);

            Operand dest = GetDest(op.Dest);

            if (op.BVal)
            {
                context.Copy(dest, context.ConditionalSelect(res, ConstF(1), Const(0)));
            }
            else
            {
                context.Copy(dest, res);
            }
        }

        public static void Psetp(EmitterContext context)
        {
            InstPsetp op = context.GetOp<InstPsetp>();

            Operand srcA = context.BitwiseNot(Register(op.Src2Pred, RegisterType.Predicate), op.Src2PredInv);
            Operand srcB = context.BitwiseNot(Register(op.Src1Pred, RegisterType.Predicate), op.Src1PredInv);

            Operand p0Res = GetPredLogicalOp(context, op.BoolOpAB, srcA, srcB);
            Operand p1Res = context.BitwiseNot(p0Res);
            Operand srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            p0Res = GetPredLogicalOp(context, op.BoolOpC, p0Res, srcPred);
            p1Res = GetPredLogicalOp(context, op.BoolOpC, p1Res, srcPred);

            context.Copy(Register(op.DestPred, RegisterType.Predicate), p0Res);
            context.Copy(Register(op.DestPredInv, RegisterType.Predicate), p1Res);
        }
    }
}