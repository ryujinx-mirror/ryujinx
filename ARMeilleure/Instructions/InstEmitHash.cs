using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Crc32b(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U8(SoftFallback.Crc32b));
        }

        public static void Crc32h(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U16(SoftFallback.Crc32h));
        }

        public static void Crc32w(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U32(SoftFallback.Crc32w));
        }

        public static void Crc32x(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U64(SoftFallback.Crc32x));
        }

        public static void Crc32cb(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U8(SoftFallback.Crc32cb));
        }

        public static void Crc32ch(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U16(SoftFallback.Crc32ch));
        }

        public static void Crc32cw(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U32(SoftFallback.Crc32cw));
        }

        public static void Crc32cx(ArmEmitterContext context)
        {
            EmitCrc32Call(context, new _U32_U32_U64(SoftFallback.Crc32cx));
        }

        private static void EmitCrc32Call(ArmEmitterContext context, Delegate dlg)
        {
            OpCodeAluBinary op = (OpCodeAluBinary)context.CurrOp;

            Operand n = GetIntOrZR(context, op.Rn);
            Operand m = GetIntOrZR(context, op.Rm);

            Operand d = context.Call(dlg, n, m);

            SetIntOrZR(context, op.Rd, d);
        }
    }
}
