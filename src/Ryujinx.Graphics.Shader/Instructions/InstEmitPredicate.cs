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
                context.Copy(dest, context.ConditionalSelect(res, ConstF(1), ConstF(0)));
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

        public static void P2rC(EmitterContext context)
        {
            InstP2rC op = context.GetOp<InstP2rC>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand dest = GetSrcReg(context, op.Dest);
            Operand mask = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);

            EmitP2r(context, srcA, dest, mask, op.ByteSel, op.Ccpr);
        }

        public static void P2rI(EmitterContext context)
        {
            InstP2rI op = context.GetOp<InstP2rI>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand dest = GetSrcReg(context, op.Dest);
            Operand mask = GetSrcImm(context, op.Imm20);

            EmitP2r(context, srcA, dest, mask, op.ByteSel, op.Ccpr);
        }

        public static void P2rR(EmitterContext context)
        {
            InstP2rR op = context.GetOp<InstP2rR>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand dest = GetSrcReg(context, op.Dest);
            Operand mask = GetSrcReg(context, op.SrcB);

            EmitP2r(context, srcA, dest, mask, op.ByteSel, op.Ccpr);
        }

        private static void EmitP2r(
            EmitterContext context,
            Operand srcA,
            Operand dest,
            Operand mask,
            ByteSel byteSel,
            bool ccpr)
        {
            int count = ccpr ? RegisterConsts.FlagsCount : RegisterConsts.PredsCount;
            int shift = (int)byteSel * 8;
            mask = context.BitwiseAnd(mask, Const(0xff));

            Operand insert = Const(0);
            for (int i = 0; i < count; i++)
            {
                Operand condition = ccpr
                    ? Register(i, RegisterType.Flag)
                    : Register(i, RegisterType.Predicate);

                Operand bit = context.ConditionalSelect(condition, Const(1 << (i + shift)), Const(0));
                insert = context.BitwiseOr(insert, bit);
            }

            Operand maskShifted = context.ShiftLeft(mask, Const(shift));
            Operand masked = context.BitwiseAnd(srcA, context.BitwiseNot(maskShifted));
            Operand res = context.BitwiseOr(masked, context.BitwiseAnd(insert, maskShifted));

            context.Copy(dest, res);
        }
    }
}
