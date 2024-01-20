namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonRound
    {
        public static void Vraddhn(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryNarrow(context, rd, rn, rm, size, context.Arm64Assembler.Raddhn);
        }

        public static void Vrhadd(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(context, rd, rn, rm, size, q, u ? context.Arm64Assembler.Urhadd : context.Arm64Assembler.Srhadd, null);
        }

        public static void Vrshl(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(
                context,
                rd,
                rm,
                rn,
                size,
                q,
                u ? context.Arm64Assembler.UrshlV : context.Arm64Assembler.SrshlV,
                u ? context.Arm64Assembler.UrshlS : context.Arm64Assembler.SrshlS);
        }

        public static void Vrshr(CodeGenContext context, uint rd, uint rm, bool u, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = InstEmitNeonShift.GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryShift(
                context,
                rd,
                rm,
                shift,
                size,
                q,
                isShl: false,
                u ? context.Arm64Assembler.UrshrV : context.Arm64Assembler.SrshrV,
                u ? context.Arm64Assembler.UrshrS : context.Arm64Assembler.SrshrS);
        }

        public static void Vrshrn(CodeGenContext context, uint rd, uint rm, uint imm6)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm6(imm6);
            uint shift = InstEmitNeonShift.GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.Rshrn);
        }

        public static void Vrsra(CodeGenContext context, uint rd, uint rm, bool u, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = InstEmitNeonShift.GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorTernaryRdShift(
                context,
                rd,
                rm,
                shift,
                size,
                q,
                isShl: false,
                u ? context.Arm64Assembler.UrsraV : context.Arm64Assembler.SrsraV,
                u ? context.Arm64Assembler.UrsraS : context.Arm64Assembler.SrsraS);
        }

        public static void Vrsubhn(CodeGenContext context, uint rd, uint rn, uint rm, uint size)
        {
            InstEmitNeonCommon.EmitVectorBinaryNarrow(context, rd, rn, rm, size, context.Arm64Assembler.Rsubhn);
        }

        public static void Vrinta(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrintaSingleAndDouble, context.Arm64Assembler.FrintaHalf);
        }

        public static void Vrintm(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrintmSingleAndDouble, context.Arm64Assembler.FrintmHalf);
        }

        public static void Vrintn(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrintnSingleAndDouble, context.Arm64Assembler.FrintnHalf);
        }

        public static void Vrintp(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrintpSingleAndDouble, context.Arm64Assembler.FrintpHalf);
        }

        public static void Vrintx(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrintxSingleAndDouble, context.Arm64Assembler.FrintxHalf);
        }

        public static void Vrintz(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnaryAnyF(context, rd, rm, size, q, context.Arm64Assembler.FrintzSingleAndDouble, context.Arm64Assembler.FrintzHalf);
        }
    }
}
