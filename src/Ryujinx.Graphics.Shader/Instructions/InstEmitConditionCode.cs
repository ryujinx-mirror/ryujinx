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
        public static void Cset(EmitterContext context)
        {
            InstCset op = context.GetOp<InstCset>();

            Operand res = GetCondition(context, op.Ccc);
            Operand srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            res = GetPredLogicalOp(context, op.Bop, res, srcPred);

            Operand dest = GetDest(op.Dest);

            if (op.BVal)
            {
                context.Copy(dest, context.ConditionalSelect(res, ConstF(1), Const(0)));
            }
            else
            {
                context.Copy(dest, res);
            }

            // TODO: CC.
        }

        public static void Csetp(EmitterContext context)
        {
            InstCsetp op = context.GetOp<InstCsetp>();

            Operand p0Res = GetCondition(context, op.Ccc);
            Operand p1Res = context.BitwiseNot(p0Res);
            Operand srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            p0Res = GetPredLogicalOp(context, op.Bop, p0Res, srcPred);
            p1Res = GetPredLogicalOp(context, op.Bop, p1Res, srcPred);

            context.Copy(Register(op.DestPred, RegisterType.Predicate), p0Res);
            context.Copy(Register(op.DestPredInv, RegisterType.Predicate), p1Res);

            // TODO: CC.
        }

        private static Operand GetCondition(EmitterContext context, Ccc cond, int defaultCond = IrConsts.True)
        {
            return cond switch
            {
                Ccc.F => Const(IrConsts.False),
                Ccc.Lt => context.BitwiseExclusiveOr(context.BitwiseAnd(GetNF(), context.BitwiseNot(GetZF())), GetVF()),
                Ccc.Eq => context.BitwiseAnd(context.BitwiseNot(GetNF()), GetZF()),
                Ccc.Le => context.BitwiseExclusiveOr(GetNF(), context.BitwiseOr(GetZF(), GetVF())),
                Ccc.Gt => context.BitwiseNot(context.BitwiseOr(context.BitwiseExclusiveOr(GetNF(), GetVF()), GetZF())),
                Ccc.Ne => context.BitwiseNot(GetZF()),
                Ccc.Ge => context.BitwiseNot(context.BitwiseExclusiveOr(GetNF(), GetVF())),
                Ccc.Num => context.BitwiseNot(context.BitwiseAnd(GetNF(), GetZF())),
                Ccc.Nan => context.BitwiseAnd(GetNF(), GetZF()),
                Ccc.Ltu => context.BitwiseExclusiveOr(GetNF(), GetVF()),
                Ccc.Equ => GetZF(),
                Ccc.Leu => context.BitwiseOr(context.BitwiseExclusiveOr(GetNF(), GetVF()), GetZF()),
                Ccc.Gtu => context.BitwiseExclusiveOr(context.BitwiseNot(GetNF()), context.BitwiseOr(GetVF(), GetZF())),
                Ccc.Neu => context.BitwiseOr(GetNF(), context.BitwiseNot(GetZF())),
                Ccc.Geu => context.BitwiseExclusiveOr(context.BitwiseOr(context.BitwiseNot(GetNF()), GetZF()), GetVF()),
                Ccc.T => Const(IrConsts.True),
                Ccc.Off => context.BitwiseNot(GetVF()),
                Ccc.Lo => context.BitwiseNot(GetCF()),
                Ccc.Sff => context.BitwiseNot(GetNF()),
                Ccc.Ls => context.BitwiseOr(GetZF(), context.BitwiseNot(GetCF())),
                Ccc.Hi => context.BitwiseAnd(GetCF(), context.BitwiseNot(GetZF())),
                Ccc.Sft => GetNF(),
                Ccc.Hs => GetCF(),
                Ccc.Oft => GetVF(),
                Ccc.Rle => context.BitwiseOr(GetNF(), GetZF()),
                Ccc.Rgt => context.BitwiseNot(context.BitwiseOr(GetNF(), GetZF())),
                _ => Const(defaultCond),
            };
        }
    }
}
