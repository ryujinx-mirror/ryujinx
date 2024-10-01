using Ryujinx.Cpu.LightningJit.CodeGen;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonLogical
    {
        public static void VandR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, q, context.Arm64Assembler.And);
        }

        public static void VbicI(CodeGenContext context, uint rd, uint cmode, uint imm8, uint q)
        {
            EmitMovi(context, rd, cmode, imm8, 1, q);
        }

        public static void VbicR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, q, context.Arm64Assembler.BicReg);
        }

        public static void VbifR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, q, context.Arm64Assembler.Bif);
        }

        public static void VbitR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, q, context.Arm64Assembler.Bit);
        }

        public static void VbslR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorTernaryRd(context, rd, rn, rm, q, context.Arm64Assembler.Bsl);
        }

        public static void VeorR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, q, context.Arm64Assembler.Eor);
        }

        public static void VornR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, q, context.Arm64Assembler.Orn);
        }

        public static void VorrI(CodeGenContext context, uint rd, uint cmode, uint imm8, uint q)
        {
            EmitMovi(context, rd, cmode, imm8, 0, q);
        }

        public static void VorrR(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, q, context.Arm64Assembler.OrrReg);
        }

        private static void EmitMovi(CodeGenContext context, uint rd, uint cmode, uint imm8, uint op, uint q)
        {
            (uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h) = InstEmitNeonMove.Split(imm8);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                InstEmitNeonCommon.MoveScalarToSide(context, tempRegister.Operand, rd, false);

                context.Arm64Assembler.Movi(tempRegister.Operand, h, g, f, e, d, cmode, c, b, a, op, q);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

                context.Arm64Assembler.Movi(rdOperand, h, g, f, e, d, cmode, c, b, a, op, q);
            }
        }
    }
}
