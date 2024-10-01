using ARMeilleure.IntermediateRepresentation;

namespace ARMeilleure.CodeGen.X86
{
    static class CodeGenCommon
    {
        public static bool IsLongConst(Operand op)
        {
            long value = op.Type == OperandType.I32 ? op.AsInt32() : op.AsInt64();

            return !ConstFitsOnS32(value);
        }

        private static bool ConstFitsOnS32(long value)
        {
            return value == (int)value;
        }
    }
}
