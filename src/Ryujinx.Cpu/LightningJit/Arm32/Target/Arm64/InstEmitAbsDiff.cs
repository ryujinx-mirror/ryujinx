using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitAbsDiff
    {
        public static void Usad8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            for (int b = 0; b < 4; b++)
            {
                context.Arm64Assembler.Ubfx(tempN.Operand, rnOperand, b * 8, 8);
                context.Arm64Assembler.Ubfx(tempM.Operand, rmOperand, b * 8, 8);

                Operand dest = b == 0 ? tempD.Operand : tempD2.Operand;

                context.Arm64Assembler.Sub(dest, tempN.Operand, tempM.Operand);

                EmitAbs(context, dest);

                if (b > 0)
                {
                    if (b < 3)
                    {
                        context.Arm64Assembler.Add(tempD.Operand, tempD.Operand, dest);
                    }
                    else
                    {
                        context.Arm64Assembler.Add(rdOperand, tempD.Operand, dest);
                    }
                }
            }
        }

        public static void Usada8(CodeGenContext context, uint rd, uint rn, uint rm, uint ra)
        {
            using ScopedRegister tempD = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempD2 = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            for (int b = 0; b < 4; b++)
            {
                context.Arm64Assembler.Ubfx(tempN.Operand, rnOperand, b * 8, 8);
                context.Arm64Assembler.Ubfx(tempM.Operand, rmOperand, b * 8, 8);

                Operand dest = b == 0 ? tempD.Operand : tempD2.Operand;

                context.Arm64Assembler.Sub(dest, tempN.Operand, tempM.Operand);

                EmitAbs(context, dest);

                if (b > 0)
                {
                    context.Arm64Assembler.Add(tempD.Operand, tempD.Operand, dest);
                }
            }

            context.Arm64Assembler.Add(rdOperand, tempD.Operand, raOperand);
        }

        private static void EmitAbs(CodeGenContext context, Operand value)
        {
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            // r = (value + ((int)value >> 31)) ^ ((int)value >> 31).
            // Subtracts 1 and then inverts the value if the sign bit is set, same as a conditional negation.

            context.Arm64Assembler.Add(tempRegister.Operand, value, value, ArmShiftType.Asr, 31);
            context.Arm64Assembler.Eor(value, tempRegister.Operand, value, ArmShiftType.Asr, 31);
        }
    }
}
