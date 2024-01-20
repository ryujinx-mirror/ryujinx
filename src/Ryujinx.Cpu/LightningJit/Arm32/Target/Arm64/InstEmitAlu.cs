using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitAlu
    {
        private const uint Imm12Limit = 0x1000;

        public static void AdcI(CodeGenContext context, uint rd, uint rn, uint imm, bool s)
        {
            EmitI(context, s ? context.Arm64Assembler.Adcs : context.Arm64Assembler.Adc, rd, rn, imm, s);
        }

        public static void AdcR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitR(context, s ? context.Arm64Assembler.Adcs : context.Arm64Assembler.Adc, rd, rn, rm, sType, imm5, s);
        }

        public static void AdcRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitRr(context, s ? context.Arm64Assembler.Adcs : context.Arm64Assembler.Adc, rd, rn, rm, sType, rs, s);
        }

        public static void AddI(CodeGenContext context, uint rd, uint rn, uint imm, bool s)
        {
            EmitArithmeticI(context, s ? context.Arm64Assembler.Adds : context.Arm64Assembler.Add, rd, rn, imm, s);
        }

        public static void AddR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitArithmeticR(context, s ? context.Arm64Assembler.Adds : context.Arm64Assembler.Add, rd, rn, rm, sType, imm5, s);
        }

        public static void AddRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitRr(context, s ? context.Arm64Assembler.Adds : context.Arm64Assembler.Add, rd, rn, rm, sType, rs, s);
        }

        public static void Adr(CodeGenContext context, uint rd, uint imm, bool add)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);

            uint pc = context.Pc & ~3u;

            if (add)
            {
                pc += imm;
            }
            else
            {
                pc -= imm;
            }

            context.Arm64Assembler.Mov(rdOperand, pc);
        }

        public static void AndI(CodeGenContext context, uint rd, uint rn, uint imm, bool immRotated, bool s)
        {
            EmitLogicalI(context, s ? context.Arm64Assembler.Ands : context.Arm64Assembler.And, rd, rn, imm, immRotated, s);
        }

        public static void AndR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitLogicalR(context, s ? context.Arm64Assembler.Ands : context.Arm64Assembler.And, rd, rn, rm, sType, imm5, s);
        }

        public static void AndRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitLogicalRr(context, s ? context.Arm64Assembler.Ands : context.Arm64Assembler.And, rd, rn, rm, sType, rs, s);
        }

        public static void BicI(CodeGenContext context, uint rd, uint rn, uint imm, bool immRotated, bool s)
        {
            if (!s && CodeGenCommon.TryEncodeBitMask(OperandType.I32, ~imm, out _, out _, out _))
            {
                AndI(context, rd, rn, ~imm, immRotated, s);
            }
            else
            {
                EmitLogicalI(context, s ? context.Arm64Assembler.Bics : context.Arm64Assembler.Bic, rd, rn, imm, immRotated, s, immForm: false);
            }
        }

        public static void BicR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitLogicalR(context, s ? context.Arm64Assembler.Bics : context.Arm64Assembler.Bic, rd, rn, rm, sType, imm5, s);
        }

        public static void BicRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitLogicalRr(context, s ? context.Arm64Assembler.Bics : context.Arm64Assembler.Bic, rd, rn, rm, sType, rs, s);
        }

        public static void CmnI(CodeGenContext context, uint rn, uint imm)
        {
            EmitCompareI(context, context.Arm64Assembler.Cmn, rn, imm);
        }

        public static void CmnR(CodeGenContext context, uint rn, uint rm, uint sType, uint imm5)
        {
            EmitCompareR(context, context.Arm64Assembler.Cmn, rn, rm, sType, imm5);
        }

        public static void CmnRr(CodeGenContext context, uint rn, uint rm, uint sType, uint rs)
        {
            EmitCompareRr(context, context.Arm64Assembler.Cmn, rn, rm, sType, rs);
        }

        public static void CmpI(CodeGenContext context, uint rn, uint imm)
        {
            EmitCompareI(context, context.Arm64Assembler.Cmp, rn, imm);
        }

        public static void CmpR(CodeGenContext context, uint rn, uint rm, uint sType, uint imm5)
        {
            EmitCompareR(context, context.Arm64Assembler.Cmp, rn, rm, sType, imm5);
        }

        public static void CmpRr(CodeGenContext context, uint rn, uint rm, uint sType, uint rs)
        {
            EmitCompareRr(context, context.Arm64Assembler.Cmp, rn, rm, sType, rs);
        }

        public static void EorI(CodeGenContext context, uint rd, uint rn, uint imm, bool immRotated, bool s)
        {
            EmitLogicalI(context, s ? context.Arm64Assembler.Eors : context.Arm64Assembler.Eor, rd, rn, imm, immRotated, s);
        }

        public static void EorR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitLogicalR(context, s ? context.Arm64Assembler.Eors : context.Arm64Assembler.Eor, rd, rn, rm, sType, imm5, s);
        }

        public static void EorRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitLogicalRr(context, s ? context.Arm64Assembler.Eors : context.Arm64Assembler.Eor, rd, rn, rm, sType, rs, s);
        }

        public static void OrnI(CodeGenContext context, uint rd, uint rn, uint imm, bool immRotated, bool s)
        {
            if (!s && CodeGenCommon.TryEncodeBitMask(OperandType.I32, ~imm, out _, out _, out _))
            {
                OrrI(context, rd, rn, ~imm, immRotated, s);
            }
            else
            {
                EmitLogicalI(context, s ? context.Arm64Assembler.Orns : context.Arm64Assembler.Orn, rd, rn, imm, immRotated, s, immForm: false);
            }
        }

        public static void OrnR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitLogicalR(context, s ? context.Arm64Assembler.Orns : context.Arm64Assembler.Orn, rd, rn, rm, sType, imm5, s);
        }

        public static void OrrI(CodeGenContext context, uint rd, uint rn, uint imm, bool immRotated, bool s)
        {
            EmitLogicalI(context, s ? context.Arm64Assembler.Orrs : context.Arm64Assembler.Orr, rd, rn, imm, immRotated, s);
        }

        public static void OrrR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitLogicalR(context, s ? context.Arm64Assembler.Orrs : context.Arm64Assembler.Orr, rd, rn, rm, sType, imm5, s);
        }

        public static void OrrRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitLogicalRr(context, s ? context.Arm64Assembler.Orrs : context.Arm64Assembler.Orr, rd, rn, rm, sType, rs, s);
        }

        public static void RsbI(CodeGenContext context, uint rd, uint rn, uint imm, bool s)
        {
            if (imm == 0)
            {
                Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
                Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

                if (s)
                {
                    context.Arm64Assembler.Negs(rdOperand, rnOperand);
                    context.SetNzcvModified();
                }
                else
                {
                    context.Arm64Assembler.Neg(rdOperand, rnOperand);
                }
            }
            else
            {
                EmitI(context, s ? context.Arm64Assembler.Subs : context.Arm64Assembler.Sub, rd, rn, imm, s, reverse: true);
            }
        }

        public static void RsbR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitR(context, s ? context.Arm64Assembler.Subs : context.Arm64Assembler.Sub, rd, rn, rm, sType, imm5, s, reverse: true);
        }

        public static void RsbRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitRr(context, s ? context.Arm64Assembler.Subs : context.Arm64Assembler.Sub, rd, rn, rm, sType, rs, s, reverse: true);
        }

        public static void RscI(CodeGenContext context, uint rd, uint rn, uint imm, bool s)
        {
            EmitI(context, s ? context.Arm64Assembler.Sbcs : context.Arm64Assembler.Sbc, rd, rn, imm, s, reverse: true);
        }

        public static void RscR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitR(context, s ? context.Arm64Assembler.Sbcs : context.Arm64Assembler.Sbc, rd, rn, rm, sType, imm5, s, reverse: true);
        }

        public static void RscRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitRr(context, s ? context.Arm64Assembler.Sbcs : context.Arm64Assembler.Sbc, rd, rn, rm, sType, rs, s, reverse: true);
        }

        public static void SbcI(CodeGenContext context, uint rd, uint rn, uint imm, bool s)
        {
            EmitI(context, s ? context.Arm64Assembler.Sbcs : context.Arm64Assembler.Sbc, rd, rn, imm, s);
        }

        public static void SbcR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitR(context, s ? context.Arm64Assembler.Sbcs : context.Arm64Assembler.Sbc, rd, rn, rm, sType, imm5, s);
        }

        public static void SbcRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitRr(context, s ? context.Arm64Assembler.Sbcs : context.Arm64Assembler.Sbc, rd, rn, rm, sType, rs, s);
        }

        public static void SubI(CodeGenContext context, uint rd, uint rn, uint imm, bool s)
        {
            EmitArithmeticI(context, s ? context.Arm64Assembler.Subs : context.Arm64Assembler.Sub, rd, rn, imm, s);
        }

        public static void SubR(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            EmitArithmeticR(context, s ? context.Arm64Assembler.Subs : context.Arm64Assembler.Sub, rd, rn, rm, sType, imm5, s);
        }

        public static void SubRr(CodeGenContext context, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            EmitRr(context, s ? context.Arm64Assembler.Subs : context.Arm64Assembler.Sub, rd, rn, rm, sType, rs, s);
        }

        public static void TeqI(CodeGenContext context, uint rn, uint imm, bool immRotated)
        {
            EmitLogicalI(context, (rnOperand, rmOperand) => EmitTeq(context, rnOperand, rmOperand), rn, imm, immRotated);
        }

        public static void TeqR(CodeGenContext context, uint rn, uint rm, uint sType, uint imm5)
        {
            EmitLogicalR(context, (rnOperand, rmOperand) => EmitTeq(context, rnOperand, rmOperand), rn, rm, sType, imm5);
        }

        public static void TeqRr(CodeGenContext context, uint rn, uint rm, uint sType, uint rs)
        {
            EmitLogicalRr(context, (rnOperand, rmOperand) => EmitTeq(context, rnOperand, rmOperand), rn, rm, sType, rs);
        }

        public static void TstI(CodeGenContext context, uint rn, uint imm, bool immRotated)
        {
            EmitLogicalI(context, context.Arm64Assembler.Tst, rn, imm, immRotated);
        }

        public static void TstR(CodeGenContext context, uint rn, uint rm, uint sType, uint imm5)
        {
            EmitLogicalR(context, context.Arm64Assembler.Tst, rn, rm, sType, imm5);
        }

        public static void TstRr(CodeGenContext context, uint rn, uint rm, uint sType, uint rs)
        {
            EmitLogicalRr(context, context.Arm64Assembler.Tst, rn, rm, sType, rs);
        }

        private static void EmitArithmeticI(CodeGenContext context, Action<Operand, Operand, Operand> action, uint rd, uint rn, uint imm, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            if (imm < Imm12Limit)
            {
                Operand rmOperand = new(OperandKind.Constant, OperandType.I32, imm);

                action(rdOperand, rnOperand, rmOperand);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Mov(tempRegister.Operand, imm);

                action(rdOperand, rnOperand, tempRegister.Operand);
            }

            if (s)
            {
                context.SetNzcvModified();
            }
        }

        private static void EmitArithmeticR(
            CodeGenContext context,
            Action<Operand, Operand, Operand, ArmShiftType, int> action,
            uint rd,
            uint rn,
            uint rm,
            uint sType,
            uint imm5,
            bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (CanShiftArithmetic(sType, imm5))
            {
                action(rdOperand, rnOperand, rmOperand, (ArmShiftType)sType, (int)imm5);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                rmOperand = GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType);

                action(rdOperand, rnOperand, rmOperand, ArmShiftType.Lsl, 0);
            }

            if (s)
            {
                context.SetNzcvModified();
            }
        }

        private static void EmitCompareI(CodeGenContext context, Action<Operand, Operand> action, uint rn, uint imm)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            if (imm < Imm12Limit)
            {
                Operand rmOperand = new(OperandKind.Constant, OperandType.I32, imm);

                action(rnOperand, rmOperand);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Mov(tempRegister.Operand, imm);

                action(rnOperand, tempRegister.Operand);
            }

            context.SetNzcvModified();
        }

        private static void EmitCompareR(
            CodeGenContext context,
            Action<Operand, Operand, ArmShiftType, int> action,
            uint rn,
            uint rm,
            uint sType,
            uint imm5)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (CanShiftArithmetic(sType, imm5))
            {
                action(rnOperand, rmOperand, (ArmShiftType)sType, (int)imm5);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                rmOperand = GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType);

                action(rnOperand, rmOperand, ArmShiftType.Lsl, 0);
            }

            context.SetNzcvModified();
        }

        private static void EmitCompareRr(CodeGenContext context, Action<Operand, Operand> action, uint rn, uint rm, uint sType, uint rs)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand rsOperand = InstEmitCommon.GetInputGpr(context, rs);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            rmOperand = GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType);

            action(rnOperand, rmOperand);

            context.SetNzcvModified();
        }

        private static void EmitI(CodeGenContext context, Action<Operand, Operand, Operand> action, uint rd, uint rn, uint imm, bool s, bool reverse = false)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.Mov(tempRegister.Operand, imm);

            if (reverse)
            {
                action(rdOperand, tempRegister.Operand, rnOperand);
            }
            else
            {
                action(rdOperand, rnOperand, tempRegister.Operand);
            }

            if (s)
            {
                context.SetNzcvModified();
            }
        }

        private static void EmitR(CodeGenContext context, Action<Operand, Operand, Operand> action, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s, bool reverse = false)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            rmOperand = GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType);

            if (reverse)
            {
                action(rdOperand, rmOperand, rnOperand);
            }
            else
            {
                action(rdOperand, rnOperand, rmOperand);
            }

            if (s)
            {
                context.SetNzcvModified();
            }
        }

        private static void EmitRr(CodeGenContext context, Action<Operand, Operand, Operand> action, uint rd, uint rn, uint rm, uint sType, uint rs, bool s, bool reverse = false)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand rsOperand = InstEmitCommon.GetInputGpr(context, rs);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            rmOperand = GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType);

            if (reverse)
            {
                action(rdOperand, rmOperand, rnOperand);
            }
            else
            {
                action(rdOperand, rnOperand, rmOperand);
            }

            if (s)
            {
                context.SetNzcvModified();
            }
        }

        private static void EmitLogicalI(CodeGenContext context, Action<Operand, Operand> action, uint rn, uint imm, bool immRotated)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

            if (immRotated)
            {
                if ((imm & (1u << 31)) != 0)
                {
                    context.Arm64Assembler.Orr(flagsRegister.Operand, flagsRegister.Operand, InstEmitCommon.Const(2));
                }
                else
                {
                    context.Arm64Assembler.Bfc(flagsRegister.Operand, 1, 1);
                }
            }

            if (CodeGenCommon.TryEncodeBitMask(OperandType.I32, imm, out _, out _, out _))
            {
                action(rnOperand, InstEmitCommon.Const((int)imm));
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Mov(tempRegister.Operand, imm);

                action(rnOperand, tempRegister.Operand);
            }

            InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

            context.SetNzcvModified();
        }

        private static void EmitLogicalI(
            CodeGenContext context,
            Action<Operand, Operand, Operand> action,
            uint rd,
            uint rn,
            uint imm,
            bool immRotated,
            bool s,
            bool immForm = true)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            ScopedRegister flagsRegister = default;

            if (s)
            {
                flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                if (immRotated)
                {
                    if ((imm & (1u << 31)) != 0)
                    {
                        context.Arm64Assembler.Orr(flagsRegister.Operand, flagsRegister.Operand, InstEmitCommon.Const(2));
                    }
                    else
                    {
                        context.Arm64Assembler.Bfc(flagsRegister.Operand, 1, 1);
                    }
                }
            }

            if (imm == 0 || (immForm && CodeGenCommon.TryEncodeBitMask(OperandType.I32, imm, out _, out _, out _)))
            {
                action(rdOperand, rnOperand, InstEmitCommon.Const((int)imm));
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Mov(tempRegister.Operand, imm);

                action(rdOperand, rnOperand, tempRegister.Operand);
            }

            if (s)
            {
                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                flagsRegister.Dispose();

                context.SetNzcvModified();
            }
        }

        private static void EmitLogicalR(CodeGenContext context, Action<Operand, Operand> action, uint rn, uint rm, uint sType, uint imm5)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

            rmOperand = GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, flagsRegister.Operand);

            action(rnOperand, rmOperand);

            InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

            context.SetNzcvModified();
        }

        private static void EmitLogicalR(CodeGenContext context, Action<Operand, Operand, Operand, ArmShiftType, int> action, uint rd, uint rn, uint rm, uint sType, uint imm5, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (CanShift(sType, imm5) && !s)
            {
                action(rdOperand, rnOperand, rmOperand, (ArmShiftType)sType, (int)imm5);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
                ScopedRegister flagsRegister = default;

                if (s)
                {
                    flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                    rmOperand = GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, flagsRegister.Operand);
                }
                else
                {
                    rmOperand = GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, null);
                }

                action(rdOperand, rnOperand, rmOperand, ArmShiftType.Lsl, 0);

                if (s)
                {
                    InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                    flagsRegister.Dispose();

                    context.SetNzcvModified();
                }
            }
        }

        private static void EmitLogicalRr(CodeGenContext context, Action<Operand, Operand> action, uint rn, uint rm, uint sType, uint rs)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand rsOperand = InstEmitCommon.GetInputGpr(context, rs);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

            rmOperand = GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType, flagsRegister.Operand);

            action(rnOperand, rmOperand);

            InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

            context.SetNzcvModified();
        }

        private static void EmitLogicalRr(CodeGenContext context, Action<Operand, Operand, Operand> action, uint rd, uint rn, uint rm, uint sType, uint rs, bool s)
        {
            if (s)
            {
                Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
                Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
                Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
                Operand rsOperand = InstEmitCommon.GetInputGpr(context, rs);

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
                using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                rmOperand = GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType, flagsRegister.Operand);

                action(rdOperand, rnOperand, rmOperand);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                context.SetNzcvModified();
            }
            else
            {
                EmitRr(context, action, rd, rn, rm, sType, rs, s);
            }
        }

        public static bool CanShiftArithmetic(uint sType, uint imm5)
        {
            // We can't encode ROR or RRX.

            return sType != 3 && (sType == 0 || imm5 != 0);
        }

        public static bool CanShift(uint sType, uint imm5)
        {
            // We can encode all shift types directly, except RRX.

            return imm5 != 0 || sType == 0;
        }

        public static Operand GetMShiftedByImmediate(CodeGenContext context, Operand dest, Operand m, uint imm, uint sType, Operand? carryOut = null)
        {
            int shift = (int)imm;

            if (shift == 0)
            {
                switch ((ArmShiftType)sType)
                {
                    case ArmShiftType.Lsr:
                        shift = 32;
                        break;
                    case ArmShiftType.Asr:
                        shift = 32;
                        break;
                    case ArmShiftType.Ror:
                        shift = 1;
                        break;
                }
            }

            if (shift != 0)
            {
                switch ((ArmShiftType)sType)
                {
                    case ArmShiftType.Lsl:
                        m = GetLslC(context, dest, m, carryOut, shift);
                        break;
                    case ArmShiftType.Lsr:
                        m = GetLsrC(context, dest, m, carryOut, shift);
                        break;
                    case ArmShiftType.Asr:
                        m = GetAsrC(context, dest, m, carryOut, shift);
                        break;
                    case ArmShiftType.Ror:
                        if (imm != 0)
                        {
                            m = GetRorC(context, dest, m, carryOut, shift);
                        }
                        else
                        {
                            m = GetRrxC(context, dest, m, carryOut);
                        }
                        break;
                }
            }

            return m;
        }

        public static Operand GetMShiftedByReg(CodeGenContext context, Operand dest, Operand m, Operand s, uint sType, Operand? carryOut = null)
        {
            Operand shiftResult = m;

            switch ((ArmShiftType)sType)
            {
                case ArmShiftType.Lsl:
                    shiftResult = EmitLslC(context, dest, m, carryOut, s);
                    break;
                case ArmShiftType.Lsr:
                    shiftResult = EmitLsrC(context, dest, m, carryOut, s);
                    break;
                case ArmShiftType.Asr:
                    shiftResult = EmitAsrC(context, dest, m, carryOut, s);
                    break;
                case ArmShiftType.Ror:
                    shiftResult = EmitRorC(context, dest, m, carryOut, s);
                    break;
            }

            return shiftResult;
        }

        private static void EmitIfHelper(CodeGenContext context, Operand boolValue, Action action, bool expected = true)
        {
            Debug.Assert(boolValue.Type == OperandType.I32);

            int branchInstructionPointer = context.CodeWriter.InstructionPointer;

            if (expected)
            {
                context.Arm64Assembler.Cbnz(boolValue, 0);
            }
            else
            {
                context.Arm64Assembler.Cbz(boolValue, 0);
            }

            action();

            int offset = context.CodeWriter.InstructionPointer - branchInstructionPointer;
            Debug.Assert(offset >= 0);
            Debug.Assert((offset << 13) >> 13 == offset);
            uint branchInst = context.CodeWriter.ReadInstructionAt(branchInstructionPointer);
            context.CodeWriter.WriteInstructionAt(branchInstructionPointer, branchInst | (uint)(offset << 5));
        }

        private static Operand EmitLslC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, Operand shift)
        {
            Debug.Assert(m.Type == OperandType.I32 && shift.Type == OperandType.I32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand mask = tempRegister.Operand;
            context.Arm64Assembler.Uxtb(mask, shift);
            context.Arm64Assembler.Sub(mask, mask, InstEmitCommon.Const(32));
            context.Arm64Assembler.Asr(mask, mask, InstEmitCommon.Const(31));

            Operand dest64 = new(OperandKind.Register, OperandType.I64, dest.Value);

            if (carryOut.HasValue)
            {
                context.Arm64Assembler.Lslv(dest64, m, shift);
            }
            else
            {
                context.Arm64Assembler.Lslv(dest, m, shift);
            }

            // If shift >= 32, force the result to 0.
            context.Arm64Assembler.And(dest, dest, mask);

            if (carryOut.HasValue)
            {
                EmitIfHelper(context, shift, () =>
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.Uxtb(mask, shift);
                    context.Arm64Assembler.Sub(mask, mask, InstEmitCommon.Const(33));
                    context.Arm64Assembler.Lsr(mask, mask, InstEmitCommon.Const(31));
                    context.Arm64Assembler.Lsr(tempRegister.Operand, dest64, InstEmitCommon.Const(32));
                    context.Arm64Assembler.And(tempRegister.Operand, tempRegister.Operand, mask);

                    UpdateCarryFlag(context, tempRegister.Operand, carryOut.Value);
                }, false);
            }

            return dest;
        }

        private static Operand GetLslC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, int shift)
        {
            Debug.Assert(m.Type == OperandType.I32);

            if ((uint)shift > 32)
            {
                return GetShiftByMoreThan32(context, carryOut);
            }
            else if (shift == 32)
            {
                if (carryOut.HasValue)
                {
                    SetCarryMLsb(context, m, carryOut.Value);
                }

                return InstEmitCommon.Const(0);
            }
            else
            {
                if (carryOut.HasValue)
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.Lsr(tempRegister.Operand, m, InstEmitCommon.Const(32 - shift));

                    UpdateCarryFlag(context, tempRegister.Operand, carryOut.Value);
                }

                context.Arm64Assembler.Lsl(dest, m, InstEmitCommon.Const(shift));

                return dest;
            }
        }

        private static Operand EmitLsrC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, Operand shift)
        {
            Debug.Assert(m.Type == OperandType.I32 && shift.Type == OperandType.I32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand mask = tempRegister.Operand;
            context.Arm64Assembler.Uxtb(mask, shift);
            context.Arm64Assembler.Sub(mask, mask, InstEmitCommon.Const(32));
            context.Arm64Assembler.Asr(mask, mask, InstEmitCommon.Const(31));

            context.Arm64Assembler.Lsrv(dest, m, shift);

            // If shift >= 32, force the result to 0.
            context.Arm64Assembler.And(dest, dest, mask);

            if (carryOut.HasValue)
            {
                EmitIfHelper(context, shift, () =>
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.Uxtb(mask, shift);
                    context.Arm64Assembler.Sub(mask, mask, InstEmitCommon.Const(33));
                    context.Arm64Assembler.Lsr(mask, mask, InstEmitCommon.Const(31));
                    context.Arm64Assembler.Sub(tempRegister.Operand, shift, InstEmitCommon.Const(1));
                    context.Arm64Assembler.Lsrv(tempRegister.Operand, m, tempRegister.Operand);
                    context.Arm64Assembler.And(tempRegister.Operand, tempRegister.Operand, mask);

                    UpdateCarryFlag(context, tempRegister.Operand, carryOut.Value);
                }, false);
            }

            return dest;
        }

        public static Operand GetLsrC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, int shift)
        {
            Debug.Assert(m.Type == OperandType.I32);

            if ((uint)shift > 32)
            {
                return GetShiftByMoreThan32(context, carryOut);
            }
            else if (shift == 32)
            {
                if (carryOut.HasValue)
                {
                    SetCarryMMsb(context, m, carryOut.Value);
                }

                return InstEmitCommon.Const(0);
            }
            else
            {
                if (carryOut.HasValue)
                {
                    SetCarryMShrOut(context, m, shift, carryOut.Value);
                }

                context.Arm64Assembler.Lsr(dest, m, InstEmitCommon.Const(shift));

                return dest;
            }
        }

        private static Operand GetShiftByMoreThan32(CodeGenContext context, Operand? carryOut)
        {
            if (carryOut.HasValue)
            {
                // Clear carry flag.

                context.Arm64Assembler.Bfc(carryOut.Value, 1, 1);
            }

            return InstEmitCommon.Const(0);
        }

        private static Operand EmitAsrC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, Operand shift)
        {
            Debug.Assert(m.Type == OperandType.I32 && shift.Type == OperandType.I32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand mask = tempRegister.Operand;
            context.Arm64Assembler.Uxtb(mask, shift);
            context.Arm64Assembler.Sub(mask, mask, InstEmitCommon.Const(31));
            context.Arm64Assembler.Orn(mask, shift, mask, ArmShiftType.Asr, 31);

            context.Arm64Assembler.Asrv(dest, m, mask);

            if (carryOut.HasValue)
            {
                EmitIfHelper(context, shift, () =>
                {
                    // If shift >= 32, carry should be equal to the MSB of Rm.

                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.Sub(tempRegister.Operand, mask, InstEmitCommon.Const(1));
                    context.Arm64Assembler.Orr(tempRegister.Operand, tempRegister.Operand, mask, ArmShiftType.Asr, 31);
                    context.Arm64Assembler.Lsrv(tempRegister.Operand, m, tempRegister.Operand);

                    UpdateCarryFlag(context, tempRegister.Operand, carryOut.Value);
                }, false);
            }

            return dest;
        }

        private static Operand GetAsrC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, int shift)
        {
            Debug.Assert(m.Type == OperandType.I32);

            if ((uint)shift >= 32)
            {
                context.Arm64Assembler.Asr(dest, m, InstEmitCommon.Const(31));

                if (carryOut.HasValue)
                {
                    SetCarryMLsb(context, dest, carryOut.Value);
                }

                return dest;
            }
            else
            {
                if (carryOut.HasValue)
                {
                    SetCarryMShrOut(context, m, shift, carryOut.Value);
                }

                context.Arm64Assembler.Asr(dest, m, InstEmitCommon.Const(shift));

                return dest;
            }
        }

        private static Operand EmitRorC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, Operand shift)
        {
            Debug.Assert(m.Type == OperandType.I32 && shift.Type == OperandType.I32);

            context.Arm64Assembler.Rorv(dest, m, shift);

            if (carryOut.HasValue)
            {
                EmitIfHelper(context, shift, () =>
                {
                    SetCarryMMsb(context, m, carryOut.Value);
                }, false);
            }

            return dest;
        }

        private static Operand GetRorC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut, int shift)
        {
            Debug.Assert(m.Type == OperandType.I32);

            shift &= 0x1f;

            context.Arm64Assembler.Ror(dest, m, InstEmitCommon.Const(shift));

            if (carryOut.HasValue)
            {
                SetCarryMMsb(context, dest, carryOut.Value);
            }

            return dest;
        }

        private static Operand GetRrxC(CodeGenContext context, Operand dest, Operand m, Operand? carryOut)
        {
            Debug.Assert(m.Type == OperandType.I32);

            // Rotate right by 1 with carry.

            if (carryOut.HasValue)
            {
                SetCarryMLsb(context, m, carryOut.Value);
            }

            context.Arm64Assembler.Mov(dest, m);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.MrsNzcv(tempRegister.Operand);
            context.Arm64Assembler.Bfxil(dest, tempRegister.Operand, 29, 1);
            context.Arm64Assembler.Ror(dest, dest, InstEmitCommon.Const(1));

            return dest;
        }

        private static void SetCarryMLsb(CodeGenContext context, Operand m, Operand carryOut)
        {
            Debug.Assert(m.Type == OperandType.I32);

            UpdateCarryFlag(context, m, carryOut);
        }

        private static void SetCarryMMsb(CodeGenContext context, Operand m, Operand carryOut)
        {
            Debug.Assert(m.Type == OperandType.I32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.Lsr(tempRegister.Operand, m, InstEmitCommon.Const(31));

            UpdateCarryFlag(context, tempRegister.Operand, carryOut);
        }

        private static void SetCarryMShrOut(CodeGenContext context, Operand m, int shift, Operand carryOut)
        {
            Debug.Assert(m.Type == OperandType.I32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.Lsr(tempRegister.Operand, m, InstEmitCommon.Const(shift - 1));

            UpdateCarryFlag(context, tempRegister.Operand, carryOut);
        }

        private static void UpdateCarryFlag(CodeGenContext context, Operand value, Operand carryOut)
        {
            context.Arm64Assembler.Bfi(carryOut, value, 1, 1);
        }

        private static void EmitTeq(CodeGenContext context, Operand rnOperand, Operand rmOperand)
        {
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.Eors(tempRegister.Operand, rnOperand, rmOperand);
        }
    }
}
