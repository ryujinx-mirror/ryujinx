using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonCrypto
    {
        public static void Aesd(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 0);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Aesd);
        }

        public static void Aese(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 0);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Aese);
        }

        public static void Aesimc(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 0);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Aesimc);
        }

        public static void Aesmc(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 0);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Aesmc);
        }
    }
}
