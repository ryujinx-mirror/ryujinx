namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonShift
    {
        public static void Vshll(CodeGenContext context, uint rd, uint rm, uint imm6, bool u)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6);
            uint shift = GetShiftLeft(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryLongShift(context, rd, rm, shift, size, isShl: true, u ? context.Arm64Assembler.Ushll : context.Arm64Assembler.Sshll);
        }

        public static void Vshll2(CodeGenContext context, uint rd, uint rm, uint size)
        {
            // Shift can't be encoded, so shift by value - 1 first, then first again by 1.
            // Doesn't matter if we do a signed or unsigned shift in this case since all sign bits will be shifted out.

            uint shift = 8u << (int)size;

            InstEmitNeonCommon.EmitVectorBinaryLongShift(context, rd, rm, shift - 1, size, isShl: true, context.Arm64Assembler.Sshll);
            InstEmitNeonCommon.EmitVectorBinaryLongShift(context, rd, rd, 1, size, isShl: true, context.Arm64Assembler.Sshll);
        }

        public static void VshlI(CodeGenContext context, uint rd, uint rm, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = GetShiftLeft(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryShift(context, rd, rm, shift, size, q, isShl: true, context.Arm64Assembler.ShlV, context.Arm64Assembler.ShlS);
        }

        public static void VshlR(CodeGenContext context, uint rd, uint rn, uint rm, bool u, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorBinary(
                context,
                rd,
                rm,
                rn,
                size,
                q,
                u ? context.Arm64Assembler.UshlV : context.Arm64Assembler.SshlV,
                u ? context.Arm64Assembler.UshlS : context.Arm64Assembler.SshlS);
        }

        public static void Vshr(CodeGenContext context, uint rd, uint rm, bool u, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryShift(
                context,
                rd,
                rm,
                shift,
                size,
                q,
                isShl: false,
                u ? context.Arm64Assembler.UshrV : context.Arm64Assembler.SshrV,
                u ? context.Arm64Assembler.UshrS : context.Arm64Assembler.SshrS);
        }

        public static void Vshrn(CodeGenContext context, uint rd, uint rm, uint imm6)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm6(imm6);
            uint shift = GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryNarrowShift(context, rd, rm, shift, size, isShl: false, context.Arm64Assembler.Shrn);
        }

        public static void Vsli(CodeGenContext context, uint rd, uint rm, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = GetShiftLeft(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryShift(
                context,
                rd,
                rm,
                shift,
                size,
                q,
                isShl: true,
                context.Arm64Assembler.SliV,
                context.Arm64Assembler.SliS);
        }

        public static void Vsra(CodeGenContext context, uint rd, uint rm, bool u, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorTernaryRdShift(
                context,
                rd,
                rm,
                shift,
                size,
                q,
                isShl: false,
                u ? context.Arm64Assembler.UsraV : context.Arm64Assembler.SsraV,
                u ? context.Arm64Assembler.UsraS : context.Arm64Assembler.SsraS);
        }

        public static void Vsri(CodeGenContext context, uint rd, uint rm, uint l, uint imm6, uint q)
        {
            uint size = InstEmitNeonCommon.GetSizeFromImm7(imm6 | (l << 6));
            uint shift = GetShiftRight(imm6, size);

            InstEmitNeonCommon.EmitVectorBinaryShift(context, rd, rm, shift, size, q, isShl: false, context.Arm64Assembler.SriV, context.Arm64Assembler.SriS);
        }

        public static uint GetShiftLeft(uint imm6, uint size)
        {
            return size < 3 ? imm6 - (8u << (int)size) : imm6;
        }

        public static uint GetShiftRight(uint imm6, uint size)
        {
            return (size == 3 ? 64u : (16u << (int)size)) - imm6;
            ;
        }
    }
}
