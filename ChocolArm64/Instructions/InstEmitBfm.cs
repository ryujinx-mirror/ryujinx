using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Bfm(ILEmitterCtx context)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            if (op.Pos < op.Shift)
            {
                // BFI.
                context.EmitLdintzr(op.Rn);

                int shift = op.GetBitsCount() - op.Shift;

                int width = op.Pos + 1;

                long mask = (long)(ulong.MaxValue >> (64 - width));

                context.EmitLdc_I(mask);

                context.Emit(OpCodes.And);

                context.EmitLsl(shift);

                context.EmitLdintzr(op.Rd);

                context.EmitLdc_I(~(mask << shift));

                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Or);

                context.EmitStintzr(op.Rd);
            }
            else
            {
                // BFXIL.
                context.EmitLdintzr(op.Rn);

                context.EmitLsr(op.Shift);

                int width = op.Pos - op.Shift + 1;

                long mask = (long)(ulong.MaxValue >> (64 - width));

                context.EmitLdc_I(mask);

                context.Emit(OpCodes.And);

                context.EmitLdintzr(op.Rd);

                context.EmitLdc_I(~mask);

                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Or);

                context.EmitStintzr(op.Rd);
            }
        }

        public static void Sbfm(ILEmitterCtx context)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            int bitsCount = op.GetBitsCount();

            if (op.Pos + 1 == bitsCount)
            {
                EmitSbfmShift(context);
            }
            else if (op.Pos < op.Shift)
            {
                EmitSbfiz(context);
            }
            else if (op.Pos == 7 && op.Shift == 0)
            {
                EmitSbfmCast(context, OpCodes.Conv_I1);
            }
            else if (op.Pos == 15 && op.Shift == 0)
            {
                EmitSbfmCast(context, OpCodes.Conv_I2);
            }
            else if (op.Pos == 31 && op.Shift == 0)
            {
                EmitSbfmCast(context, OpCodes.Conv_I4);
            }
            else
            {
                EmitBfmLoadRn(context);

                context.EmitLdintzr(op.Rn);

                context.EmitLsl(bitsCount - 1 - op.Pos);
                context.EmitAsr(bitsCount - 1);

                context.EmitLdc_I(~op.TMask);

                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Or);

                context.EmitStintzr(op.Rd);
            }
        }

        public static void Ubfm(ILEmitterCtx context)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            if (op.Pos + 1 == op.GetBitsCount())
            {
                EmitUbfmShift(context);
            }
            else if (op.Pos < op.Shift)
            {
                EmitUbfiz(context);
            }
            else if (op.Pos + 1 == op.Shift)
            {
                EmitBfmLsl(context);
            }
            else if (op.Pos == 7 && op.Shift == 0)
            {
                EmitUbfmCast(context, OpCodes.Conv_U1);
            }
            else if (op.Pos == 15 && op.Shift == 0)
            {
                EmitUbfmCast(context, OpCodes.Conv_U2);
            }
            else
            {
                EmitBfmLoadRn(context);

                context.EmitStintzr(op.Rd);
            }
        }

        private static void EmitSbfiz(ILEmitterCtx context) => EmitBfiz(context, true);
        private static void EmitUbfiz(ILEmitterCtx context) => EmitBfiz(context, false);

        private static void EmitBfiz(ILEmitterCtx context, bool signed)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            int width = op.Pos + 1;

            context.EmitLdintzr(op.Rn);

            context.EmitLsl(op.GetBitsCount() - width);

            if (signed)
            {
                context.EmitAsr(op.Shift - width);
            }
            else
            {
                context.EmitLsr(op.Shift - width);
            }

            context.EmitStintzr(op.Rd);
        }

        private static void EmitSbfmCast(ILEmitterCtx context, OpCode ilOp)
        {
            EmitBfmCast(context, ilOp, true);
        }

        private static void EmitUbfmCast(ILEmitterCtx context, OpCode ilOp)
        {
            EmitBfmCast(context, ilOp, false);
        }

        private static void EmitBfmCast(ILEmitterCtx context, OpCode ilOp, bool signed)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.Emit(ilOp);

            if (op.RegisterSize != RegisterSize.Int32)
            {
                context.Emit(signed
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);
            }

            context.EmitStintzr(op.Rd);
        }

        private static void EmitSbfmShift(ILEmitterCtx context)
        {
            EmitBfmShift(context, true);
        }

        private static void EmitUbfmShift(ILEmitterCtx context)
        {
            EmitBfmShift(context, false);
        }

        private static void EmitBfmShift(ILEmitterCtx context, bool signed)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            context.EmitLdintzr(op.Rn);
            context.EmitLdc_I4(op.Shift);

            context.Emit(signed
                ? OpCodes.Shr
                : OpCodes.Shr_Un);

            context.EmitStintzr(op.Rd);
        }

        private static void EmitBfmLsl(ILEmitterCtx context)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.EmitLsl(op.GetBitsCount() - op.Shift);

            context.EmitStintzr(op.Rd);
        }

        private static void EmitBfmLoadRn(ILEmitterCtx context)
        {
            OpCodeBfm64 op = (OpCodeBfm64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.EmitRor(op.Shift);

            context.EmitLdc_I(op.WMask & op.TMask);

            context.Emit(OpCodes.And);
        }
    }
}