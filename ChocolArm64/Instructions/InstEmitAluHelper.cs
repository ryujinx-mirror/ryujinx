using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static class InstEmitAluHelper
    {
        public static void EmitAdcsCCheck(ILEmitterCtx context)
        {
            //C = (Rd == Rn && CIn) || Rd < Rn
            context.EmitSttmp();
            context.EmitLdtmp();
            context.EmitLdtmp();

            EmitDataLoadRn(context);

            context.Emit(OpCodes.Ceq);

            context.EmitLdflg((int)PState.CBit);

            context.Emit(OpCodes.And);

            context.EmitLdtmp();

            EmitDataLoadRn(context);

            context.Emit(OpCodes.Clt_Un);
            context.Emit(OpCodes.Or);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitAddsCCheck(ILEmitterCtx context)
        {
            //C = Rd < Rn
            context.Emit(OpCodes.Dup);

            EmitDataLoadRn(context);

            context.Emit(OpCodes.Clt_Un);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitAddsVCheck(ILEmitterCtx context)
        {
            //V = (Rd ^ Rn) & ~(Rn ^ Rm) < 0
            context.Emit(OpCodes.Dup);

            EmitDataLoadRn(context);

            context.Emit(OpCodes.Xor);

            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Xor);
            context.Emit(OpCodes.Not);
            context.Emit(OpCodes.And);

            context.EmitLdc_I(0);

            context.Emit(OpCodes.Clt);

            context.EmitStflg((int)PState.VBit);
        }

        public static void EmitSbcsCCheck(ILEmitterCtx context)
        {
            //C = (Rn == Rm && CIn) || Rn > Rm
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Ceq);

            context.EmitLdflg((int)PState.CBit);

            context.Emit(OpCodes.And);

            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Cgt_Un);
            context.Emit(OpCodes.Or);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitSubsCCheck(ILEmitterCtx context)
        {
            //C = Rn == Rm || Rn > Rm = !(Rn < Rm)
            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Clt_Un);

            context.EmitLdc_I4(1);

            context.Emit(OpCodes.Xor);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitSubsVCheck(ILEmitterCtx context)
        {
            //V = (Rd ^ Rn) & (Rn ^ Rm) < 0
            context.Emit(OpCodes.Dup);

            EmitDataLoadRn(context);

            context.Emit(OpCodes.Xor);

            EmitDataLoadOpers(context);

            context.Emit(OpCodes.Xor);
            context.Emit(OpCodes.And);

            context.EmitLdc_I(0);

            context.Emit(OpCodes.Clt);

            context.EmitStflg((int)PState.VBit);
        }

        public static void EmitDataLoadRm(ILEmitterCtx context)
        {
            context.EmitLdintzr(((IOpCodeAluRs64)context.CurrOp).Rm);
        }

        public static void EmitDataLoadOpers(ILEmitterCtx context)
        {
            EmitDataLoadRn(context);
            EmitDataLoadOper2(context);
        }

        public static void EmitDataLoadRn(ILEmitterCtx context)
        {
            IOpCodeAlu64 op = (IOpCodeAlu64)context.CurrOp;

            if (op.DataOp == DataOp.Logical || op is IOpCodeAluRs64)
            {
                context.EmitLdintzr(op.Rn);
            }
            else
            {
                context.EmitLdint(op.Rn);
            }
        }

        public static void EmitDataLoadOper2(ILEmitterCtx context)
        {
            switch (context.CurrOp)
            {
                case IOpCodeAluImm64 op:
                    context.EmitLdc_I(op.Imm);
                    break;

                case IOpCodeAluRs64 op:
                    context.EmitLdintzr(op.Rm);

                    switch (op.ShiftType)
                    {
                        case ShiftType.Lsl: context.EmitLsl(op.Shift); break;
                        case ShiftType.Lsr: context.EmitLsr(op.Shift); break;
                        case ShiftType.Asr: context.EmitAsr(op.Shift); break;
                        case ShiftType.Ror: context.EmitRor(op.Shift); break;
                    }
                    break;

                case IOpCodeAluRx64 op:
                    context.EmitLdintzr(op.Rm);
                    context.EmitCast(op.IntType);
                    context.EmitLsl(op.Shift);
                    break;
            }
        }

        public static void EmitDataStore(ILEmitterCtx context)  => EmitDataStore(context, false);
        public static void EmitDataStoreS(ILEmitterCtx context) => EmitDataStore(context, true);

        public static void EmitDataStore(ILEmitterCtx context, bool setFlags)
        {
            IOpCodeAlu64 op = (IOpCodeAlu64)context.CurrOp;

            if (setFlags || op is IOpCodeAluRs64)
            {
                context.EmitStintzr(op.Rd);
            }
            else
            {
                context.EmitStint(op.Rd);
            }
        }

        public static void EmitSetNzcv(ILEmitterCtx context, int nzcv)
        {
            context.EmitLdc_I4((nzcv >> 0) & 1);

            context.EmitStflg((int)PState.VBit);

            context.EmitLdc_I4((nzcv >> 1) & 1);

            context.EmitStflg((int)PState.CBit);

            context.EmitLdc_I4((nzcv >> 2) & 1);

            context.EmitStflg((int)PState.ZBit);

            context.EmitLdc_I4((nzcv >> 3) & 1);

            context.EmitStflg((int)PState.NBit);
        }
    }
}