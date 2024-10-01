namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonBit
    {
        public static void Vcls(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.Cls);
        }

        public static void Vclz(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.Clz);
        }

        public static void Vcnt(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.Cnt);
        }

        public static void Vrev16(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.Rev16);
        }

        public static void Vrev32(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.Rev32);
        }

        public static void Vrev64(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.Rev64);
        }
    }
}
