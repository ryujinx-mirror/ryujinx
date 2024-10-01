namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitVfpRound
    {
        public static void Vrinta(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintaFloat);
        }

        public static void Vrintm(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintmFloat);
        }

        public static void Vrintn(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintnFloat);
        }

        public static void Vrintp(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintpFloat);
        }

        public static void Vrintr(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintiFloat);
        }

        public static void Vrintx(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintxFloat);
        }

        public static void Vrintz(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FrintzFloat);
        }
    }
}
