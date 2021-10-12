using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitAluHelper;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void FmnmxR(EmitterContext context)
        {
            InstFmnmxR op = context.GetOp<InstFmnmxR>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcReg(context, op.SrcB);
            var srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitFmnmx(context, srcA, srcB, srcPred, op.Dest, op.AbsA, op.AbsB, op.NegA, op.NegB, op.WriteCC);
        }

        public static void FmnmxI(EmitterContext context)
        {
            InstFmnmxI op = context.GetOp<InstFmnmxI>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcImm(context, Imm20ToFloat(op.Imm20));
            var srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitFmnmx(context, srcA, srcB, srcPred, op.Dest, op.AbsA, op.AbsB, op.NegA, op.NegB, op.WriteCC);
        }

        public static void FmnmxC(EmitterContext context)
        {
            InstFmnmxC op = context.GetOp<InstFmnmxC>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);
            var srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitFmnmx(context, srcA, srcB, srcPred, op.Dest, op.AbsA, op.AbsB, op.NegA, op.NegB, op.WriteCC);
        }

        private static void EmitFmnmx(
            EmitterContext context,
            Operand srcA,
            Operand srcB,
            Operand srcPred,
            int rd,
            bool absoluteA,
            bool absoluteB,
            bool negateA,
            bool negateB,
            bool writeCC)
        {
            srcA = context.FPAbsNeg(srcA, absoluteA, negateA);
            srcB = context.FPAbsNeg(srcB, absoluteB, negateB);

            Operand resMin = context.FPMinimum(srcA, srcB);
            Operand resMax = context.FPMaximum(srcA, srcB);

            Operand dest = GetDest(rd);

            context.Copy(dest, context.ConditionalSelect(srcPred, resMin, resMax));

            SetFPZnFlags(context, dest, writeCC);
        }
    }
}