using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitMove
    {
        public static void MvnI(CodeGenContext context, uint rd, uint imm, bool immRotated, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);

            if (s)
            {
                using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                if (immRotated)
                {
                    if ((imm & (1u << 31)) != 0)
                    {
                        context.Arm64Assembler.Orr(flagsRegister.Operand, flagsRegister.Operand, InstEmitCommon.Const(1 << 29));
                    }
                    else
                    {
                        context.Arm64Assembler.Bfc(flagsRegister.Operand, 29, 1);
                    }
                }

                context.Arm64Assembler.Mov(rdOperand, ~imm);
                context.Arm64Assembler.Tst(rdOperand, rdOperand);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                context.SetNzcvModified();
            }
            else
            {
                context.Arm64Assembler.Mov(rdOperand, ~imm);
            }
        }

        public static void MvnR(CodeGenContext context, uint rd, uint rm, uint sType, uint imm5, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            ScopedRegister flagsRegister = default;

            if (s)
            {
                flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                rmOperand = InstEmitAlu.GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, flagsRegister.Operand);
            }
            else
            {
                rmOperand = InstEmitAlu.GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType);
            }

            context.Arm64Assembler.Mvn(rdOperand, rmOperand);

            if (s)
            {
                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                flagsRegister.Dispose();

                context.SetNzcvModified();
            }
        }

        public static void MvnRr(CodeGenContext context, uint rd, uint rm, uint sType, uint rs, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand rsOperand = InstEmitCommon.GetInputGpr(context, rs);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            ScopedRegister flagsRegister = default;

            if (s)
            {
                flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                rmOperand = InstEmitAlu.GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType, flagsRegister.Operand);
            }
            else
            {
                rmOperand = InstEmitAlu.GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType);
            }

            context.Arm64Assembler.Mvn(rdOperand, rmOperand);

            if (s)
            {
                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                flagsRegister.Dispose();

                context.SetNzcvModified();
            }
        }

        public static void MovI(CodeGenContext context, uint rd, uint imm, bool immRotated, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);

            if (s)
            {
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

                context.Arm64Assembler.Mov(rdOperand, imm);
                context.Arm64Assembler.Tst(rdOperand, rdOperand);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                context.SetNzcvModified();
            }
            else
            {
                context.Arm64Assembler.Mov(rdOperand, imm);
            }
        }

        public static void MovR(CodeGenContext context, uint rd, uint rm, uint sType, uint imm5, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (InstEmitAlu.CanShift(sType, imm5) && !s)
            {
                if (imm5 != 0)
                {
                    switch ((ArmShiftType)sType)
                    {
                        case ArmShiftType.Lsl:
                            context.Arm64Assembler.Lsl(rdOperand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                        case ArmShiftType.Lsr:
                            context.Arm64Assembler.Lsr(rdOperand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                        case ArmShiftType.Asr:
                            context.Arm64Assembler.Asr(rdOperand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                        case ArmShiftType.Ror:
                            context.Arm64Assembler.Ror(rdOperand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                    }
                }
                else
                {
                    context.Arm64Assembler.Mov(rdOperand, rmOperand);
                }
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
                ScopedRegister flagsRegister = default;

                if (s)
                {
                    flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                    rmOperand = InstEmitAlu.GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, flagsRegister.Operand);
                }
                else
                {
                    rmOperand = InstEmitAlu.GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, null);
                }

                context.Arm64Assembler.Mov(rdOperand, rmOperand);

                if (s)
                {
                    context.Arm64Assembler.Tst(rdOperand, rdOperand);

                    InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                    flagsRegister.Dispose();

                    context.SetNzcvModified();
                }
            }
        }

        public static void MovR(CodeGenContext context, uint cond, uint rd, uint rm, uint sType, uint imm5, bool s)
        {
            if (context.ConsumeSkipNextInstruction())
            {
                return;
            }

            if ((ArmCondition)cond >= ArmCondition.Al || s)
            {
                MovR(context, rd, rm, sType, imm5, s);

                return;
            }

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (InstEmitAlu.CanShift(sType, imm5))
            {
                if (imm5 != 0)
                {
                    switch ((ArmShiftType)sType)
                    {
                        case ArmShiftType.Lsl:
                            context.Arm64Assembler.Lsl(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                        case ArmShiftType.Lsr:
                            context.Arm64Assembler.Lsr(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                        case ArmShiftType.Asr:
                            context.Arm64Assembler.Asr(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                        case ArmShiftType.Ror:
                            context.Arm64Assembler.Ror(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)imm5));
                            break;
                    }

                    context.Arm64Assembler.Csel(rdOperand, tempRegister.Operand, rdOperand, (ArmCondition)cond);
                }
                else
                {
                    Operand other = rdOperand;

                    InstInfo nextInstruction = context.PeekNextInstruction();

                    if (nextInstruction.Name == InstName.MovR)
                    {
                        // If this instruction is followed by another move with the inverse condition,
                        // we can just put it into the second operand of the CSEL instruction and skip the next move.

                        InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 nextInst = new(nextInstruction.Encoding);

                        if (nextInst.Rd == rd &&
                            nextInst.S == 0 &&
                            nextInst.Stype == 0 &&
                            nextInst.Imm5 == 0 &&
                            nextInst.Cond == (cond ^ 1u) &&
                            nextInst.Rm != RegisterUtils.PcRegister)
                        {
                            other = InstEmitCommon.GetInputGpr(context, nextInst.Rm);
                            context.SetSkipNextInstruction();
                        }
                    }

                    context.Arm64Assembler.Csel(rdOperand, rmOperand, other, (ArmCondition)cond);
                }
            }
            else
            {
                rmOperand = InstEmitAlu.GetMShiftedByImmediate(context, tempRegister.Operand, rmOperand, imm5, sType, null);

                context.Arm64Assembler.Csel(rdOperand, rmOperand, rdOperand, (ArmCondition)cond);
            }
        }

        public static void MovRr(CodeGenContext context, uint rd, uint rm, uint sType, uint rs, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand rsOperand = InstEmitCommon.GetInputGpr(context, rs);

            if (!s)
            {
                InstEmitAlu.GetMShiftedByReg(context, rdOperand, rmOperand, rsOperand, sType);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
                using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                rmOperand = InstEmitAlu.GetMShiftedByReg(context, tempRegister.Operand, rmOperand, rsOperand, sType, flagsRegister.Operand);

                context.Arm64Assembler.Mov(rdOperand, rmOperand);
                context.Arm64Assembler.Tst(rdOperand, rdOperand);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                context.SetNzcvModified();
            }
        }

        public static void Movt(CodeGenContext context, uint rd, uint imm)
        {
            Operand rdOperand = InstEmitCommon.GetInputGpr(context, rd);

            context.Arm64Assembler.Movk(rdOperand, (int)imm, 1);
        }

        public static void Pkh(CodeGenContext context, uint rd, uint rn, uint rm, bool tb, uint imm5)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (!tb && imm5 == 0)
            {
                context.Arm64Assembler.Extr(rdOperand, rnOperand, rmOperand, 16);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                if (tb)
                {
                    context.Arm64Assembler.Asr(tempRegister.Operand, rmOperand, InstEmitCommon.Const(imm5 == 0 ? 31 : (int)imm5));
                    context.Arm64Assembler.Extr(rdOperand, tempRegister.Operand, rnOperand, 16);
                }
                else
                {
                    context.Arm64Assembler.Lsl(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)imm5));
                    context.Arm64Assembler.Extr(rdOperand, rnOperand, tempRegister.Operand, 16);
                }
            }

            context.Arm64Assembler.Ror(rdOperand, rdOperand, InstEmitCommon.Const(16));
        }
    }
}
