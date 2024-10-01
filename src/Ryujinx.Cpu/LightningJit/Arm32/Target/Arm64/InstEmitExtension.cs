using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitExtension
    {
        public static void Sxtab(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate)
        {
            EmitRotated(context, ArmExtensionType.Sxtb, rd, rn, rm, rotate);
        }

        public static void Sxtab16(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate)
        {
            EmitExtendAccumulate8(context, rd, rn, rm, rotate, unsigned: false);
        }

        public static void Sxtah(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate)
        {
            EmitRotated(context, ArmExtensionType.Sxth, rd, rn, rm, rotate);
        }

        public static void Sxtb(CodeGenContext context, uint rd, uint rm, uint rotate)
        {
            EmitRotated(context, context.Arm64Assembler.Sxtb, rd, rm, rotate);
        }

        public static void Sxtb16(CodeGenContext context, uint rd, uint rm, uint rotate)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempRegister2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            if (rotate != 0)
            {
                context.Arm64Assembler.Ror(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)rotate * 8));
                context.Arm64Assembler.And(rdOperand, tempRegister.Operand, InstEmitCommon.Const(0xff00ff));
            }
            else
            {
                context.Arm64Assembler.And(rdOperand, rmOperand, InstEmitCommon.Const(0xff00ff));
            }

            // Sign-extend by broadcasting sign bits.
            context.Arm64Assembler.And(tempRegister.Operand, rdOperand, InstEmitCommon.Const(0x800080));
            context.Arm64Assembler.Lsl(tempRegister2.Operand, tempRegister.Operand, InstEmitCommon.Const(9));
            context.Arm64Assembler.Sub(tempRegister.Operand, tempRegister2.Operand, tempRegister.Operand);
            context.Arm64Assembler.Orr(rdOperand, rdOperand, tempRegister.Operand);
        }

        public static void Sxth(CodeGenContext context, uint rd, uint rm, uint rotate)
        {
            EmitRotated(context, context.Arm64Assembler.Sxth, rd, rm, rotate);
        }

        public static void Uxtab(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate)
        {
            EmitRotated(context, ArmExtensionType.Uxtb, rd, rn, rm, rotate);
        }

        public static void Uxtab16(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate)
        {
            EmitExtendAccumulate8(context, rd, rn, rm, rotate, unsigned: true);
        }

        public static void Uxtah(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate)
        {
            EmitRotated(context, ArmExtensionType.Uxth, rd, rn, rm, rotate);
        }

        public static void Uxtb(CodeGenContext context, uint rd, uint rm, uint rotate)
        {
            EmitRotated(context, context.Arm64Assembler.Uxtb, rd, rm, rotate);
        }

        public static void Uxtb16(CodeGenContext context, uint rd, uint rm, uint rotate)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (rotate != 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)rotate * 8));
                context.Arm64Assembler.And(rdOperand, tempRegister.Operand, InstEmitCommon.Const(0xff00ff));
            }
            else
            {
                context.Arm64Assembler.And(rdOperand, rmOperand, InstEmitCommon.Const(0xff00ff));
            }
        }

        public static void Uxth(CodeGenContext context, uint rd, uint rm, uint rotate)
        {
            EmitRotated(context, context.Arm64Assembler.Uxth, rd, rm, rotate);
        }

        private static void EmitRotated(CodeGenContext context, Action<Operand, Operand> action, uint rd, uint rm, uint rotate)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (rotate != 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)rotate * 8));
                action(rdOperand, tempRegister.Operand);
            }
            else
            {
                action(rdOperand, rmOperand);
            }
        }

        private static void EmitRotated(CodeGenContext context, ArmExtensionType extensionType, uint rd, uint rn, uint rm, uint rotate)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (rotate != 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)rotate * 8));
                context.Arm64Assembler.Add(rdOperand, rnOperand, tempRegister.Operand, extensionType);
            }
            else
            {
                context.Arm64Assembler.Add(rdOperand, rnOperand, rmOperand, extensionType);
            }
        }

        private static void EmitExtendAccumulate8(CodeGenContext context, uint rd, uint rn, uint rm, uint rotate, bool unsigned)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (rotate != 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(tempRegister.Operand, rmOperand, InstEmitCommon.Const((int)rotate * 8));

                EmitExtendAccumulate8Core(context, rdOperand, rnOperand, tempRegister.Operand, unsigned);
            }
            else
            {
                EmitExtendAccumulate8Core(context, rdOperand, rnOperand, rmOperand, unsigned);
            }
        }

        private static void EmitExtendAccumulate8Core(CodeGenContext context, Operand rd, Operand rn, Operand rm, bool unsigned)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            if (unsigned)
            {
                context.Arm64Assembler.Uxth(tempN.Operand, rn);
            }
            else
            {
                context.Arm64Assembler.Sxth(tempN.Operand, rn);
            }

            context.Arm64Assembler.Add(tempD.Operand, tempN.Operand, rm, unsigned ? ArmExtensionType.Uxtb : ArmExtensionType.Sxtb);
            context.Arm64Assembler.Uxth(tempD2.Operand, tempD.Operand);

            if (unsigned)
            {
                context.Arm64Assembler.Lsr(tempN.Operand, rn, InstEmitCommon.Const(16));
            }
            else
            {
                context.Arm64Assembler.Asr(tempN.Operand, rn, InstEmitCommon.Const(16));
            }

            context.Arm64Assembler.Lsr(tempD.Operand, rm, InstEmitCommon.Const(16));
            context.Arm64Assembler.Add(tempD.Operand, tempN.Operand, tempD.Operand, unsigned ? ArmExtensionType.Uxtb : ArmExtensionType.Sxtb);
            context.Arm64Assembler.Orr(rd, tempD2.Operand, tempD.Operand, ArmShiftType.Lsl, 16);
        }
    }
}
