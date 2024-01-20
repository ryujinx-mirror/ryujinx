namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitVfpArithmetic
    {
        public static void VabsF(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FabsFloat);
        }

        public static void VaddF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FaddFloat);
        }

        public static void VdivF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FdivFloat);
        }

        public static void VfmaF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryRdF(context, rd, rn, rm, size, context.Arm64Assembler.FmaddFloat);
        }

        public static void VfmsF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryRdF(context, rd, rn, rm, size, context.Arm64Assembler.FmsubFloat);
        }

        public static void VfnmaF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryRdF(context, rd, rn, rm, size, context.Arm64Assembler.FnmaddFloat);
        }

        public static void VfnmsF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryRdF(context, rd, rn, rm, size, context.Arm64Assembler.FnmsubFloat);
        }

        public static void Vmaxnm(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FmaxnmFloat);
        }

        public static void Vminnm(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FminnmFloat);
        }

        public static void VmlaF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryMulNegRdF(context, rd, rn, rm, size, negD: false, negProduct: false);
        }

        public static void VmlsF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryMulNegRdF(context, rd, rn, rm, size, negD: false, negProduct: true);
        }

        public static void VmulF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FmulFloat);
        }

        public static void VnegF(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FnegFloat);
        }

        public static void VnmlaF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryMulNegRdF(context, rd, rn, rm, size, negD: true, negProduct: true);
        }

        public static void VnmlsF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarTernaryMulNegRdF(context, rd, rn, rm, size, negD: true, negProduct: false);
        }

        public static void VnmulF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FnmulFloat);
        }

        public static void VsqrtF(CodeGenContext context, uint rd, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FsqrtFloat);
        }

        public static void VsubF(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitScalarBinaryF(context, rd, rn, rm, size, context.Arm64Assembler.FsubFloat);
        }
    }
}
