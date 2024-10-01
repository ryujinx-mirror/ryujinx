using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitCommon
    {
        public static Operand Const(int value)
        {
            return new(OperandKind.Constant, OperandType.I32, (uint)value);
        }

        public static Operand GetInputGpr(CodeGenContext context, uint register)
        {
            Operand operand = context.RegisterAllocator.RemapGprRegister((int)register);

            if (register == RegisterUtils.PcRegister)
            {
                context.Arm64Assembler.Mov(operand, context.Pc);
            }

            return operand;
        }

        public static Operand GetOutputGpr(CodeGenContext context, uint register)
        {
            return context.RegisterAllocator.RemapGprRegister((int)register);
        }

        public static void GetCurrentFlags(CodeGenContext context, Operand flagsOut)
        {
            context.Arm64Assembler.MrsNzcv(flagsOut);
            context.Arm64Assembler.Lsr(flagsOut, flagsOut, Const(28));
        }

        public static void RestoreNzcvFlags(CodeGenContext context, Operand nzcvFlags)
        {
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.Lsl(tempRegister.Operand, nzcvFlags, Const(28));
            context.Arm64Assembler.MsrNzcv(tempRegister.Operand);
        }

        public static void RestoreCvFlags(CodeGenContext context, Operand cvFlags)
        {
            // Arm64 zeros the carry and overflow flags for logical operations, but Arm32 keeps them unchanged.
            // This will restore carry and overflow after a operation has zeroed them.

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.MrsNzcv(tempRegister.Operand);
            context.Arm64Assembler.Bfi(tempRegister.Operand, cvFlags, 28, 2);
            context.Arm64Assembler.MsrNzcv(tempRegister.Operand);
        }

        public static void SetThumbFlag(CodeGenContext context)
        {
            Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.LdrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
            context.Arm64Assembler.Orr(tempRegister.Operand, tempRegister.Operand, Const(1 << 5));
            context.Arm64Assembler.StrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
        }

        public static void SetThumbFlag(CodeGenContext context, Operand value)
        {
            Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.LdrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
            context.Arm64Assembler.Bfi(tempRegister.Operand, value, 5, 1);
            context.Arm64Assembler.StrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
        }

        public static void ClearThumbFlag(CodeGenContext context)
        {
            Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.LdrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
            context.Arm64Assembler.Bfc(tempRegister.Operand, 5, 1);
            context.Arm64Assembler.StrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
        }

        public static void EmitSigned16BitPair(CodeGenContext context, uint rd, uint rn, Action<Operand, Operand> elementAction)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = GetOutputGpr(context, rd);
            Operand rnOperand = GetInputGpr(context, rn);

            context.Arm64Assembler.Sxth(tempN.Operand, rnOperand);
            elementAction(tempD.Operand, tempN.Operand);
            context.Arm64Assembler.Uxth(tempD2.Operand, tempD.Operand);

            context.Arm64Assembler.Asr(tempN.Operand, rnOperand, Const(16));
            elementAction(tempD.Operand, tempN.Operand);
            context.Arm64Assembler.Orr(rdOperand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 16);
        }

        public static void EmitSigned16BitPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand> elementAction)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = GetOutputGpr(context, rd);
            Operand rnOperand = GetInputGpr(context, rn);
            Operand rmOperand = GetInputGpr(context, rm);

            context.Arm64Assembler.Sxth(tempN.Operand, rnOperand);
            context.Arm64Assembler.Sxth(tempM.Operand, rmOperand);
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand);
            context.Arm64Assembler.Uxth(tempD2.Operand, tempD.Operand);

            context.Arm64Assembler.Asr(tempN.Operand, rnOperand, Const(16));
            context.Arm64Assembler.Asr(tempM.Operand, rmOperand, Const(16));
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand);
            context.Arm64Assembler.Orr(rdOperand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 16);
        }

        public static void EmitSigned16BitXPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand, int> elementAction)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = GetOutputGpr(context, rd);
            Operand rnOperand = GetInputGpr(context, rn);
            Operand rmOperand = GetInputGpr(context, rm);

            context.Arm64Assembler.Sxth(tempN.Operand, rnOperand);
            context.Arm64Assembler.Asr(tempM.Operand, rmOperand, Const(16));
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand, 0);
            context.Arm64Assembler.Uxth(tempD2.Operand, tempD.Operand);

            context.Arm64Assembler.Asr(tempN.Operand, rnOperand, Const(16));
            context.Arm64Assembler.Sxth(tempM.Operand, rmOperand);
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand, 1);
            context.Arm64Assembler.Orr(rdOperand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 16);
        }

        public static void EmitSigned8BitPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand> elementAction)
        {
            Emit8BitPair(context, rd, rn, rm, elementAction, unsigned: false);
        }

        public static void EmitUnsigned16BitPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand> elementAction)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = GetOutputGpr(context, rd);
            Operand rnOperand = GetInputGpr(context, rn);
            Operand rmOperand = GetInputGpr(context, rm);

            context.Arm64Assembler.Uxth(tempN.Operand, rnOperand);
            context.Arm64Assembler.Uxth(tempM.Operand, rmOperand);
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand);
            context.Arm64Assembler.Uxth(tempD2.Operand, tempD.Operand);

            context.Arm64Assembler.Lsr(tempN.Operand, rnOperand, Const(16));
            context.Arm64Assembler.Lsr(tempM.Operand, rmOperand, Const(16));
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand);
            context.Arm64Assembler.Orr(rdOperand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 16);
        }

        public static void EmitUnsigned16BitXPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand, int> elementAction)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = GetOutputGpr(context, rd);
            Operand rnOperand = GetInputGpr(context, rn);
            Operand rmOperand = GetInputGpr(context, rm);

            context.Arm64Assembler.Uxth(tempN.Operand, rnOperand);
            context.Arm64Assembler.Lsr(tempM.Operand, rmOperand, Const(16));
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand, 0);
            context.Arm64Assembler.Uxth(tempD2.Operand, tempD.Operand);

            context.Arm64Assembler.Lsr(tempN.Operand, rnOperand, Const(16));
            context.Arm64Assembler.Uxth(tempM.Operand, rmOperand);
            elementAction(tempD.Operand, tempN.Operand, tempM.Operand, 1);
            context.Arm64Assembler.Orr(rdOperand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 16);
        }

        public static void EmitUnsigned8BitPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand> elementAction)
        {
            Emit8BitPair(context, rd, rn, rm, elementAction, unsigned: true);
        }

        private static void Emit8BitPair(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand> elementAction, bool unsigned)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = GetOutputGpr(context, rd);
            Operand rnOperand = GetInputGpr(context, rn);
            Operand rmOperand = GetInputGpr(context, rm);

            for (int b = 0; b < 4; b++)
            {
                if (unsigned)
                {
                    context.Arm64Assembler.Ubfx(tempN.Operand, rnOperand, b * 8, 8);
                    context.Arm64Assembler.Ubfx(tempM.Operand, rmOperand, b * 8, 8);
                }
                else
                {
                    context.Arm64Assembler.Sbfx(tempN.Operand, rnOperand, b * 8, 8);
                    context.Arm64Assembler.Sbfx(tempM.Operand, rmOperand, b * 8, 8);
                }

                elementAction(tempD.Operand, tempN.Operand, tempM.Operand);

                if (b == 0)
                {
                    context.Arm64Assembler.Uxtb(tempD2.Operand, tempD.Operand);
                }
                else if (b < 3)
                {
                    context.Arm64Assembler.Uxtb(tempD.Operand, tempD.Operand);
                    context.Arm64Assembler.Orr(tempD2.Operand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, b * 8);
                }
                else
                {
                    context.Arm64Assembler.Orr(rdOperand, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 24);
                }
            }
        }

        public static uint CombineV(uint low4, uint high1, uint size)
        {
            return size == 3 ? CombineV(low4, high1) : CombineVF(high1, low4);
        }

        public static uint CombineV(uint low4, uint high1)
        {
            return low4 | (high1 << 4);
        }

        public static uint CombineVF(uint low1, uint high4)
        {
            return low1 | (high4 << 1);
        }
    }
}
