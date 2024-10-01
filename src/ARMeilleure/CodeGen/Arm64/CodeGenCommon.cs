using ARMeilleure.IntermediateRepresentation;
using System.Numerics;

namespace ARMeilleure.CodeGen.Arm64
{
    static class CodeGenCommon
    {
        public const int TcAddressRegister = 8;
        public const int ReservedRegister = 17;

        public static bool ConstFitsOnSImm7(int value, int scale)
        {
            return (((value >> scale) << 25) >> (25 - scale)) == value;
        }

        public static bool ConstFitsOnSImm9(int value)
        {
            return ((value << 23) >> 23) == value;
        }

        public static bool ConstFitsOnUImm12(int value)
        {
            return (value & 0xfff) == value;
        }

        public static bool ConstFitsOnUImm12(int value, OperandType type)
        {
            int scale = Assembler.GetScaleForType(type);
            return (((value >> scale) & 0xfff) << scale) == value;
        }

        public static bool TryEncodeBitMask(Operand operand, out int immN, out int immS, out int immR)
        {
            return TryEncodeBitMask(operand.Type, operand.Value, out immN, out immS, out immR);
        }

        public static bool TryEncodeBitMask(OperandType type, ulong value, out int immN, out int immS, out int immR)
        {
            if (type == OperandType.I32)
            {
                value |= value << 32;
            }

            return TryEncodeBitMask(value, out immN, out immS, out immR);
        }

        public static bool TryEncodeBitMask(ulong value, out int immN, out int immS, out int immR)
        {
            // Some special values also can't be encoded:
            // 0 can't be encoded because we need to subtract 1 from onesCount (which would became negative if 0).
            // A value with all bits set can't be encoded because it is reserved according to the spec, because:
            // Any value AND all ones will be equal itself, so it's effectively a no-op.
            // Any value OR all ones will be equal all ones, so one can just use MOV.
            // Any value XOR all ones will be equal its inverse, so one can just use MVN.
            if (value == 0 || value == ulong.MaxValue)
            {
                immN = 0;
                immS = 0;
                immR = 0;

                return false;
            }

            // Normalize value, rotating it such that the LSB is 1: Ensures we get a complete element that has not
            // been cut-in-half across the word boundary.
            int rotation = BitOperations.TrailingZeroCount(value & (value + 1));
            ulong rotatedValue = ulong.RotateRight(value, rotation);

            // Now that we have a complete element in the LSB with the LSB = 1, determine size and number of ones
            // in element.
            int elementSize = BitOperations.TrailingZeroCount(rotatedValue & (rotatedValue + 1));
            int onesInElement = BitOperations.TrailingZeroCount(~rotatedValue);

            // Check the value is repeating; also ensures element size is a power of two.
            if (ulong.RotateRight(value, elementSize) != value)
            {
                immN = 0;
                immS = 0;
                immR = 0;

                return false;
            }

            immN = (elementSize >> 6) & 1;
            immS = (((~elementSize + 1) << 1) | (onesInElement - 1)) & 0x3f;
            immR = (elementSize - rotation) & (elementSize - 1);

            return true;
        }
    }
}
