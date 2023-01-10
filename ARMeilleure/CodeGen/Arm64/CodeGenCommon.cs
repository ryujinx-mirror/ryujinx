using ARMeilleure.IntermediateRepresentation;
using System;
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
            ulong value = operand.Value;

            if (operand.Type == OperandType.I32)
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
            if (value == ulong.MaxValue)
            {
                immN = 0;
                immS = 0;
                immR = 0;

                return false;
            }

            int bitLength = CountSequence(value);

            if ((value >> bitLength) != 0)
            {
                bitLength += CountSequence(value >> bitLength);
            }

            int bitLengthLog2 = BitOperations.Log2((uint)bitLength);
            int bitLengthPow2 = 1 << bitLengthLog2;

            if (bitLengthPow2 < bitLength)
            {
                bitLengthLog2++;
                bitLengthPow2 <<= 1;
            }

            int selectedESize = 64;
            int repetitions = 1;
            int onesCount = BitOperations.PopCount(value);

            if (bitLengthPow2 < 64 && (value >> bitLengthPow2) != 0)
            {
                for (int eSizeLog2 = bitLengthLog2; eSizeLog2 < 6; eSizeLog2++)
                {
                    bool match = true;
                    int eSize = 1 << eSizeLog2;
                    ulong mask = (1UL << eSize) - 1;
                    ulong eValue = value & mask;

                    for (int e = 1; e < 64 / eSize; e++)
                    {
                        if (((value >> (e * eSize)) & mask) != eValue)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        selectedESize = eSize;
                        repetitions = 64 / eSize;
                        onesCount = BitOperations.PopCount(eValue);
                        break;
                    }
                }
            }

            // Find rotation. We have two cases, one where the highest bit is 0
            // and one where it is 1.
            // If it's 1, we just need to count the number of 1 bits on the MSB to find the right rotation.
            // If it's 0, we just need to count the number of 0 bits on the LSB to find the left rotation,
            // then we can convert it to the right rotation shift by subtracting the value from the element size.
            int rotation;
            long vHigh = (long)(value << (64 - selectedESize));
            if (vHigh < 0)
            {
                rotation = BitOperations.LeadingZeroCount(~(ulong)vHigh);
            }
            else
            {
                rotation = (selectedESize - BitOperations.TrailingZeroCount(value)) & (selectedESize - 1);
            }

            // Reconstruct value and see if it matches. If not, we can't encode.
            ulong reconstructed = onesCount == 64 ? ulong.MaxValue : RotateRight((1UL << onesCount) - 1, rotation, selectedESize);

            for (int bit = 32; bit >= selectedESize; bit >>= 1)
            {
                reconstructed |= reconstructed << bit;
            }

            if (reconstructed != value || onesCount == 0)
            {
                immN = 0;
                immS = 0;
                immR = 0;

                return false;
            }

            immR = rotation;

            // immN indicates that there are no repetitions.
            // The MSB of immS indicates the amount of repetitions, and the LSB the number of bits set.
            if (repetitions == 1)
            {
                immN = 1;
                immS = 0;
            }
            else
            {
                immN = 0;
                immS = (0xf80 >> BitOperations.Log2((uint)repetitions)) & 0x3f;
            }

            immS |= onesCount - 1;

            return true;
        }

        private static int CountSequence(ulong value)
        {
            return BitOperations.TrailingZeroCount(value) + BitOperations.TrailingZeroCount(~value);
        }

        private static ulong RotateRight(ulong bits, int shift, int size)
        {
            return (bits >> shift) | ((bits << (size - shift)) & (size == 64 ? ulong.MaxValue : (1UL << size) - 1));
        }
    }
}