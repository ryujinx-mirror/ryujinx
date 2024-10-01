namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonCompare
    {
        public static void Vacge(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FacgeV, context.Arm64Assembler.FacgeVH);
        }

        public static void Vacgt(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FacgtV, context.Arm64Assembler.FacgtVH);
        }

        public static void VceqI(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcmeqZeroV, context.Arm64Assembler.FcmeqZeroVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.CmeqZeroV);
            }
        }

        public static void VceqR(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.CmeqRegV, context.Arm64Assembler.CmeqRegS);
        }

        public static void VceqFR(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FcmeqRegV, context.Arm64Assembler.FcmeqRegVH);
        }

        public static void VcgeI(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcmgeZeroV, context.Arm64Assembler.FcmgeZeroVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.CmgeZeroV);
            }
        }

        public static void VcgeR(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(
                context,
                rd,
                rn,
                rm,
                size,
                q,
                u ? context.Arm64Assembler.CmhsV : context.Arm64Assembler.CmgeRegV,
                u ? context.Arm64Assembler.CmhsS : context.Arm64Assembler.CmgeRegS);
        }

        public static void VcgeFR(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FcmgeRegV, context.Arm64Assembler.FcmgeRegVH);
        }

        public static void VcgtI(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcmgtZeroV, context.Arm64Assembler.FcmgtZeroVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.CmgtZeroV);
            }
        }

        public static void VcgtR(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(
                context,
                rd,
                rn,
                rm,
                size,
                q,
                u ? context.Arm64Assembler.CmhiV : context.Arm64Assembler.CmgtRegV,
                u ? context.Arm64Assembler.CmhiS : context.Arm64Assembler.CmgtRegS);
        }

        public static void VcgtFR(CodeGenContext context, uint rd, uint rn, uint rm, uint sz, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinaryF(context, rd, rn, rm, sz, q, context.Arm64Assembler.FcmgtRegV, context.Arm64Assembler.FcmgtRegVH);
        }

        public static void VcleI(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcmleV, context.Arm64Assembler.FcmleVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.CmleV);
            }
        }

        public static void VcltI(CodeGenContext context, uint rd, uint rm, bool f, uint size, uint q)
        {
            if (f)
            {
                InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FcmltV, context.Arm64Assembler.FcmltVH);
            }
            else
            {
                InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, size, q, context.Arm64Assembler.CmltV);
            }
        }

        public static void Vtst(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, context.Arm64Assembler.CmtstV, context.Arm64Assembler.CmtstS);
        }
    }
}
