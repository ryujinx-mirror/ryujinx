using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static class AInstEmitAluHelper
    {
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
            //V = (Rd ^ Rn) & (Rd ^ Rm) & ~(Rn ^ Rm) < 0
            Context.EmitSttmp();
            Context.EmitLdtmp();
            Context.EmitLdtmp();

            EmitDataLoadRn(Context);

            Context.Emit(OpCodes.Xor);

            Context.EmitLdtmp();

            EmitDataLoadOper2(Context);

            Context.Emit(OpCodes.Xor);
            Context.Emit(OpCodes.And);

            EmitDataLoadOpers(Context);

            Context.Emit(OpCodes.Xor);
            Context.Emit(OpCodes.Not);
            Context.Emit(OpCodes.And);

            Context.EmitLdc_I(0);

            Context.Emit(OpCodes.Clt);

            Context.EmitStflg((int)APState.VBit);
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
    }
}