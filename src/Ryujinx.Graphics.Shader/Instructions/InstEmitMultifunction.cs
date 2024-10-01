using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void RroR(EmitterContext context)
        {
            InstRroR op = context.GetOp<InstRroR>();

            EmitRro(context, GetSrcReg(context, op.SrcB), op.Dest, op.AbsB, op.NegB);
        }

        public static void RroI(EmitterContext context)
        {
            InstRroI op = context.GetOp<InstRroI>();

            EmitRro(context, GetSrcImm(context, Imm20ToFloat(op.Imm20)), op.Dest, op.AbsB, op.NegB);
        }

        public static void RroC(EmitterContext context)
        {
            InstRroC op = context.GetOp<InstRroC>();

            EmitRro(context, GetSrcCbuf(context, op.CbufSlot, op.CbufOffset), op.Dest, op.AbsB, op.NegB);
        }

        public static void Mufu(EmitterContext context)
        {
            InstMufu op = context.GetOp<InstMufu>();

            Operand res = context.FPAbsNeg(GetSrcReg(context, op.SrcA), op.AbsA, op.NegA);

            switch (op.MufuOp)
            {
                case MufuOp.Cos:
                    res = context.FPCosine(res);
                    break;

                case MufuOp.Sin:
                    res = context.FPSine(res);
                    break;

                case MufuOp.Ex2:
                    res = context.FPExponentB2(res);
                    break;

                case MufuOp.Lg2:
                    res = context.FPLogarithmB2(res);
                    break;

                case MufuOp.Rcp:
                    res = context.FPReciprocal(res);
                    break;

                case MufuOp.Rsq:
                    res = context.FPReciprocalSquareRoot(res);
                    break;

                case MufuOp.Rcp64h:
                    res = context.PackDouble2x32(OperandHelper.Const(0), res);
                    res = context.UnpackDouble2x32High(context.FPReciprocal(res, Instruction.FP64));
                    break;

                case MufuOp.Rsq64h:
                    res = context.PackDouble2x32(OperandHelper.Const(0), res);
                    res = context.UnpackDouble2x32High(context.FPReciprocalSquareRoot(res, Instruction.FP64));
                    break;

                case MufuOp.Sqrt:
                    res = context.FPSquareRoot(res);
                    break;

                default:
                    context.TranslatorContext.GpuAccessor.Log($"Invalid MUFU operation \"{op.MufuOp}\".");
                    break;
            }

            context.Copy(GetDest(op.Dest), context.FPSaturate(res, op.Sat));
        }

        private static void EmitRro(EmitterContext context, Operand srcB, int rd, bool absB, bool negB)
        {
            // This is the range reduction operator,
            // we translate it as a simple move, as it
            // should be always followed by a matching
            // MUFU instruction.
            srcB = context.FPAbsNeg(srcB, absB, negB);

            context.Copy(GetDest(rd), srcB);
        }
    }
}
