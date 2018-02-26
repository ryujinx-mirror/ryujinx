using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static class AInstEmitAluHelper
    {
        public static void EmitAdcsCCheck(AILEmitterCtx Context)
        {
            //C = (Rd == Rn && CIn) || Rd < Rn
            Context.EmitSttmp();
            Context.EmitLdtmp();
            Context.EmitLdtmp();

            EmitDataLoadRn(Context);

            Context.Emit(OpCodes.Ceq);

            Context.EmitLdflg((int)APState.CBit);

            Context.Emit(OpCodes.And);

            Context.EmitLdtmp();

            EmitDataLoadRn(Context);

            Context.Emit(OpCodes.Clt_Un);
            Context.Emit(OpCodes.Or);

            Context.EmitStflg((int)APState.CBit);
        }

        public static void EmitAddsCCheck(AILEmitterCtx Context)
        {
            //C = Rd < Rn
            Context.Emit(OpCodes.Dup);

            EmitDataLoadRn(Context);

            Context.Emit(OpCodes.Clt_Un);

            Context.EmitStflg((int)APState.CBit);
        }

        public static void EmitAddsVCheck(AILEmitterCtx Context)
        {
            //V = (Rd ^ Rn) & ~(Rn ^ Rm) < 0
            Context.Emit(OpCodes.Dup);

            EmitDataLoadRn(Context);

            Context.Emit(OpCodes.Xor);

            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Xor);
            Context.Emit(OpCodes.Not);
            Context.Emit(OpCodes.And);

            Context.EmitLdc_I(0);

            Context.Emit(OpCodes.Clt);

            Context.EmitStflg((int)APState.VBit);
        }

        public static void EmitSbcsCCheck(AILEmitterCtx Context)
        {
            //C = (Rn == Rm && CIn) || Rn > Rm
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Ceq);

            Context.EmitLdflg((int)APState.CBit);

            Context.Emit(OpCodes.And);

            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Cgt_Un);
            Context.Emit(OpCodes.Or);

            Context.EmitStflg((int)APState.CBit);
        }

        public static void EmitSubsCCheck(AILEmitterCtx Context)
        {
            //C = Rn == Rm || Rn > Rm = !(Rn < Rm)
            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Clt_Un);

            Context.EmitLdc_I4(1);

            Context.Emit(OpCodes.Xor);

            Context.EmitStflg((int)APState.CBit);
        }

        public static void EmitSubsVCheck(AILEmitterCtx Context)
        {
            //V = (Rd ^ Rn) & (Rn ^ Rm) < 0
            Context.Emit(OpCodes.Dup);

            EmitDataLoadRn(Context);

            Context.Emit(OpCodes.Xor);

            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Xor);
            Context.Emit(OpCodes.And);

            Context.EmitLdc_I(0);

            Context.Emit(OpCodes.Clt);

            Context.EmitStflg((int)APState.VBit);
        }

        public static void EmitDataLoadRm(AILEmitterCtx Context)
        {
            Context.EmitLdintzr(((IAOpCodeAluRs)Context.CurrOp).Rm);
        }

        public static void EmitDataLoadOpers(AILEmitterCtx Context)
        {
            EmitDataLoadRn(Context);
            EmitDataLoadOper2(Context);
        }

        public static void EmitDataLoadRn(AILEmitterCtx Context)
        {
            IAOpCodeAlu Op = (IAOpCodeAlu)Context.CurrOp;

            if (Op.DataOp == ADataOp.Logical || Op is IAOpCodeAluRs)
            {
                Context.EmitLdintzr(Op.Rn);
            }
            else
            {
                Context.EmitLdint(Op.Rn);
            }
        }

        public static void EmitDataLoadOper2(AILEmitterCtx Context)
        {
            switch (Context.CurrOp)
            {
                case IAOpCodeAluImm Op:
                    Context.EmitLdc_I(Op.Imm);
                    break;

                case IAOpCodeAluRs Op:
                    Context.EmitLdintzr(Op.Rm);

                    switch (Op.ShiftType)
                    {
                        case AShiftType.Lsl: Context.EmitLsl(Op.Shift); break;
                        case AShiftType.Lsr: Context.EmitLsr(Op.Shift); break;
                        case AShiftType.Asr: Context.EmitAsr(Op.Shift); break;
                        case AShiftType.Ror: Context.EmitRor(Op.Shift); break;
                    }
                    break;

                case IAOpCodeAluRx Op:
                    Context.EmitLdintzr(Op.Rm);
                    Context.EmitCast(Op.IntType);
                    Context.EmitLsl(Op.Shift);
                    break;
            }
        }

        public static void EmitDataStore(AILEmitterCtx Context)  => EmitDataStore(Context, false);
        public static void EmitDataStoreS(AILEmitterCtx Context) => EmitDataStore(Context, true);

        public static void EmitDataStore(AILEmitterCtx Context, bool SetFlags)
        {
            IAOpCodeAlu Op = (IAOpCodeAlu)Context.CurrOp;

            if (SetFlags || Op is IAOpCodeAluRs)
            {
                Context.EmitStintzr(Op.Rd);
            }
            else
            {
                Context.EmitStint(Op.Rd);
            }
        }

        public static void EmitSetNZCV(AILEmitterCtx Context, int NZCV)
        {
            Context.EmitLdc_I4((NZCV >> 0) & 1);

            Context.EmitStflg((int)APState.VBit);

            Context.EmitLdc_I4((NZCV >> 1) & 1);

            Context.EmitStflg((int)APState.CBit);

            Context.EmitLdc_I4((NZCV >> 2) & 1);

            Context.EmitStflg((int)APState.ZBit);

            Context.EmitLdc_I4((NZCV >> 3) & 1);

            Context.EmitStflg((int)APState.NBit);
        }
    }
}