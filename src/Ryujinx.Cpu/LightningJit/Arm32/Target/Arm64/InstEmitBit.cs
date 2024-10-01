using Ryujinx.Cpu.LightningJit.CodeGen;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitBit
    {
        public static void Bfc(CodeGenContext context, uint rd, uint lsb, uint msb)
        {
            // This is documented as "unpredictable".
            if (msb < lsb)
            {
                return;
            }

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);

            context.Arm64Assembler.Bfc(rdOperand, (int)lsb, (int)(msb - lsb + 1));
        }

        public static void Bfi(CodeGenContext context, uint rd, uint rn, uint lsb, uint msb)
        {
            // This is documented as "unpredictable".
            if (msb < lsb)
            {
                return;
            }

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            context.Arm64Assembler.Bfi(rdOperand, rnOperand, (int)lsb, (int)(msb - lsb + 1));
        }

        public static void Clz(CodeGenContext context, uint rd, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Clz(rdOperand, rmOperand);
        }

        public static void Rbit(CodeGenContext context, uint rd, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Rbit(rdOperand, rmOperand);
        }

        public static void Rev(CodeGenContext context, uint rd, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Rev(rdOperand, rmOperand);
        }

        public static void Rev16(CodeGenContext context, uint rd, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Rev16(rdOperand, rmOperand);
        }

        public static void Revsh(CodeGenContext context, uint rd, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Rev16(rdOperand, rmOperand);
            context.Arm64Assembler.Sxth(rdOperand, rdOperand);
        }

        public static void Sbfx(CodeGenContext context, uint rd, uint rn, uint lsb, uint widthMinus1)
        {
            // This is documented as "unpredictable".
            if (lsb + widthMinus1 > 31)
            {
                return;
            }

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            context.Arm64Assembler.Sbfx(rdOperand, rnOperand, (int)lsb, (int)widthMinus1 + 1);
        }

        public static void Ubfx(CodeGenContext context, uint rd, uint rn, uint lsb, uint widthMinus1)
        {
            // This is documented as "unpredictable".
            if (lsb + widthMinus1 > 31)
            {
                return;
            }

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            context.Arm64Assembler.Ubfx(rdOperand, rnOperand, (int)lsb, (int)widthMinus1 + 1);
        }
    }
}
