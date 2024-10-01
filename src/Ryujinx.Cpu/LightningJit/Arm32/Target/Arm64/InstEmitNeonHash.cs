using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonHash
    {
        public static void Sha1c(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha1c);
        }

        public static void Sha1h(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 2);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Sha1h);
        }

        public static void Sha1m(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha1m);
        }

        public static void Sha1p(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha1p);
        }

        public static void Sha1su0(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha1su0);
        }

        public static void Sha1su1(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 2);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Sha1su1);
        }

        public static void Sha256h(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha256h);
        }

        public static void Sha256h2(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha256h2);
        }

        public static void Sha256su0(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(size == 2);

            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, context.Arm64Assembler.Sha256su0);
        }

        public static void Sha256su1(CodeGenContext context, uint rd, uint rn, uint rm, uint q)
        {
            // TODO: Feature check, emulation if not supported.

            Debug.Assert(q == 1);

            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, context.Arm64Assembler.Sha256su1);
        }
    }
}
