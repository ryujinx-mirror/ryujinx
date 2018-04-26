using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitAluHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Adc(AILEmitterCtx Context)  => EmitAdc(Context, false);
        public static void Adcs(AILEmitterCtx Context) => EmitAdc(Context, true);

        private static void EmitAdc(AILEmitterCtx Context, bool SetFlags)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Add);

            Context.EmitLdflg((int)APState.CBit);

            Type[] MthdTypes  = new Type[] { typeof(bool) };

            MethodInfo MthdInfo = typeof(Convert).GetMethod(nameof(Convert.ToInt32), MthdTypes);

            Context.EmitCall(MthdInfo);

            if (Context.CurrOp.RegisterSize != ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U8);
            }

            Context.Emit(OpCodes.Add);

            if (SetFlags)
            {
                Context.EmitZNFlagCheck();

                EmitAdcsCCheck(Context);
                EmitAddsVCheck(Context);
            }

            EmitDataStore(Context);
        }

        public static void Add(AILEmitterCtx Context) => EmitDataOp(Context, OpCodes.Add);

        public static void Adds(AILEmitterCtx Context)
        {
            Context.TryOptMarkCondWithoutCmp();

            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Add);

            Context.EmitZNFlagCheck();

            EmitAddsCCheck(Context);
            EmitAddsVCheck(Context);
            EmitDataStoreS(Context);
        }

        public static void And(AILEmitterCtx Context) => EmitDataOp(Context, OpCodes.And);

        public static void Ands(AILEmitterCtx Context)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.And);

            EmitZeroCVFlags(Context);

            Context.EmitZNFlagCheck();

            EmitDataStoreS(Context);
        }

        public static void Asrv(AILEmitterCtx Context) => EmitDataOpShift(Context, OpCodes.Shr);

        public static void Bic(AILEmitterCtx Context)  => EmitBic(Context, false);
        public static void Bics(AILEmitterCtx Context) => EmitBic(Context, true);

        private static void EmitBic(AILEmitterCtx Context, bool SetFlags)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Not);
            Context.Emit(OpCodes.And);

            if (SetFlags)
            {
                EmitZeroCVFlags(Context);

                Context.EmitZNFlagCheck();
            }

            EmitDataStore(Context, SetFlags);
        }

        public static void Cls(AILEmitterCtx Context)
        {
            AOpCodeAlu Op = (AOpCodeAlu)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            Context.EmitLdc_I4(Op.RegisterSize == ARegisterSize.Int32 ? 32 : 64);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.CountLeadingSigns));

            Context.EmitStintzr(Op.Rd);
        }

        public static void Clz(AILEmitterCtx Context)
        {
            AOpCodeAlu Op = (AOpCodeAlu)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            Context.EmitLdc_I4(Op.RegisterSize == ARegisterSize.Int32 ? 32 : 64);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.CountLeadingZeros));

            Context.EmitStintzr(Op.Rd);
        }

        public static void Eon(AILEmitterCtx Context)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Not);
            Context.Emit(OpCodes.Xor);

            EmitDataStore(Context);
        }

        public static void Eor(AILEmitterCtx Context) => EmitDataOp(Context, OpCodes.Xor);

        public static void Extr(AILEmitterCtx Context)
        {
            //TODO: Ensure that the Shift is valid for the Is64Bits.
            AOpCodeAluRs Op = (AOpCodeAluRs)Context.CurrOp;

            Context.EmitLdintzr(Op.Rm);

            if (Op.Shift > 0)
            {
                Context.EmitLdc_I4(Op.Shift);

                Context.Emit(OpCodes.Shr_Un);

                Context.EmitLdintzr(Op.Rn);
                Context.EmitLdc_I4(Op.GetBitsCount() - Op.Shift);

                Context.Emit(OpCodes.Shl);
                Context.Emit(OpCodes.Or);
            }

            EmitDataStore(Context);
        }

        public static void Lslv(AILEmitterCtx Context) => EmitDataOpShift(Context, OpCodes.Shl);
        public static void Lsrv(AILEmitterCtx Context) => EmitDataOpShift(Context, OpCodes.Shr_Un);

        public static void Sbc(AILEmitterCtx Context)  => EmitSbc(Context, false);
        public static void Sbcs(AILEmitterCtx Context) => EmitSbc(Context, true);

        private static void EmitSbc(AILEmitterCtx Context, bool SetFlags)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Sub);

            Context.EmitLdflg((int)APState.CBit);

            Type[] MthdTypes  = new Type[] { typeof(bool) };

            MethodInfo MthdInfo = typeof(Convert).GetMethod(nameof(Convert.ToInt32), MthdTypes);

            Context.EmitCall(MthdInfo);

            Context.EmitLdc_I4(1);

            Context.Emit(OpCodes.Xor);

            if (Context.CurrOp.RegisterSize != ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U8);
            }

            Context.Emit(OpCodes.Sub);

            if (SetFlags)
            {
                Context.EmitZNFlagCheck();

                EmitSbcsCCheck(Context);
                EmitSubsVCheck(Context);
            }

            EmitDataStore(Context);
        }

        public static void Sub(AILEmitterCtx Context) => EmitDataOp(Context, OpCodes.Sub);

        public static void Subs(AILEmitterCtx Context)
        {
            Context.TryOptMarkCondWithoutCmp();

            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Sub);

            Context.EmitZNFlagCheck();

            EmitSubsCCheck(Context);
            EmitSubsVCheck(Context);
            EmitDataStoreS(Context);
        }      

        public static void Orn(AILEmitterCtx Context)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Not);
            Context.Emit(OpCodes.Or);

            EmitDataStore(Context);
        }

        public static void Orr(AILEmitterCtx Context) => EmitDataOp(Context, OpCodes.Or);

        public static void Rbit(AILEmitterCtx Context) => EmitFallback32_64(Context,
            nameof(ASoftFallback.ReverseBits32),
            nameof(ASoftFallback.ReverseBits64));

        public static void Rev16(AILEmitterCtx Context) => EmitFallback32_64(Context,
            nameof(ASoftFallback.ReverseBytes16_32),
            nameof(ASoftFallback.ReverseBytes16_64));

        public static void Rev32(AILEmitterCtx Context) => EmitFallback32_64(Context,
            nameof(ASoftFallback.ReverseBytes32_32),
            nameof(ASoftFallback.ReverseBytes32_64));

        public static void EmitFallback32_64(AILEmitterCtx Context, string Name32, string Name64)
        {
            AOpCodeAlu Op = (AOpCodeAlu)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            if (Op.RegisterSize == ARegisterSize.Int32)
            {
                ASoftFallback.EmitCall(Context, Name32);
            }
            else
            {
                ASoftFallback.EmitCall(Context, Name64);
            }

            Context.EmitStintzr(Op.Rd);
        }

        public static void Rev64(AILEmitterCtx Context)
        {
            AOpCodeAlu Op = (AOpCodeAlu)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ReverseBytes64));

            Context.EmitStintzr(Op.Rd);
        }

        public static void Rorv(AILEmitterCtx Context)
        {
            EmitDataLoadRn(Context);
            EmitDataLoadShift(Context);

            Context.Emit(OpCodes.Shr_Un);

            EmitDataLoadRn(Context);

            Context.EmitLdc_I4(Context.CurrOp.GetBitsCount());

            EmitDataLoadShift(Context);

            Context.Emit(OpCodes.Sub);
            Context.Emit(OpCodes.Shl);
            Context.Emit(OpCodes.Or);

            EmitDataStore(Context);
        }

        public static void Sdiv(AILEmitterCtx Context) => EmitDiv(Context, OpCodes.Div);
        public static void Udiv(AILEmitterCtx Context) => EmitDiv(Context, OpCodes.Div_Un);

        private static void EmitDiv(AILEmitterCtx Context, OpCode ILOp)
        {
            //If Rm == 0, Rd = 0 (division by zero).
            Context.EmitLdc_I(0);

            EmitDataLoadRm(Context);

            Context.EmitLdc_I(0);

            AILLabel BadDiv = new AILLabel();

            Context.Emit(OpCodes.Beq_S, BadDiv);
            Context.Emit(OpCodes.Pop);

            if (ILOp == OpCodes.Div)
            {
                //If Rn == INT_MIN && Rm == -1, Rd = INT_MIN (overflow).
                long IntMin = 1L << (Context.CurrOp.GetBitsCount() - 1);

                Context.EmitLdc_I(IntMin);

                EmitDataLoadRn(Context);

                Context.EmitLdc_I(IntMin);
                
                Context.Emit(OpCodes.Ceq);

                EmitDataLoadRm(Context);

                Context.EmitLdc_I(-1);

                Context.Emit(OpCodes.Ceq);
                Context.Emit(OpCodes.And);
                Context.Emit(OpCodes.Brtrue_S, BadDiv);
                Context.Emit(OpCodes.Pop);
            }

            EmitDataLoadRn(Context);
            EmitDataLoadRm(Context);

            Context.Emit(ILOp);

            Context.MarkLabel(BadDiv);

            EmitDataStore(Context);
        }

        private static void EmitDataOp(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitDataLoadOpers(Context);

            Context.Emit(ILOp);

            EmitDataStore(Context);
        }

        private static void EmitDataOpShift(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitDataLoadRn(Context);
            EmitDataLoadShift(Context);

            Context.Emit(ILOp);

            EmitDataStore(Context);
        }

        private static void EmitDataLoadShift(AILEmitterCtx Context)
        {
            EmitDataLoadRm(Context);

            Context.EmitLdc_I(Context.CurrOp.GetBitsCount() - 1);

            Context.Emit(OpCodes.And);

            //Note: Only 32-bits shift values are valid, so when the value is 64-bits
            //we need to cast it to a 32-bits integer. This is fine because we
            //AND the value and only keep the lower 5 or 6 bits anyway -- it
            //could very well fit on a byte.
            if (Context.CurrOp.RegisterSize != ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_I4);
            }
        }

        private static void EmitZeroCVFlags(AILEmitterCtx Context)
        {
            Context.EmitLdc_I4(0);

            Context.EmitStflg((int)APState.VBit);

            Context.EmitLdc_I4(0);

            Context.EmitStflg((int)APState.CBit);
        }
    }
}
