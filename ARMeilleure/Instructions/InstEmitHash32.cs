using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitHashHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Crc32b(ArmEmitterContext context)
        {
            EmitCrc32Call(context, ByteSizeLog2, false);
        }

        public static void Crc32h(ArmEmitterContext context)
        {
            EmitCrc32Call(context, HWordSizeLog2, false);
        }

        public static void Crc32w(ArmEmitterContext context)
        {
            EmitCrc32Call(context, WordSizeLog2, false);
        }

        public static void Crc32cb(ArmEmitterContext context)
        {
            EmitCrc32Call(context, ByteSizeLog2, true);
        }

        public static void Crc32ch(ArmEmitterContext context)
        {
            EmitCrc32Call(context, HWordSizeLog2, true);
        }

        public static void Crc32cw(ArmEmitterContext context)
        {
            EmitCrc32Call(context, WordSizeLog2, true);
        }

        private static void EmitCrc32Call(ArmEmitterContext context, int size, bool c)
        {
            IOpCode32AluReg op = (IOpCode32AluReg)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);
            Operand m = GetIntA32(context, op.Rm);

            Operand d = EmitCrc32(context, n, m, size, c);

            EmitAluStore(context, d);
        }
    }
}
