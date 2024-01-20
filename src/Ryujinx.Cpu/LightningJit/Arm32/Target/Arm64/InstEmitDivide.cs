using Ryujinx.Cpu.LightningJit.CodeGen;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitDivide
    {
        public static void Sdiv(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Sdiv(rdOperand, rnOperand, rmOperand);
        }

        public static void Udiv(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Udiv(rdOperand, rnOperand, rmOperand);
        }
    }
}
