using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
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

            EmitAluLoadRn(context);

            context.Emit(OpCodes.Ceq);

            context.EmitLdflg((int)PState.CBit);

            context.Emit(OpCodes.And);

            context.EmitLdtmp();

            EmitAluLoadRn(context);

            context.Emit(OpCodes.Clt_Un);
            context.Emit(OpCodes.Or);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitAddsCCheck(ILEmitterCtx context)
        {
            //C = Rd < Rn
            context.Emit(OpCodes.Dup);

            EmitAluLoadRn(context);

            context.Emit(OpCodes.Clt_Un);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitAddsVCheck(ILEmitterCtx context)
        {
            //V = (Rd ^ Rn) & ~(Rn ^ Rm) < 0
            context.Emit(OpCodes.Dup);

            EmitAluLoadRn(context);

            context.Emit(OpCodes.Xor);

            EmitAluLoadOpers(context);

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
            EmitAluLoadOpers(context);

            context.Emit(OpCodes.Ceq);

            context.EmitLdflg((int)PState.CBit);

            context.Emit(OpCodes.And);

            EmitAluLoadOpers(context);

            context.Emit(OpCodes.Cgt_Un);
            context.Emit(OpCodes.Or);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitSubsCCheck(ILEmitterCtx context)
        {
            //C = Rn == Rm || Rn > Rm = !(Rn < Rm)
            EmitAluLoadOpers(context);

            context.Emit(OpCodes.Clt_Un);

            context.EmitLdc_I4(1);

            context.Emit(OpCodes.Xor);

            context.EmitStflg((int)PState.CBit);
        }

        public static void EmitSubsVCheck(ILEmitterCtx context)
        {
            //V = (Rd ^ Rn) & (Rn ^ Rm) < 0
            context.Emit(OpCodes.Dup);

            EmitAluLoadRn(context);

            context.Emit(OpCodes.Xor);

            EmitAluLoadOpers(context);

            context.Emit(OpCodes.Xor);
            context.Emit(OpCodes.And);

            context.EmitLdc_I(0);

            context.Emit(OpCodes.Clt);

            context.EmitStflg((int)PState.VBit);
        }

        public static void EmitAluLoadRm(ILEmitterCtx context)
        {
            if (context.CurrOp is IOpCodeAluRs64 op)
            {
                context.EmitLdintzr(op.Rm);
            }
            else if (context.CurrOp is OpCodeAluRsImm32 op32)
            {
                InstEmit32Helper.EmitLoadFromRegister(context, op32.Rm);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static void EmitAluLoadOpers(ILEmitterCtx context, bool setCarry = true)
        {
            EmitAluLoadRn(context);
            EmitAluLoadOper2(context, setCarry);
        }

        public static void EmitAluLoadRn(ILEmitterCtx context)
        {
            if (context.CurrOp is IOpCodeAlu64 op)
            {
                if (op.DataOp == DataOp.Logical || op is IOpCodeAluRs64)
                {
                    context.EmitLdintzr(op.Rn);
                }
                else
                {
                    context.EmitLdint(op.Rn);
                }
            }
            else if (context.CurrOp is IOpCodeAlu32 op32)
            {
                InstEmit32Helper.EmitLoadFromRegister(context, op32.Rn);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static void EmitAluLoadOper2(ILEmitterCtx context, bool setCarry = true)
        {
            switch (context.CurrOp)
            {
                //ARM32.
                case OpCodeAluImm32 op:
                    context.EmitLdc_I4(op.Imm);

                    if (op.SetFlags && op.IsRotated)
                    {
                        context.EmitLdc_I4((int)((uint)op.Imm >> 31));

                        context.EmitStflg((int)PState.CBit);
                    }
                    break;

                case OpCodeAluRsImm32 op:
                    EmitLoadRmShiftedByImmediate(context, op, setCarry);
                    break;

                case OpCodeAluImm8T16 op:
                    context.EmitLdc_I4(op.Imm);
                    break;

                //ARM64.
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

                default: throw new InvalidOperationException();
            }
        }

        public static void EmitSetNzcv(ILEmitterCtx context)
        {
            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.And);
            context.EmitStflg((int)PState.VBit);

            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.Shr);
            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.And);
            context.EmitStflg((int)PState.CBit);

            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.Shr);
            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.And);
            context.EmitStflg((int)PState.ZBit);

            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.Shr);
            context.Emit(OpCodes.Ldc_I4_1);
            context.Emit(OpCodes.And);
            context.EmitStflg((int)PState.NBit);
        }

        //ARM32 helpers.
        private static void EmitLoadRmShiftedByImmediate(ILEmitterCtx context, OpCodeAluRsImm32 op, bool setCarry)
        {
            int shift = op.Imm;

            if (shift == 0)
            {
                switch (op.ShiftType)
                {
                    case ShiftType.Lsr: shift = 32; break;
                    case ShiftType.Asr: shift = 32; break;
                    case ShiftType.Ror: shift = 1;  break;
                }
            }

            context.EmitLdint(op.Rm);

            if (shift != 0)
            {
                setCarry &= op.SetFlags;

                switch (op.ShiftType)
                {
                    case ShiftType.Lsl: EmitLslC(context, setCarry, shift); break;
                    case ShiftType.Lsr: EmitLsrC(context, setCarry, shift); break;
                    case ShiftType.Asr: EmitAsrC(context, setCarry, shift); break;
                    case ShiftType.Ror:
                        if (op.Imm != 0)
                        {
                            EmitRorC(context, setCarry, shift);
                        }
                        else
                        {
                            EmitRrxC(context, setCarry);
                        }
                        break;
                }
            }
        }

        private static void EmitLslC(ILEmitterCtx context, bool setCarry, int shift)
        {
            if ((uint)shift > 32)
            {
                EmitShiftByMoreThan32(context, setCarry);
            }
            else if (shift == 32)
            {
                if (setCarry)
                {
                    context.EmitLdc_I4(1);

                    context.Emit(OpCodes.And);

                    context.EmitStflg((int)PState.CBit);
                }
                else
                {
                    context.Emit(OpCodes.Pop);
                }

                context.EmitLdc_I4(0);
            }
            else
            {
                if (setCarry)
                {
                    context.Emit(OpCodes.Dup);

                    context.EmitLsr(32 - shift);

                    context.EmitLdc_I4(1);

                    context.Emit(OpCodes.And);

                    context.EmitStflg((int)PState.CBit);
                }

                context.EmitLsl(shift);
            }
        }

        private static void EmitLsrC(ILEmitterCtx context, bool setCarry, int shift)
        {
            if ((uint)shift > 32)
            {
                EmitShiftByMoreThan32(context, setCarry);
            }
            else if (shift == 32)
            {
                if (setCarry)
                {
                    context.EmitLsr(31);

                    context.EmitStflg((int)PState.CBit);
                }
                else
                {
                    context.Emit(OpCodes.Pop);
                }

                context.EmitLdc_I4(0);
            }
            else
            {
                context.Emit(OpCodes.Dup);

                context.EmitLsr(shift - 1);

                context.EmitLdc_I4(1);

                context.Emit(OpCodes.And);

                context.EmitStflg((int)PState.CBit);

                context.EmitLsr(shift);
            }
        }

        private static void EmitShiftByMoreThan32(ILEmitterCtx context, bool setCarry)
        {
            context.Emit(OpCodes.Pop);

            context.EmitLdc_I4(0);

            if (setCarry)
            {
                context.Emit(OpCodes.Dup);

                context.EmitStflg((int)PState.CBit);
            }
        }

        private static void EmitAsrC(ILEmitterCtx context, bool setCarry, int shift)
        {
            if ((uint)shift >= 32)
            {
                context.EmitAsr(31);

                if (setCarry)
                {
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(1);

                    context.Emit(OpCodes.And);

                    context.EmitStflg((int)PState.CBit);
                }
            }
            else
            {
                if (setCarry)
                {
                    context.Emit(OpCodes.Dup);

                    context.EmitLsr(shift - 1);

                    context.EmitLdc_I4(1);

                    context.Emit(OpCodes.And);

                    context.EmitStflg((int)PState.CBit);
                }

                context.EmitAsr(shift);
            }
        }

        private static void EmitRorC(ILEmitterCtx context, bool setCarry, int shift)
        {
            shift &= 0x1f;

            context.EmitRor(shift);

            if (setCarry)
            {
                context.Emit(OpCodes.Dup);

                context.EmitLsr(31);

                context.EmitStflg((int)PState.CBit);
            }
        }

        private static void EmitRrxC(ILEmitterCtx context, bool setCarry)
        {
            //Rotate right by 1 with carry.
            if (setCarry)
            {
                context.Emit(OpCodes.Dup);

                context.EmitLdc_I4(1);

                context.Emit(OpCodes.And);

                context.EmitSttmp();
            }

            context.EmitLsr(1);

            context.EmitLdflg((int)PState.CBit);

            context.EmitLsl(31);

            context.Emit(OpCodes.Or);

            if (setCarry)
            {
                context.EmitLdtmp();
                context.EmitStflg((int)PState.CBit);
            }
        }
    }
}
