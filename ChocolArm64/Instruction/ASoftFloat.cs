using System;

namespace ChocolArm64.Instruction
{
    static class ASoftFloat
    {
        static ASoftFloat()
        {
            InvSqrtEstimateTable = BuildInvSqrtEstimateTable();
            RecipEstimateTable = BuildRecipEstimateTable();
        }

        private static readonly byte[] RecipEstimateTable;
        private static readonly byte[] InvSqrtEstimateTable;

        private static byte[] BuildInvSqrtEstimateTable()
        {
            byte[] Table = new byte[512];
            for (ulong index = 128; index < 512; index++)
            {
                ulong a = index;
                if (a < 256)
                {
                    a = (a << 1) + 1;
                }
                else
                {
                    a = (a | 1) << 1;
                }

                ulong b = 256;
                while (a * (b + 1) * (b + 1) < (1ul << 28))
                {
                    b++;
                }
                b = (b + 1) >> 1;

                Table[index] = (byte)(b & 0xFF);
            }
            return Table;
        }

        private static byte[] BuildRecipEstimateTable()
        {
            byte[] Table = new byte[256];
            for (ulong index = 0; index < 256; index++)
            {
                ulong a = index | 0x100;

                a = (a << 1) + 1;
                ulong b = 0x80000 / a;
                b = (b + 1) >> 1;

                Table[index] = (byte)(b & 0xFF);
            }
            return Table;
        }

        public static float InvSqrtEstimate(float x)
        {
            return (float)InvSqrtEstimate((double)x);
        }

        public static double InvSqrtEstimate(double x)
        {
            ulong x_bits = (ulong)BitConverter.DoubleToInt64Bits(x);
            ulong x_sign = x_bits & 0x8000000000000000;
            long x_exp = (long)((x_bits >> 52) & 0x7FF);
            ulong scaled = x_bits & ((1ul << 52) - 1);

            if (x_exp == 0x7FF && scaled != 0)
            {
                // NaN
                return BitConverter.Int64BitsToDouble((long)(x_bits | 0x0008000000000000));
            }

            if (x_exp == 0)
            {
                if (scaled == 0)
                {
                    // Zero -> Infinity
                    return BitConverter.Int64BitsToDouble((long)(x_sign | 0x7ff0000000000000));
                }

                // Denormal
                while ((scaled & (1 << 51)) == 0)
                {
                    scaled <<= 1;
                    x_exp--;
                }
                scaled <<= 1;
            }

            if (x_sign != 0)
            {
                // Negative -> NaN
                return BitConverter.Int64BitsToDouble((long)0x7ff8000000000000);
            }

            if (x_exp == 0x7ff && scaled == 0)
            {
                // Infinity -> Zero
                return BitConverter.Int64BitsToDouble((long)x_sign);
            }

            if (((ulong)x_exp & 1) == 1)
            {
                scaled >>= 45;
                scaled &= 0xFF;
                scaled |= 0x80;
            }
            else
            {
                scaled >>= 44;
                scaled &= 0xFF;
                scaled |= 0x100;
            }

            ulong result_exp = ((ulong)(3068 - x_exp) / 2) & 0x7FF;
            ulong estimate = (ulong)InvSqrtEstimateTable[scaled];
            ulong fraction = estimate << 44;

            ulong result = x_sign | (result_exp << 52) | fraction;
            return BitConverter.Int64BitsToDouble((long)result);
        }

        public static float RecipEstimate(float x)
        {
            return (float)RecipEstimate((double)x);
        }

        public static double RecipEstimate(double x)
        {
            ulong x_bits = (ulong)BitConverter.DoubleToInt64Bits(x);
            ulong x_sign = x_bits & 0x8000000000000000;
            ulong x_exp = (x_bits >> 52) & 0x7FF;
            ulong scaled = x_bits & ((1ul << 52) - 1);

            if (x_exp >= 2045)
            {
                if (x_exp == 0x7ff && scaled != 0)
                {
                    // NaN
                    return BitConverter.Int64BitsToDouble((long)(x_bits | 0x0008000000000000));
                }

                // Infinity, or Out of range -> Zero
                return BitConverter.Int64BitsToDouble((long)x_sign);
            }

            if (x_exp == 0)
            {
                if (scaled == 0)
                {
                    // Zero -> Infinity
                    return BitConverter.Int64BitsToDouble((long)(x_sign | 0x7ff0000000000000));
                }

                // Denormal
                if ((scaled & (1ul << 51)) == 0)
                {
                    x_exp = ~0ul;
                    scaled <<= 2;
                }
                else
                {
                    scaled <<= 1;
                }
            }

            scaled >>= 44;
            scaled &= 0xFF;

            ulong result_exp = (2045 - x_exp) & 0x7FF;
            ulong estimate = (ulong)RecipEstimateTable[scaled];
            ulong fraction = estimate << 44;

            if (result_exp == 0)
            {
                fraction >>= 1;
                fraction |= 1ul << 51;
            }
            else if (result_exp == 0x7FF)
            {
                result_exp = 0;
                fraction >>= 2;
                fraction |= 1ul << 50;
            }

            ulong result = x_sign | (result_exp << 52) | fraction;
            return BitConverter.Int64BitsToDouble((long)result);
        }

        public static float RecipStep(float op1, float op2)
        {
            return (float)RecipStep((double)op1, (double)op2);
        }

        public static double RecipStep(double op1, double op2)
        {
            op1 = -op1;

            ulong op1_bits = (ulong)BitConverter.DoubleToInt64Bits(op1);
            ulong op2_bits = (ulong)BitConverter.DoubleToInt64Bits(op2);

            ulong op1_sign = op1_bits & 0x8000000000000000;
            ulong op2_sign = op2_bits & 0x8000000000000000;
            ulong op1_other = op1_bits & 0x7FFFFFFFFFFFFFFF;
            ulong op2_other = op2_bits & 0x7FFFFFFFFFFFFFFF;

            bool inf1 = op1_other == 0x7ff0000000000000;
            bool inf2 = op2_other == 0x7ff0000000000000;
            bool zero1 = op1_other == 0;
            bool zero2 = op2_other == 0;

            if ((inf1 && zero2) || (zero1 && inf2))
            {
                return 2.0;
            }
            else if (inf1 || inf2)
            {
                // Infinity
                return BitConverter.Int64BitsToDouble((long)(0x7ff0000000000000 | (op1_sign ^ op2_sign)));
            }

            return 2.0 + op1 * op2;
        }
    }
}