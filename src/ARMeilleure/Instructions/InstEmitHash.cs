using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHashHelper;
using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        private const int ByteSizeLog2 = 0;
        private const int HWordSizeLog2 = 1;
        private const int WordSizeLog2 = 2;
        private const int DWordSizeLog2 = 3;

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

        public static void Crc32x(ArmEmitterContext context)
        {
            EmitCrc32Call(context, DWordSizeLog2, false);
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

        public static void Crc32cx(ArmEmitterContext context)
        {
            EmitCrc32Call(context, DWordSizeLog2, true);
        }

        private static void EmitCrc32Call(ArmEmitterContext context, int size, bool c)
        {
            OpCodeAluBinary op = (OpCodeAluBinary)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand d = EmitCrc32(context, n, m, size, c);

            SetIntOrZR(context, op.Rd, d);
        }
    }
}
