using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instructions.InstEmitAluHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Adc(ILEmitterCtx context)  => EmitAdc(context, false);
        public static void Adcs(ILEmitterCtx context) => EmitAdc(context, true);

        private static void EmitAdc(ILEmitterCtx context, bool setFlags)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Add);

            context.EmitLdflg((int)PState.CBit);

            Type[] mthdTypes  = new Type[] { typeof(bool) };

            MethodInfo mthdInfo = typeof(Convert).GetMethod(nameof(Convert.ToInt32), mthdTypes);

            context.EmitCall(mthdInfo);

            if (context.CurrOp.RegisterSize != RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.Emit(OpCodes.Add);

            if (setFlags)
            {
                context.EmitZnFlagCheck();

                EmitAdcsCCheck(context);
                EmitAddsVCheck(context);
            }

            EmitDataStore(context);
        }

        public static void Add(ILEmitterCtx context) => EmitDataOp(context, OpCodes.Add);

        public static void Adds(ILEmitterCtx context)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Add);

            context.EmitZnFlagCheck();

            EmitAddsCCheck(context);
            EmitAddsVCheck(context);
            EmitDataStoreS(context);
        }

        public static void And(ILEmitterCtx context) => EmitDataOp(context, OpCodes.And);

        public static void Ands(ILEmitterCtx context)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.And);

            EmitZeroCvFlags(context);

            context.EmitZnFlagCheck();

            EmitDataStoreS(context);
        }

        public static void Asrv(ILEmitterCtx context) => EmitDataOpShift(context, OpCodes.Shr);

        public static void Bic(ILEmitterCtx context)  => EmitBic(context, false);
        public static void Bics(ILEmitterCtx context) => EmitBic(context, true);

        private static void EmitBic(ILEmitterCtx context, bool setFlags)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Not);
            context.Emit(OpCodes.And);

            if (setFlags)
            {
                EmitZeroCvFlags(context);

                context.EmitZnFlagCheck();
            }

            EmitDataStore(context, setFlags);
        }

        public static void Cls(ILEmitterCtx context)
        {
            OpCodeAlu64 op = (OpCodeAlu64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            context.EmitLdc_I4(op.RegisterSize == RegisterSize.Int32 ? 32 : 64);

            SoftFallback.EmitCall(context, nameof(SoftFallback.CountLeadingSigns));

            context.EmitStintzr(op.Rd);
        }

        public static void Clz(ILEmitterCtx context)
        {
            OpCodeAlu64 op = (OpCodeAlu64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (Lzcnt.IsSupported)
            {
                Type tValue = op.RegisterSize == RegisterSize.Int32 ? typeof(uint) : typeof(ulong);

                context.EmitCall(typeof(Lzcnt).GetMethod(nameof(Lzcnt.LeadingZeroCount), new Type[] { tValue }));
            }
            else
            {
                context.EmitLdc_I4(op.RegisterSize == RegisterSize.Int32 ? 32 : 64);

                SoftFallback.EmitCall(context, nameof(SoftFallback.CountLeadingZeros));
            }

            context.EmitStintzr(op.Rd);
        }

        public static void Eon(ILEmitterCtx context)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Not);
            context.Emit(OpCodes.Xor);

            EmitDataStore(context);
        }

        public static void Eor(ILEmitterCtx context) => EmitDataOp(context, OpCodes.Xor);

        public static void Extr(ILEmitterCtx context)
        {
            //TODO: Ensure that the Shift is valid for the Is64Bits.
            OpCodeAluRs64 op = (OpCodeAluRs64)context.CurrOp;

            context.EmitLdintzr(op.Rm);

            if (op.Shift > 0)
            {
                context.EmitLdc_I4(op.Shift);

                context.Emit(OpCodes.Shr_Un);

                context.EmitLdintzr(op.Rn);
                context.EmitLdc_I4(op.GetBitsCount() - op.Shift);

                context.Emit(OpCodes.Shl);
                context.Emit(OpCodes.Or);
            }

            EmitDataStore(context);
        }

        public static void Lslv(ILEmitterCtx context) => EmitDataOpShift(context, OpCodes.Shl);
        public static void Lsrv(ILEmitterCtx context) => EmitDataOpShift(context, OpCodes.Shr_Un);

        public static void Sbc(ILEmitterCtx context)  => EmitSbc(context, false);
        public static void Sbcs(ILEmitterCtx context) => EmitSbc(context, true);

        private static void EmitSbc(ILEmitterCtx context, bool setFlags)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Sub);

            context.EmitLdflg((int)PState.CBit);

            Type[] mthdTypes  = new Type[] { typeof(bool) };

            MethodInfo mthdInfo = typeof(Convert).GetMethod(nameof(Convert.ToInt32), mthdTypes);

            context.EmitCall(mthdInfo);

            context.EmitLdc_I4(1);

            context.Emit(OpCodes.Xor);

            if (context.CurrOp.RegisterSize != RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.Emit(OpCodes.Sub);

            if (setFlags)
            {
                context.EmitZnFlagCheck();

                EmitSbcsCCheck(context);
                EmitSubsVCheck(context);
            }

            EmitDataStore(context);
        }

        public static void Sub(ILEmitterCtx context) => EmitDataOp(context, OpCodes.Sub);

        public static void Subs(ILEmitterCtx context)
        {
            context.TryOptMarkCondWithoutCmp();

            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Sub);

            context.EmitZnFlagCheck();

            EmitSubsCCheck(context);
            EmitSubsVCheck(context);
            EmitDataStoreS(context);
        }

        public static void Orn(ILEmitterCtx context)
        {
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Not);
            context.Emit(OpCodes.Or);

            EmitDataStore(context);
        }

        public static void Orr(ILEmitterCtx context) => EmitDataOp(context, OpCodes.Or);

        public static void Rbit(ILEmitterCtx context) => EmitFallback32_64(context,
            nameof(SoftFallback.ReverseBits32),
            nameof(SoftFallback.ReverseBits64));

        public static void Rev16(ILEmitterCtx context) => EmitFallback32_64(context,
            nameof(SoftFallback.ReverseBytes16_32),
            nameof(SoftFallback.ReverseBytes16_64));

        public static void Rev32(ILEmitterCtx context) => EmitFallback32_64(context,
            nameof(SoftFallback.ReverseBytes32_32),
            nameof(SoftFallback.ReverseBytes32_64));

        private static void EmitFallback32_64(ILEmitterCtx context, string name32, string name64)
        {
            OpCodeAlu64 op = (OpCodeAlu64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (op.RegisterSize == RegisterSize.Int32)
            {
                SoftFallback.EmitCall(context, name32);
            }
            else
            {
                SoftFallback.EmitCall(context, name64);
            }

            context.EmitStintzr(op.Rd);
        }

        public static void Rev64(ILEmitterCtx context)
        {
            OpCodeAlu64 op = (OpCodeAlu64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            SoftFallback.EmitCall(context, nameof(SoftFallback.ReverseBytes64));

            context.EmitStintzr(op.Rd);
        }

        public static void Rorv(ILEmitterCtx context)
        {
            EmitDataLoadRn(context);
            EmitDataLoadShift(context);

            context.Emit(OpCodes.Shr_Un);

            EmitDataLoadRn(context);

            context.EmitLdc_I4(context.CurrOp.GetBitsCount());

            EmitDataLoadShift(context);

            context.Emit(OpCodes.Sub);
            context.Emit(OpCodes.Shl);
            context.Emit(OpCodes.Or);

            EmitDataStore(context);
        }

        public static void Sdiv(ILEmitterCtx context) => EmitDiv(context, OpCodes.Div);
        public static void Udiv(ILEmitterCtx context) => EmitDiv(context, OpCodes.Div_Un);

        private static void EmitDiv(ILEmitterCtx context, OpCode ilOp)
        {
            //If Rm == 0, Rd = 0 (division by zero).
            context.EmitLdc_I(0);

            EmitDataLoadRm(context);

            context.EmitLdc_I(0);

            ILLabel badDiv = new ILLabel();

            context.Emit(OpCodes.Beq_S, badDiv);
            context.Emit(OpCodes.Pop);

            if (ilOp == OpCodes.Div)
            {
                //If Rn == INT_MIN && Rm == -1, Rd = INT_MIN (overflow).
                long intMin = 1L << (context.CurrOp.GetBitsCount() - 1);

                context.EmitLdc_I(intMin);

                EmitDataLoadRn(context);

                context.EmitLdc_I(intMin);

                context.Emit(OpCodes.Ceq);

                EmitDataLoadRm(context);

                context.EmitLdc_I(-1);

                context.Emit(OpCodes.Ceq);
                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Brtrue_S, badDiv);
                context.Emit(OpCodes.Pop);
            }

            EmitDataLoadRn(context);
            EmitDataLoadRm(context);

            context.Emit(ilOp);

            context.MarkLabel(badDiv);

            EmitDataStore(context);
        }

        private static void EmitDataOp(ILEmitterCtx context, OpCode ilOp)
        {
            EmitDataLoadOpers(context);

            context.Emit(ilOp);

            EmitDataStore(context);
        }

        private static void EmitDataOpShift(ILEmitterCtx context, OpCode ilOp)
        {
            EmitDataLoadRn(context);
            EmitDataLoadShift(context);

            context.Emit(ilOp);

            EmitDataStore(context);
        }

        private static void EmitDataLoadShift(ILEmitterCtx context)
        {
            EmitDataLoadRm(context);

            context.EmitLdc_I(context.CurrOp.GetBitsCount() - 1);

            context.Emit(OpCodes.And);

            //Note: Only 32-bits shift values are valid, so when the value is 64-bits
            //we need to cast it to a 32-bits integer. This is fine because we
            //AND the value and only keep the lower 5 or 6 bits anyway -- it
            //could very well fit on a byte.
            if (context.CurrOp.RegisterSize != RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_I4);
            }
        }

        private static void EmitZeroCvFlags(ILEmitterCtx context)
        {
            context.EmitLdc_I4(0);

            context.EmitStflg((int)PState.VBit);

            context.EmitLdc_I4(0);

            context.EmitStflg((int)PState.CBit);
        }
    }
}
