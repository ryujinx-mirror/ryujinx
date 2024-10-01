using Ryujinx.Cpu.LightningJit.CodeGen;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonSystem
    {
        public static void Vmrs(CodeGenContext context, uint rt, uint reg)
        {
            if (context.ConsumeSkipNextInstruction())
            {
                // This case means that we managed to combine a VCMP and VMRS instruction,
                // so we have nothing to do here as FCMP/FCMPE already set PSTATE.NZCV.
                context.SetNzcvModified();

                return;
            }

            if (reg == 1)
            {
                // FPSCR

                Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

                if (rt == RegisterUtils.PcRegister)
                {
                    using ScopedRegister fpsrRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.LdrRiUn(fpsrRegister.Operand, ctx, NativeContextOffsets.FpFlagsBaseOffset);
                    context.Arm64Assembler.Lsr(fpsrRegister.Operand, fpsrRegister.Operand, InstEmitCommon.Const(28));

                    InstEmitCommon.RestoreNzcvFlags(context, fpsrRegister.Operand);

                    context.SetNzcvModified();
                }
                else
                {
                    // FPSCR is a combination of the FPCR and FPSR registers.
                    // We also need to set the FPSR NZCV bits that no longer exist on AArch64.

                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);

                    context.Arm64Assembler.MrsFpsr(rtOperand);
                    context.Arm64Assembler.MrsFpcr(tempRegister.Operand);
                    context.Arm64Assembler.Orr(rtOperand, rtOperand, tempRegister.Operand);
                    context.Arm64Assembler.LdrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FpFlagsBaseOffset);
                    context.Arm64Assembler.Bfc(tempRegister.Operand, 0, 28);
                    context.Arm64Assembler.Orr(rtOperand, rtOperand, tempRegister.Operand);
                }
            }
            else
            {
                Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);

                context.Arm64Assembler.Mov(rtOperand, 0u);
            }
        }

        public static void Vmsr(CodeGenContext context, uint rt, uint reg)
        {
            if (reg == 1)
            {
                // FPSCR

                // TODO: Do not set bits related to features that are not supported (like FP16)?

                Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);
                Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);

                context.Arm64Assembler.MsrFpcr(rtOperand);
                context.Arm64Assembler.MsrFpsr(rtOperand);
                context.Arm64Assembler.StrRiUn(rtOperand, ctx, NativeContextOffsets.FpFlagsBaseOffset);
            }
        }
    }
}
