using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Bfm(AILEmitterCtx Context)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            EmitBfmLoadRn(Context);

            Context.EmitLdintzr(Op.Rd);
            Context.EmitLdc_I(~Op.WMask & Op.TMask);

            Context.Emit(OpCodes.And);
            Context.Emit(OpCodes.Or);

            Context.EmitLdintzr(Op.Rd);
            Context.EmitLdc_I(~Op.TMask);

            Context.Emit(OpCodes.And);
            Context.Emit(OpCodes.Or);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Sbfm(AILEmitterCtx Context)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            int BitsCount = Op.GetBitsCount();

            if (Op.Pos + 1 == BitsCount)
            {
                EmitSbfmShift(Context);
            }
            else if (Op.Pos < Op.Shift)
            {
                EmitSbfiz(Context);
            }
            else if (Op.Pos == 7 && Op.Shift == 0)
            {
                EmitSbfmCast(Context, OpCodes.Conv_I1);
            }
            else if (Op.Pos == 15 && Op.Shift == 0)
            {
                EmitSbfmCast(Context, OpCodes.Conv_I2);
            }
            else if (Op.Pos == 31 && Op.Shift == 0)
            {
                EmitSbfmCast(Context, OpCodes.Conv_I4);
            }
            else if (Op.Shift == 0)
            {
                Context.EmitLdintzr(Op.Rn);

                Context.EmitLsl(BitsCount - 1 - Op.Pos);
                Context.EmitAsr(BitsCount - 1);

                Context.EmitStintzr(Op.Rd);
            }
            else
            {
                EmitBfmLoadRn(Context);

                Context.EmitLdintzr(Op.Rn);

                Context.EmitLsl(BitsCount - 1 - Op.Pos);
                Context.EmitAsr(BitsCount - 1);

                Context.EmitLdc_I(~Op.TMask);

                Context.Emit(OpCodes.And);
                Context.Emit(OpCodes.Or);

                Context.EmitStintzr(Op.Rd);
            }
        }

        public static void Ubfm(AILEmitterCtx Context)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            if (Op.Pos + 1 == Op.GetBitsCount())
            {
                EmitUbfmShift(Context);
            }
            else if (Op.Pos < Op.Shift)
            {
                EmitUbfiz(Context);
            }
            else if (Op.Pos + 1 == Op.Shift)
            {
                EmitBfmLsl(Context);
            }
            else if (Op.Pos == 7 && Op.Shift == 0)
            {
                EmitUbfmCast(Context, OpCodes.Conv_U1);
            }
            else if (Op.Pos == 15 && Op.Shift == 0)
            {
                EmitUbfmCast(Context, OpCodes.Conv_U2);
            }
            else
            {
                EmitBfmLoadRn(Context);

                Context.EmitStintzr(Op.Rd);
            }
        }

        private static void EmitSbfiz(AILEmitterCtx Context) => EmitBfiz(Context, true);
        private static void EmitUbfiz(AILEmitterCtx Context) => EmitBfiz(Context, false);

        private static void EmitBfiz(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            int Width = Op.Pos + 1;

            Context.EmitLdintzr(Op.Rn);

            Context.EmitLsl(Op.GetBitsCount() - Width);

            if (Signed)
            {
                Context.EmitAsr(Op.Shift - Width);
            }
            else
            {
                Context.EmitLsr(Op.Shift - Width);
            }

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitSbfmCast(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitBfmCast(Context, ILOp, true);
        }

        private static void EmitUbfmCast(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitBfmCast(Context, ILOp, false);
        }

        private static void EmitBfmCast(AILEmitterCtx Context, OpCode ILOp, bool Signed)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            Context.Emit(ILOp);

            if (Op.RegisterSize != ARegisterSize.Int32)
            {
                Context.Emit(Signed
                    ? OpCodes.Conv_I8
                    : OpCodes.Conv_U8);
            }

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitSbfmShift(AILEmitterCtx Context)
        {
            EmitBfmShift(Context, true);
        }

        private static void EmitUbfmShift(AILEmitterCtx Context)
        {
            EmitBfmShift(Context, false);
        }

        private static void EmitBfmShift(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdc_I4(Op.Shift);

            Context.Emit(Signed
                ? OpCodes.Shr
                : OpCodes.Shr_Un);

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitBfmLsl(AILEmitterCtx Context)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            Context.EmitLsl(Op.GetBitsCount() - Op.Shift);

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitBfmLoadRn(AILEmitterCtx Context)
        {
            AOpCodeBfm Op = (AOpCodeBfm)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            Context.EmitRor(Op.Shift);

            Context.EmitLdc_I(Op.WMask & Op.TMask);

            Context.Emit(OpCodes.And);
        }
    }
}