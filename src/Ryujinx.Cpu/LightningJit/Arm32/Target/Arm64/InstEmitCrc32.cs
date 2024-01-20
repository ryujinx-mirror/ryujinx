using Ryujinx.Cpu.LightningJit.CodeGen;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitCrc32
    {
        public static void Crc32(CodeGenContext context, uint rd, uint rn, uint rm, uint sz)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Crc32(rdOperand, rnOperand, rmOperand, Math.Min(2, sz));
        }

        public static void Crc32c(CodeGenContext context, uint rd, uint rn, uint rm, uint sz)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            context.Arm64Assembler.Crc32c(rdOperand, rnOperand, rmOperand, Math.Min(2, sz));
        }
    }
}
