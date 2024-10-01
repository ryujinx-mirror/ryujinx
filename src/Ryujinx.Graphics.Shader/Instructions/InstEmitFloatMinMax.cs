using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitAluHelper;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void DmnmxR(EmitterContext context)
        {
            InstDmnmxR op = context.GetOp<InstDmnmxR>();

            var srcA = GetSrcReg(context, op.SrcA, isFP64: true);
            var srcB = GetSrcReg(context, op.SrcB, isFP64: true);
            var srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitFmnmx(context, srcA, srcB, srcPred, op.Dest, op.AbsA, op.AbsB, op.NegA, op.NegB, op.WriteCC, isFP64: true);
        }

        public static void DmnmxI(EmitterContext context)
        {
            InstDmnmxI op = context.GetOp<InstDmnmxI>();

            var srcA = GetSrcReg(context, op.SrcA, isFP64: true);
            var srcB = GetSrcImm(context, Imm20ToFloat(op.Imm20), isFP64: true);
            var srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitFmnmx(context, srcA, srcB, srcPred, op.Dest, op.AbsA, op.AbsB, op.NegA, op.NegB, op.WriteCC, isFP64: true);
        }

        public static void DmnmxC(EmitterContext context)
        {
            InstDmnmxC op = context.GetOp<InstDmnmxC>();

            var srcA = GetSrcReg(context, op.SrcA, isFP64: true);
            var srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset, isFP64: true);
            var srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitFmnmx(context, srcA, srcB, srcPred, op.Dest, op.AbsA, op.AbsB, op.NegA, op.NegB, op.WriteCC, isFP64: true);
        }

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
            bool writeCC,
            bool isFP64 = false)
        {
            Instruction fpType = isFP64 ? Instruction.FP64 : Instruction.FP32;

            srcA = context.FPAbsNeg(srcA, absoluteA, negateA, fpType);
            srcB = context.FPAbsNeg(srcB, absoluteB, negateB, fpType);

            Operand resMin = context.FPMinimum(srcA, srcB, fpType);
            Operand resMax = context.FPMaximum(srcA, srcB, fpType);

            Operand res = context.ConditionalSelect(srcPred, resMin, resMax);

            SetDest(context, res, rd, isFP64);

            SetFPZnFlags(context, res, writeCC, fpType);
        }
    }
}
