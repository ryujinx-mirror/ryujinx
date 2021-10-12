using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void ShlR(EmitterContext context)
        {
            InstShlR op = context.GetOp<InstShlR>();

            EmitShl(context, GetSrcReg(context, op.SrcA), GetSrcReg(context, op.SrcB), op.Dest, op.M);
        }

        public static void ShlI(EmitterContext context)
        {
            InstShlI op = context.GetOp<InstShlI>();

            EmitShl(context, GetSrcReg(context, op.SrcA), GetSrcImm(context, Imm20ToSInt(op.Imm20)), op.Dest, op.M);
        }

        public static void ShlC(EmitterContext context)
        {
            InstShlC op = context.GetOp<InstShlC>();

            EmitShl(context, GetSrcReg(context, op.SrcA), GetSrcCbuf(context, op.CbufSlot, op.CbufOffset), op.Dest, op.M);
        }

        public static void ShrR(EmitterContext context)
        {
            InstShrR op = context.GetOp<InstShrR>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcReg(context, op.SrcB);

            EmitShr(context, srcA, srcB, op.Dest, op.M, op.Brev, op.Signed);
        }

        public static void ShrI(EmitterContext context)
        {
            InstShrI op = context.GetOp<InstShrI>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcImm(context, Imm20ToSInt(op.Imm20));

            EmitShr(context, srcA, srcB, op.Dest, op.M, op.Brev, op.Signed);
        }

        public static void ShrC(EmitterContext context)
        {
            InstShrC op = context.GetOp<InstShrC>();

            var srcA = GetSrcReg(context, op.SrcA);
            var srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);

            EmitShr(context, srcA, srcB, op.Dest, op.M, op.Brev, op.Signed);
        }

        private static void EmitShl(EmitterContext context, Operand srcA, Operand srcB, int rd, bool mask)
        {
            if (mask)
            {
                srcB = context.BitwiseAnd(srcB, Const(0x1f));
            }

            Operand res = context.ShiftLeft(srcA, srcB);

            if (!mask)
            {
                // Clamped shift value.
                Operand isLessThan32 = context.ICompareLessUnsigned(srcB, Const(32));

                res = context.ConditionalSelect(isLessThan32, res, Const(0));
            }

            // TODO: X, CC.

            context.Copy(GetDest(rd), res);
        }

        private static void EmitShr(
            EmitterContext context,
            Operand srcA,
            Operand srcB,
            int rd,
            bool mask,
            bool bitReverse,
            bool isSigned)
        {
            if (bitReverse)
            {
                srcA = context.BitfieldReverse(srcA);
            }

            if (mask)
            {
                srcB = context.BitwiseAnd(srcB, Const(0x1f));
            }

            Operand res = isSigned
                ? context.ShiftRightS32(srcA, srcB)
                : context.ShiftRightU32(srcA, srcB);

            if (!mask)
            {
                // Clamped shift value.
                Operand resShiftBy32;

                if (isSigned)
                {
                    resShiftBy32 = context.ShiftRightS32(srcA, Const(31));
                }
                else
                {
                    resShiftBy32 = Const(0);
                }

                Operand isLessThan32 = context.ICompareLessUnsigned(srcB, Const(32));

                res = context.ConditionalSelect(isLessThan32, res, resShiftBy32);
            }

            // TODO: X, CC.

            context.Copy(GetDest(rd), res);
        }
    }
}