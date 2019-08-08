using ChocolArm64.State;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ChocolArm64.Instructions
{
    static class SoftFloat
    {
        static SoftFloat()
        {
            RecipEstimateTable     = BuildRecipEstimateTable();
            RecipSqrtEstimateTable = BuildRecipSqrtEstimateTable();
        }

        internal static readonly byte[] RecipEstimateTable;
        internal static readonly byte[] RecipSqrtEstimateTable;

        private static byte[] BuildRecipEstimateTable()
        {
            byte[] tbl = new byte[256];

            for (int idx = 0; idx < 256; idx++)
            {
                uint src = (uint)idx + 256u;

                Debug.Assert(256u <= src && src < 512u);

                src = (src << 1) + 1u;

                uint aux = (1u << 19) / src;

                uint dst = (aux + 1u) >> 1;

                Debug.Assert(256u <= dst && dst < 512u);

                tbl[idx] = (byte)(dst - 256u);
            }

            return tbl;
        }

        private static byte[] BuildRecipSqrtEstimateTable()
        {
            byte[] tbl = new byte[384];

            for (int idx = 0; idx < 384; idx++)
            {
                uint src = (uint)idx + 128u;

                Debug.Assert(128u <= src && src < 512u);

                if (src < 256u)
                {
                    src = (src << 1) + 1u;
                }
                else
                {
                    src = (src >> 1) << 1;
                    src = (src + 1u) << 1;
                }

                uint aux = 512u;

                while (src * (aux + 1u) * (aux + 1u) < (1u << 28))
                {
                    aux = aux + 1u;
                }

                uint dst = (aux + 1u) >> 1;

                Debug.Assert(256u <= dst && dst < 512u);

                tbl[idx] = (byte)(dst - 256u);
            }

            return tbl;
        }
    }

    static class SoftFloat16_32
    {
        public static float FPConvert(ushort valueBits, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat16_32.FPConvert: state.Fpcr = 0x{state.CFpcr:X8}");

            double real = valueBits.FPUnpackCv(out FpType type, out bool sign, state);

            float result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                if (state.GetFpcrFlag(Fpcr.Dn))
                {
                    result = FPDefaultNaN();
                }
                else
                {
                    result = FPConvertNaN(valueBits);
                }

                if (type == FpType.SNaN)
                {
                    FPProcessException(FpExc.InvalidOp, state);
                }
            }
            else if (type == FpType.Infinity)
            {
                result = FPInfinity(sign);
            }
            else if (type == FpType.Zero)
            {
                result = FPZero(sign);
            }
            else
            {
                result = FPRoundCv(real, state);
            }

            return result;
        }

        private static float FPDefaultNaN()
        {
            return -float.NaN;
        }

        private static float FPInfinity(bool sign)
        {
            return sign ? float.NegativeInfinity : float.PositiveInfinity;
        }

        private static float FPZero(bool sign)
        {
            return sign ? -0f : +0f;
        }

        private static float FPMaxNormal(bool sign)
        {
            return sign ? float.MinValue : float.MaxValue;
        }

        private static double FPUnpackCv(
            this ushort valueBits,
            out FpType type,
            out bool sign,
            CpuThreadState state)
        {
            sign = (~(uint)valueBits & 0x8000u) == 0u;

            uint exp16  = ((uint)valueBits & 0x7C00u) >> 10;
            uint frac16 =  (uint)valueBits & 0x03FFu;

            double real;

            if (exp16 == 0u)
            {
                if (frac16 == 0u)
                {
                    type = FpType.Zero;
                    real = 0d;
                }
                else
                {
                    type = FpType.Nonzero; // Subnormal.
                    real = Math.Pow(2d, -14) * ((double)frac16 * Math.Pow(2d, -10));
                }
            }
            else if (exp16 == 0x1Fu && !state.GetFpcrFlag(Fpcr.Ahp))
            {
                if (frac16 == 0u)
                {
                    type = FpType.Infinity;
                    real = Math.Pow(2d, 1000);
                }
                else
                {
                    type = (~frac16 & 0x0200u) == 0u ? FpType.QNaN : FpType.SNaN;
                    real = 0d;
                }
            }
            else
            {
                type = FpType.Nonzero; // Normal.
                real = Math.Pow(2d, (int)exp16 - 15) * (1d + (double)frac16 * Math.Pow(2d, -10));
            }

            return sign ? -real : real;
        }

        private static float FPRoundCv(double real, CpuThreadState state)
        {
            const int minimumExp = -126;

            const int e = 8;
            const int f = 23;

            bool   sign;
            double mantissa;

            if (real < 0d)
            {
                sign     = true;
                mantissa = -real;
            }
            else
            {
                sign     = false;
                mantissa = real;
            }

            int exponent = 0;

            while (mantissa < 1d)
            {
                mantissa *= 2d;
                exponent--;
            }

            while (mantissa >= 2d)
            {
                mantissa /= 2d;
                exponent++;
            }

            if (state.GetFpcrFlag(Fpcr.Fz) && exponent < minimumExp)
            {
                state.SetFpsrFlag(Fpsr.Ufc);

                return FPZero(sign);
            }

            uint biasedExp = (uint)Math.Max(exponent - minimumExp + 1, 0);

            if (biasedExp == 0u)
            {
                mantissa /= Math.Pow(2d, minimumExp - exponent);
            }

            uint intMant = (uint)Math.Floor(mantissa * Math.Pow(2d, f));
            double error = mantissa * Math.Pow(2d, f) - (double)intMant;

            if (biasedExp == 0u && (error != 0d || state.GetFpcrFlag(Fpcr.Ufe)))
            {
                FPProcessException(FpExc.Underflow, state);
            }

            bool overflowToInf;
            bool roundUp;

            switch (state.FPRoundingMode())
            {
                default:
                case RoundMode.ToNearest:
                    roundUp       = (error > 0.5d || (error == 0.5d && (intMant & 1u) == 1u));
                    overflowToInf = true;
                    break;

                case RoundMode.TowardsPlusInfinity:
                    roundUp       = (error != 0d && !sign);
                    overflowToInf = !sign;
                    break;

                case RoundMode.TowardsMinusInfinity:
                    roundUp       = (error != 0d && sign);
                    overflowToInf = sign;
                    break;

                case RoundMode.TowardsZero:
                    roundUp       = false;
                    overflowToInf = false;
                    break;
            }

            if (roundUp)
            {
                intMant++;

                if (intMant == 1u << f)
                {
                    biasedExp = 1u;
                }

                if (intMant == 1u << (f + 1))
                {
                    biasedExp++;
                    intMant >>= 1;
                }
            }

            float result;

            if (biasedExp >= (1u << e) - 1u)
            {
                result = overflowToInf ? FPInfinity(sign) : FPMaxNormal(sign);

                FPProcessException(FpExc.Overflow, state);

                error = 1d;
            }
            else
            {
                result = BitConverter.Int32BitsToSingle(
                    (int)((sign ? 1u : 0u) << 31 | (biasedExp & 0xFFu) << 23 | (intMant & 0x007FFFFFu)));
            }

            if (error != 0d)
            {
                FPProcessException(FpExc.Inexact, state);
            }

            return result;
        }

        private static float FPConvertNaN(ushort valueBits)
        {
            return BitConverter.Int32BitsToSingle(
                (int)(((uint)valueBits & 0x8000u) << 16 | 0x7FC00000u | ((uint)valueBits & 0x01FFu) << 13));
        }

        private static void FPProcessException(FpExc exc, CpuThreadState state)
        {
            int enable = (int)exc + 8;

            if ((state.CFpcr & (1 << enable)) != 0)
            {
                throw new NotImplementedException("Floating-point trap handling.");
            }
            else
            {
                state.CFpsr |= 1 << (int)exc;
            }
        }
    }

    static class SoftFloat32_16
    {
        public static ushort FPConvert(float value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32_16.FPConvert: state.Fpcr = 0x{state.CFpcr:X8}");

            double real = value.FPUnpackCv(out FpType type, out bool sign, out uint valueBits, state);

            bool altHp = state.GetFpcrFlag(Fpcr.Ahp);

            ushort resultBits;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                if (altHp)
                {
                    resultBits = FPZero(sign);
                }
                else if (state.GetFpcrFlag(Fpcr.Dn))
                {
                    resultBits = FPDefaultNaN();
                }
                else
                {
                    resultBits = FPConvertNaN(valueBits);
                }

                if (type == FpType.SNaN || altHp)
                {
                    FPProcessException(FpExc.InvalidOp, state);
                }
            }
            else if (type == FpType.Infinity)
            {
                if (altHp)
                {
                    resultBits = (ushort)((sign ? 1u : 0u) << 15 | 0x7FFFu);

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else
                {
                    resultBits = FPInfinity(sign);
                }
            }
            else if (type == FpType.Zero)
            {
                resultBits = FPZero(sign);
            }
            else
            {
                resultBits = FPRoundCv(real, state);
            }

            return resultBits;
        }

        private static ushort FPDefaultNaN()
        {
            return (ushort)0x7E00u;
        }

        private static ushort FPInfinity(bool sign)
        {
            return sign ? (ushort)0xFC00u : (ushort)0x7C00u;
        }

        private static ushort FPZero(bool sign)
        {
            return sign ? (ushort)0x8000u : (ushort)0x0000u;
        }

        private static ushort FPMaxNormal(bool sign)
        {
            return sign ? (ushort)0xFBFFu : (ushort)0x7BFFu;
        }

        private static double FPUnpackCv(
            this float value,
            out FpType type,
            out bool sign,
            out uint valueBits,
            CpuThreadState state)
        {
            valueBits = (uint)BitConverter.SingleToInt32Bits(value);

            sign = (~valueBits & 0x80000000u) == 0u;

            uint exp32  = (valueBits & 0x7F800000u) >> 23;
            uint frac32 =  valueBits & 0x007FFFFFu;

            double real;

            if (exp32 == 0u)
            {
                if (frac32 == 0u || state.GetFpcrFlag(Fpcr.Fz))
                {
                    type = FpType.Zero;
                    real = 0d;

                    if (frac32 != 0u)
                    {
                        FPProcessException(FpExc.InputDenorm, state);
                    }
                }
                else
                {
                    type = FpType.Nonzero; // Subnormal.
                    real = Math.Pow(2d, -126) * ((double)frac32 * Math.Pow(2d, -23));
                }
            }
            else if (exp32 == 0xFFu)
            {
                if (frac32 == 0u)
                {
                    type = FpType.Infinity;
                    real = Math.Pow(2d, 1000);
                }
                else
                {
                    type = (~frac32 & 0x00400000u) == 0u ? FpType.QNaN : FpType.SNaN;
                    real = 0d;
                }
            }
            else
            {
                type = FpType.Nonzero; // Normal.
                real = Math.Pow(2d, (int)exp32 - 127) * (1d + (double)frac32 * Math.Pow(2d, -23));
            }

            return sign ? -real : real;
        }

        private static ushort FPRoundCv(double real, CpuThreadState state)
        {
            const int minimumExp = -14;

            const int e = 5;
            const int f = 10;

            bool   sign;
            double mantissa;

            if (real < 0d)
            {
                sign     = true;
                mantissa = -real;
            }
            else
            {
                sign     = false;
                mantissa = real;
            }

            int exponent = 0;

            while (mantissa < 1d)
            {
                mantissa *= 2d;
                exponent--;
            }

            while (mantissa >= 2d)
            {
                mantissa /= 2d;
                exponent++;
            }

            uint biasedExp = (uint)Math.Max(exponent - minimumExp + 1, 0);

            if (biasedExp == 0u)
            {
                mantissa /= Math.Pow(2d, minimumExp - exponent);
            }

            uint intMant = (uint)Math.Floor(mantissa * Math.Pow(2d, f));
            double error = mantissa * Math.Pow(2d, f) - (double)intMant;

            if (biasedExp == 0u && (error != 0d || state.GetFpcrFlag(Fpcr.Ufe)))
            {
                FPProcessException(FpExc.Underflow, state);
            }

            bool overflowToInf;
            bool roundUp;

            switch (state.FPRoundingMode())
            {
                default:
                case RoundMode.ToNearest:
                    roundUp       = (error > 0.5d || (error == 0.5d && (intMant & 1u) == 1u));
                    overflowToInf = true;
                    break;

                case RoundMode.TowardsPlusInfinity:
                    roundUp       = (error != 0d && !sign);
                    overflowToInf = !sign;
                    break;

                case RoundMode.TowardsMinusInfinity:
                    roundUp       = (error != 0d && sign);
                    overflowToInf = sign;
                    break;

                case RoundMode.TowardsZero:
                    roundUp       = false;
                    overflowToInf = false;
                    break;
            }

            if (roundUp)
            {
                intMant++;

                if (intMant == 1u << f)
                {
                    biasedExp = 1u;
                }

                if (intMant == 1u << (f + 1))
                {
                    biasedExp++;
                    intMant >>= 1;
                }
            }

            ushort resultBits;

            if (!state.GetFpcrFlag(Fpcr.Ahp))
            {
                if (biasedExp >= (1u << e) - 1u)
                {
                    resultBits = overflowToInf ? FPInfinity(sign) : FPMaxNormal(sign);

                    FPProcessException(FpExc.Overflow, state);

                    error = 1d;
                }
                else
                {
                    resultBits = (ushort)((sign ? 1u : 0u) << 15 | (biasedExp & 0x1Fu) << 10 | (intMant & 0x03FFu));
                }
            }
            else
            {
                if (biasedExp >= 1u << e)
                {
                    resultBits = (ushort)((sign ? 1u : 0u) << 15 | 0x7FFFu);

                    FPProcessException(FpExc.InvalidOp, state);

                    error = 0d;
                }
                else
                {
                    resultBits = (ushort)((sign ? 1u : 0u) << 15 | (biasedExp & 0x1Fu) << 10 | (intMant & 0x03FFu));
                }
            }

            if (error != 0d)
            {
                FPProcessException(FpExc.Inexact, state);
            }

            return resultBits;
        }

        private static ushort FPConvertNaN(uint valueBits)
        {
            return (ushort)((valueBits & 0x80000000u) >> 16 | 0x7E00u | (valueBits & 0x003FE000u) >> 13);
        }

        private static void FPProcessException(FpExc exc, CpuThreadState state)
        {
            int enable = (int)exc + 8;

            if ((state.CFpcr & (1 << enable)) != 0)
            {
                throw new NotImplementedException("Floating-point trap handling.");
            }
            else
            {
                state.CFpsr |= 1 << (int)exc;
            }
        }
    }

    static class SoftFloat32
    {
        public static float FPAdd(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPAdd: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if (inf1 && inf2 && sign1 == !sign2)
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if ((inf1 && !sign1) || (inf2 && !sign2))
                {
                    result = FPInfinity(false);
                }
                else if ((inf1 && sign1) || (inf2 && sign2))
                {
                    result = FPInfinity(true);
                }
                else if (zero1 && zero2 && sign1 == sign2)
                {
                    result = FPZero(sign1);
                }
                else
                {
                    result = value1 + value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        public static int FPCompare(float value1, float value2, bool signalNaNs, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPCompare: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out _, state);

            int result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = 0b0011;

                if (type1 == FpType.SNaN || type2 == FpType.SNaN || signalNaNs)
                {
                    FPProcessException(FpExc.InvalidOp, state);
                }
            }
            else
            {
                if (value1 == value2)
                {
                    result = 0b0110;
                }
                else if (value1 < value2)
                {
                    result = 0b1000;
                }
                else
                {
                    result = 0b0010;
                }
            }

            return result;
        }

        public static float FPCompareEQ(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPCompareEQ: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out _, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out _, out _, state);

            float result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = ZerosOrOnes(false);

                if (type1 == FpType.SNaN || type2 == FpType.SNaN)
                {
                    FPProcessException(FpExc.InvalidOp, state);
                }
            }
            else
            {
                result = ZerosOrOnes(value1 == value2);
            }

            return result;
        }

        public static float FPCompareGE(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPCompareGE: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out _, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out _, out _, state);

            float result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = ZerosOrOnes(false);

                FPProcessException(FpExc.InvalidOp, state);
            }
            else
            {
                result = ZerosOrOnes(value1 >= value2);
            }

            return result;
        }

        public static float FPCompareGT(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPCompareGT: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out _, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out _, out _, state);

            float result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = ZerosOrOnes(false);

                FPProcessException(FpExc.InvalidOp, state);
            }
            else
            {
                result = ZerosOrOnes(value1 > value2);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FPCompareLE(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPCompareLE: state.Fpcr = 0x{state.CFpcr:X8}");

            return FPCompareGE(value2, value1, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FPCompareLT(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPCompareLT: state.Fpcr = 0x{state.CFpcr:X8}");

            return FPCompareGT(value2, value1, state);
        }

        public static float FPDiv(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPDiv: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && inf2) || (zero1 && zero2))
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if (inf1 || zero2)
                {
                    result = FPInfinity(sign1 ^ sign2);

                    if (!inf1)
                    {
                        FPProcessException(FpExc.DivideByZero, state);
                    }
                }
                else if (zero1 || inf2)
                {
                    result = FPZero(sign1 ^ sign2);
                }
                else
                {
                    result = value1 / value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        public static float FPMax(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMax: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                if (value1 > value2)
                {
                    if (type1 == FpType.Infinity)
                    {
                        result = FPInfinity(sign1);
                    }
                    else if (type1 == FpType.Zero)
                    {
                        result = FPZero(sign1 && sign2);
                    }
                    else
                    {
                        result = value1;
                    }
                }
                else
                {
                    if (type2 == FpType.Infinity)
                    {
                        result = FPInfinity(sign2);
                    }
                    else if (type2 == FpType.Zero)
                    {
                        result = FPZero(sign1 && sign2);
                    }
                    else
                    {
                        result = value2;

                        if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                        {
                            state.SetFpsrFlag(Fpsr.Ufc);

                            result = FPZero(result < 0f);
                        }
                    }
                }
            }

            return result;
        }

        public static float FPMaxNum(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMaxNum: state.Fpcr = 0x{state.CFpcr:X8}");

            value1.FPUnpack(out FpType type1, out _, out _, state);
            value2.FPUnpack(out FpType type2, out _, out _, state);

            if (type1 == FpType.QNaN && type2 != FpType.QNaN)
            {
                value1 = FPInfinity(true);
            }
            else if (type1 != FpType.QNaN && type2 == FpType.QNaN)
            {
                value2 = FPInfinity(true);
            }

            return FPMax(value1, value2, state);
        }

        public static float FPMin(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMin: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                if (value1 < value2)
                {
                    if (type1 == FpType.Infinity)
                    {
                        result = FPInfinity(sign1);
                    }
                    else if (type1 == FpType.Zero)
                    {
                        result = FPZero(sign1 || sign2);
                    }
                    else
                    {
                        result = value1;
                    }
                }
                else
                {
                    if (type2 == FpType.Infinity)
                    {
                        result = FPInfinity(sign2);
                    }
                    else if (type2 == FpType.Zero)
                    {
                        result = FPZero(sign1 || sign2);
                    }
                    else
                    {
                        result = value2;

                        if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                        {
                            state.SetFpsrFlag(Fpsr.Ufc);

                            result = FPZero(result < 0f);
                        }
                    }
                }
            }

            return result;
        }

        public static float FPMinNum(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMinNum: state.Fpcr = 0x{state.CFpcr:X8}");

            value1.FPUnpack(out FpType type1, out _, out _, state);
            value2.FPUnpack(out FpType type2, out _, out _, state);

            if (type1 == FpType.QNaN && type2 != FpType.QNaN)
            {
                value1 = FPInfinity(false);
            }
            else if (type1 != FpType.QNaN && type2 == FpType.QNaN)
            {
                value2 = FPInfinity(false);
            }

            return FPMin(value1, value2, state);
        }

        public static float FPMul(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMul: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else if (zero1 || zero2)
                {
                    result = FPZero(sign1 ^ sign2);
                }
                else
                {
                    result = value1 * value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        public static float FPMulAdd(
            float valueA,
            float value1,
            float value2,
            CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMulAdd: state.Fpcr = 0x{state.CFpcr:X8}");

            valueA = valueA.FPUnpack(out FpType typeA, out bool signA, out uint addend, state);
            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1,    state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2,    state);

            bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
            bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

            float result = FPProcessNaNs3(typeA, type1, type2, addend, op1, op2, out bool done, state);

            if (typeA == FpType.QNaN && ((inf1 && zero2) || (zero1 && inf2)))
            {
                result = FPDefaultNaN();

                FPProcessException(FpExc.InvalidOp, state);
            }

            if (!done)
            {
                bool infA = typeA == FpType.Infinity; bool zeroA = typeA == FpType.Zero;

                bool signP = sign1 ^  sign2;
                bool infP  = inf1  || inf2;
                bool zeroP = zero1 || zero2;

                if ((inf1 && zero2) || (zero1 && inf2) || (infA && infP && signA != signP))
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if ((infA && !signA) || (infP && !signP))
                {
                    result = FPInfinity(false);
                }
                else if ((infA && signA) || (infP && signP))
                {
                    result = FPInfinity(true);
                }
                else if (zeroA && zeroP && signA == signP)
                {
                    result = FPZero(signA);
                }
                else
                {
                    // TODO: When available, use: T MathF.FusedMultiplyAdd(T, T, T);
                    // https://github.com/dotnet/corefx/issues/31903

                    result = valueA + (value1 * value2);

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FPMulSub(
            float valueA,
            float value1,
            float value2,
            CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMulSub: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPNeg();

            return FPMulAdd(valueA, value1, value2, state);
        }

        public static float FPMulX(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPMulX: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPTwo(sign1 ^ sign2);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else if (zero1 || zero2)
                {
                    result = FPZero(sign1 ^ sign2);
                }
                else
                {
                    result = value1 * value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        public static float FPRecipEstimate(float value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPRecipEstimate: state.Fpcr = 0x{state.CFpcr:X8}");

            value.FPUnpack(out FpType type, out bool sign, out uint op, state);

            float result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else if (type == FpType.Infinity)
            {
                result = FPZero(sign);
            }
            else if (type == FpType.Zero)
            {
                result = FPInfinity(sign);

                FPProcessException(FpExc.DivideByZero, state);
            }
            else if (MathF.Abs(value) < MathF.Pow(2f, -128))
            {
                bool overflowToInf;

                switch (state.FPRoundingMode())
                {
                    default:
                    case RoundMode.ToNearest:            overflowToInf = true;  break;
                    case RoundMode.TowardsPlusInfinity:  overflowToInf = !sign; break;
                    case RoundMode.TowardsMinusInfinity: overflowToInf = sign;  break;
                    case RoundMode.TowardsZero:          overflowToInf = false; break;
                }

                result = overflowToInf ? FPInfinity(sign) : FPMaxNormal(sign);

                FPProcessException(FpExc.Overflow, state);
                FPProcessException(FpExc.Inexact,  state);
            }
            else if (state.GetFpcrFlag(Fpcr.Fz) && (MathF.Abs(value) >= MathF.Pow(2f, 126)))
            {
                result = FPZero(sign);

                state.SetFpsrFlag(Fpsr.Ufc);
            }
            else
            {
                ulong fraction = (ulong)(op & 0x007FFFFFu) << 29;
                uint exp = (op & 0x7F800000u) >> 23;

                if (exp == 0u)
                {
                    if ((fraction & 0x0008000000000000ul) == 0ul)
                    {
                        fraction = (fraction & 0x0003FFFFFFFFFFFFul) << 2;
                        exp -= 1u;
                    }
                    else
                    {
                        fraction = (fraction & 0x0007FFFFFFFFFFFFul) << 1;
                    }
                }

                uint scaled = (uint)(((fraction & 0x000FF00000000000ul) | 0x0010000000000000ul) >> 44);

                uint resultExp = 253u - exp;

                uint estimate = (uint)SoftFloat.RecipEstimateTable[scaled - 256u] + 256u;

                fraction = (ulong)(estimate & 0xFFu) << 44;

                if (resultExp == 0u)
                {
                    fraction = ((fraction & 0x000FFFFFFFFFFFFEul) | 0x0010000000000000ul) >> 1;
                }
                else if (resultExp + 1u == 0u)
                {
                    fraction = ((fraction & 0x000FFFFFFFFFFFFCul) | 0x0010000000000000ul) >> 2;
                    resultExp = 0u;
                }

                result = BitConverter.Int32BitsToSingle(
                    (int)((sign ? 1u : 0u) << 31 | (resultExp & 0xFFu) << 23 | (uint)(fraction >> 29) & 0x007FFFFFu));
            }

            return result;
        }

        public static float FPRecipStepFused(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPRecipStepFused: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPNeg();

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPTwo(false);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else
                {
                    // TODO: When available, use: T MathF.FusedMultiplyAdd(T, T, T);
                    // https://github.com/dotnet/corefx/issues/31903

                    result = 2f + (value1 * value2);

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        public static float FPRecpX(float value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPRecpX: state.Fpcr = 0x{state.CFpcr:X8}");

            value.FPUnpack(out FpType type, out bool sign, out uint op, state);

            float result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else
            {
                uint notExp = (~op >> 23) & 0xFFu;
                uint maxExp = 0xFEu;

                result = BitConverter.Int32BitsToSingle(
                    (int)((sign ? 1u : 0u) << 31 | (notExp == 0xFFu ? maxExp : notExp) << 23));
            }

            return result;
        }

        public static float FPRSqrtEstimate(float value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPRSqrtEstimate: state.Fpcr = 0x{state.CFpcr:X8}");

            value.FPUnpack(out FpType type, out bool sign, out uint op, state);

            float result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else if (type == FpType.Zero)
            {
                result = FPInfinity(sign);

                FPProcessException(FpExc.DivideByZero, state);
            }
            else if (sign)
            {
                result = FPDefaultNaN();

                FPProcessException(FpExc.InvalidOp, state);
            }
            else if (type == FpType.Infinity)
            {
                result = FPZero(false);
            }
            else
            {
                ulong fraction = (ulong)(op & 0x007FFFFFu) << 29;
                uint exp = (op & 0x7F800000u) >> 23;

                if (exp == 0u)
                {
                    while ((fraction & 0x0008000000000000ul) == 0ul)
                    {
                        fraction = (fraction & 0x0007FFFFFFFFFFFFul) << 1;
                        exp -= 1u;
                    }

                    fraction = (fraction & 0x0007FFFFFFFFFFFFul) << 1;
                }

                uint scaled;

                if ((exp & 1u) == 0u)
                {
                    scaled = (uint)(((fraction & 0x000FF00000000000ul) | 0x0010000000000000ul) >> 44);
                }
                else
                {
                    scaled = (uint)(((fraction & 0x000FE00000000000ul) | 0x0010000000000000ul) >> 45);
                }

                uint resultExp = (380u - exp) >> 1;

                uint estimate = (uint)SoftFloat.RecipSqrtEstimateTable[scaled - 128u] + 256u;

                result = BitConverter.Int32BitsToSingle((int)((resultExp & 0xFFu) << 23 | (estimate & 0xFFu) << 15));
            }

            return result;
        }

        public static float FPRSqrtStepFused(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPRSqrtStepFused: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPNeg();

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPOnePointFive(false);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else
                {
                    // TODO: When available, use: T MathF.FusedMultiplyAdd(T, T, T);
                    // https://github.com/dotnet/corefx/issues/31903

                    result = (3f + (value1 * value2)) / 2f;

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        public static float FPSqrt(float value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPSqrt: state.Fpcr = 0x{state.CFpcr:X8}");

            value = value.FPUnpack(out FpType type, out bool sign, out uint op, state);

            float result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else if (type == FpType.Zero)
            {
                result = FPZero(sign);
            }
            else if (type == FpType.Infinity && !sign)
            {
                result = FPInfinity(sign);
            }
            else if (sign)
            {
                result = FPDefaultNaN();

                FPProcessException(FpExc.InvalidOp, state);
            }
            else
            {
                result = MathF.Sqrt(value);

                if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                {
                    state.SetFpsrFlag(Fpsr.Ufc);

                    result = FPZero(result < 0f);
                }
            }

            return result;
        }

        public static float FPSub(float value1, float value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat32.FPSub: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out uint op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out uint op2, state);

            float result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if (inf1 && inf2 && sign1 == sign2)
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if ((inf1 && !sign1) || (inf2 && sign2))
                {
                    result = FPInfinity(false);
                }
                else if ((inf1 && sign1) || (inf2 && !sign2))
                {
                    result = FPInfinity(true);
                }
                else if (zero1 && zero2 && sign1 == !sign2)
                {
                    result = FPZero(sign1);
                }
                else
                {
                    result = value1 - value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && float.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0f);
                    }
                }
            }

            return result;
        }

        private static float FPDefaultNaN()
        {
            return -float.NaN;
        }

        private static float FPInfinity(bool sign)
        {
            return sign ? float.NegativeInfinity : float.PositiveInfinity;
        }

        private static float FPZero(bool sign)
        {
            return sign ? -0f : +0f;
        }

        private static float FPMaxNormal(bool sign)
        {
            return sign ? float.MinValue : float.MaxValue;
        }

        private static float FPTwo(bool sign)
        {
            return sign ? -2f : +2f;
        }

        private static float FPOnePointFive(bool sign)
        {
            return sign ? -1.5f : +1.5f;
        }

        private static float FPNeg(this float value)
        {
            return -value;
        }

        private static float ZerosOrOnes(bool ones)
        {
            return BitConverter.Int32BitsToSingle(ones ? -1 : 0);
        }

        private static float FPUnpack(
            this float value,
            out FpType type,
            out bool sign,
            out uint valueBits,
            CpuThreadState state)
        {
            valueBits = (uint)BitConverter.SingleToInt32Bits(value);

            sign = (~valueBits & 0x80000000u) == 0u;

            if ((valueBits & 0x7F800000u) == 0u)
            {
                if ((valueBits & 0x007FFFFFu) == 0u || state.GetFpcrFlag(Fpcr.Fz))
                {
                    type  = FpType.Zero;
                    value = FPZero(sign);

                    if ((valueBits & 0x007FFFFFu) != 0u)
                    {
                        FPProcessException(FpExc.InputDenorm, state);
                    }
                }
                else
                {
                    type = FpType.Nonzero;
                }
            }
            else if ((~valueBits & 0x7F800000u) == 0u)
            {
                if ((valueBits & 0x007FFFFFu) == 0u)
                {
                    type = FpType.Infinity;
                }
                else
                {
                    type  = (~valueBits & 0x00400000u) == 0u ? FpType.QNaN : FpType.SNaN;
                    value = FPZero(sign);
                }
            }
            else
            {
                type = FpType.Nonzero;
            }

            return value;
        }

        private static float FPProcessNaNs(
            FpType type1,
            FpType type2,
            uint op1,
            uint op2,
            out bool done,
            CpuThreadState state)
        {
            done = true;

            if (type1 == FpType.SNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.SNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }
            else if (type1 == FpType.QNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.QNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }

            done = false;

            return FPZero(false);
        }

        private static float FPProcessNaNs3(
            FpType type1,
            FpType type2,
            FpType type3,
            uint op1,
            uint op2,
            uint op3,
            out bool done,
            CpuThreadState state)
        {
            done = true;

            if (type1 == FpType.SNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.SNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }
            else if (type3 == FpType.SNaN)
            {
                return FPProcessNaN(type3, op3, state);
            }
            else if (type1 == FpType.QNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.QNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }
            else if (type3 == FpType.QNaN)
            {
                return FPProcessNaN(type3, op3, state);
            }

            done = false;

            return FPZero(false);
        }

        private static float FPProcessNaN(FpType type, uint op, CpuThreadState state)
        {
            if (type == FpType.SNaN)
            {
                op |= 1u << 22;

                FPProcessException(FpExc.InvalidOp, state);
            }

            if (state.GetFpcrFlag(Fpcr.Dn))
            {
                return FPDefaultNaN();
            }

            return BitConverter.Int32BitsToSingle((int)op);
        }

        private static void FPProcessException(FpExc exc, CpuThreadState state)
        {
            int enable = (int)exc + 8;

            if ((state.CFpcr & (1 << enable)) != 0)
            {
                throw new NotImplementedException("Floating-point trap handling.");
            }
            else
            {
                state.CFpsr |= 1 << (int)exc;
            }
        }
    }

    static class SoftFloat64
    {
        public static double FPAdd(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPAdd: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if (inf1 && inf2 && sign1 == !sign2)
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if ((inf1 && !sign1) || (inf2 && !sign2))
                {
                    result = FPInfinity(false);
                }
                else if ((inf1 && sign1) || (inf2 && sign2))
                {
                    result = FPInfinity(true);
                }
                else if (zero1 && zero2 && sign1 == sign2)
                {
                    result = FPZero(sign1);
                }
                else
                {
                    result = value1 + value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        public static int FPCompare(double value1, double value2, bool signalNaNs, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPCompare: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out _, state);

            int result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = 0b0011;

                if (type1 == FpType.SNaN || type2 == FpType.SNaN || signalNaNs)
                {
                    FPProcessException(FpExc.InvalidOp, state);
                }
            }
            else
            {
                if (value1 == value2)
                {
                    result = 0b0110;
                }
                else if (value1 < value2)
                {
                    result = 0b1000;
                }
                else
                {
                    result = 0b0010;
                }
            }

            return result;
        }

        public static double FPCompareEQ(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPCompareEQ: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out _, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out _, out _, state);

            double result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = ZerosOrOnes(false);

                if (type1 == FpType.SNaN || type2 == FpType.SNaN)
                {
                    FPProcessException(FpExc.InvalidOp, state);
                }
            }
            else
            {
                result = ZerosOrOnes(value1 == value2);
            }

            return result;
        }

        public static double FPCompareGE(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPCompareGE: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out _, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out _, out _, state);

            double result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = ZerosOrOnes(false);

                FPProcessException(FpExc.InvalidOp, state);
            }
            else
            {
                result = ZerosOrOnes(value1 >= value2);
            }

            return result;
        }

        public static double FPCompareGT(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPCompareGT: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out _, out _, state);
            value2 = value2.FPUnpack(out FpType type2, out _, out _, state);

            double result;

            if (type1 == FpType.SNaN || type1 == FpType.QNaN || type2 == FpType.SNaN || type2 == FpType.QNaN)
            {
                result = ZerosOrOnes(false);

                FPProcessException(FpExc.InvalidOp, state);
            }
            else
            {
                result = ZerosOrOnes(value1 > value2);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FPCompareLE(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPCompareLE: state.Fpcr = 0x{state.CFpcr:X8}");

            return FPCompareGE(value2, value1, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FPCompareLT(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPCompareLT: state.Fpcr = 0x{state.CFpcr:X8}");

            return FPCompareGT(value2, value1, state);
        }

        public static double FPDiv(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPDiv: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && inf2) || (zero1 && zero2))
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if (inf1 || zero2)
                {
                    result = FPInfinity(sign1 ^ sign2);

                    if (!inf1)
                    {
                        FPProcessException(FpExc.DivideByZero, state);
                    }
                }
                else if (zero1 || inf2)
                {
                    result = FPZero(sign1 ^ sign2);
                }
                else
                {
                    result = value1 / value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        public static double FPMax(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMax: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                if (value1 > value2)
                {
                    if (type1 == FpType.Infinity)
                    {
                        result = FPInfinity(sign1);
                    }
                    else if (type1 == FpType.Zero)
                    {
                        result = FPZero(sign1 && sign2);
                    }
                    else
                    {
                        result = value1;
                    }
                }
                else
                {
                    if (type2 == FpType.Infinity)
                    {
                        result = FPInfinity(sign2);
                    }
                    else if (type2 == FpType.Zero)
                    {
                        result = FPZero(sign1 && sign2);
                    }
                    else
                    {
                        result = value2;

                        if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                        {
                            state.SetFpsrFlag(Fpsr.Ufc);

                            result = FPZero(result < 0d);
                        }
                    }
                }
            }

            return result;
        }

        public static double FPMaxNum(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMaxNum: state.Fpcr = 0x{state.CFpcr:X8}");

            value1.FPUnpack(out FpType type1, out _, out _, state);
            value2.FPUnpack(out FpType type2, out _, out _, state);

            if (type1 == FpType.QNaN && type2 != FpType.QNaN)
            {
                value1 = FPInfinity(true);
            }
            else if (type1 != FpType.QNaN && type2 == FpType.QNaN)
            {
                value2 = FPInfinity(true);
            }

            return FPMax(value1, value2, state);
        }

        public static double FPMin(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMin: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                if (value1 < value2)
                {
                    if (type1 == FpType.Infinity)
                    {
                        result = FPInfinity(sign1);
                    }
                    else if (type1 == FpType.Zero)
                    {
                        result = FPZero(sign1 || sign2);
                    }
                    else
                    {
                        result = value1;
                    }
                }
                else
                {
                    if (type2 == FpType.Infinity)
                    {
                        result = FPInfinity(sign2);
                    }
                    else if (type2 == FpType.Zero)
                    {
                        result = FPZero(sign1 || sign2);
                    }
                    else
                    {
                        result = value2;

                        if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                        {
                            state.SetFpsrFlag(Fpsr.Ufc);

                            result = FPZero(result < 0d);
                        }
                    }
                }
            }

            return result;
        }

        public static double FPMinNum(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMinNum: state.Fpcr = 0x{state.CFpcr:X8}");

            value1.FPUnpack(out FpType type1, out _, out _, state);
            value2.FPUnpack(out FpType type2, out _, out _, state);

            if (type1 == FpType.QNaN && type2 != FpType.QNaN)
            {
                value1 = FPInfinity(false);
            }
            else if (type1 != FpType.QNaN && type2 == FpType.QNaN)
            {
                value2 = FPInfinity(false);
            }

            return FPMin(value1, value2, state);
        }

        public static double FPMul(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMul: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else if (zero1 || zero2)
                {
                    result = FPZero(sign1 ^ sign2);
                }
                else
                {
                    result = value1 * value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        public static double FPMulAdd(
            double valueA,
            double value1,
            double value2,
            CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMulAdd: state.Fpcr = 0x{state.CFpcr:X8}");

            valueA = valueA.FPUnpack(out FpType typeA, out bool signA, out ulong addend, state);
            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1,    state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2,    state);

            bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
            bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

            double result = FPProcessNaNs3(typeA, type1, type2, addend, op1, op2, out bool done, state);

            if (typeA == FpType.QNaN && ((inf1 && zero2) || (zero1 && inf2)))
            {
                result = FPDefaultNaN();

                FPProcessException(FpExc.InvalidOp, state);
            }

            if (!done)
            {
                bool infA = typeA == FpType.Infinity; bool zeroA = typeA == FpType.Zero;

                bool signP = sign1 ^  sign2;
                bool infP  = inf1  || inf2;
                bool zeroP = zero1 || zero2;

                if ((inf1 && zero2) || (zero1 && inf2) || (infA && infP && signA != signP))
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if ((infA && !signA) || (infP && !signP))
                {
                    result = FPInfinity(false);
                }
                else if ((infA && signA) || (infP && signP))
                {
                    result = FPInfinity(true);
                }
                else if (zeroA && zeroP && signA == signP)
                {
                    result = FPZero(signA);
                }
                else
                {
                    // TODO: When available, use: T Math.FusedMultiplyAdd(T, T, T);
                    // https://github.com/dotnet/corefx/issues/31903

                    result = valueA + (value1 * value2);

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FPMulSub(
            double valueA,
            double value1,
            double value2,
            CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMulSub: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPNeg();

            return FPMulAdd(valueA, value1, value2, state);
        }

        public static double FPMulX(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPMulX: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPTwo(sign1 ^ sign2);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else if (zero1 || zero2)
                {
                    result = FPZero(sign1 ^ sign2);
                }
                else
                {
                    result = value1 * value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        public static double FPRecipEstimate(double value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPRecipEstimate: state.Fpcr = 0x{state.CFpcr:X8}");

            value.FPUnpack(out FpType type, out bool sign, out ulong op, state);

            double result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else if (type == FpType.Infinity)
            {
                result = FPZero(sign);
            }
            else if (type == FpType.Zero)
            {
                result = FPInfinity(sign);

                FPProcessException(FpExc.DivideByZero, state);
            }
            else if (Math.Abs(value) < Math.Pow(2d, -1024))
            {
                bool overflowToInf;

                switch (state.FPRoundingMode())
                {
                    default:
                    case RoundMode.ToNearest:            overflowToInf = true;  break;
                    case RoundMode.TowardsPlusInfinity:  overflowToInf = !sign; break;
                    case RoundMode.TowardsMinusInfinity: overflowToInf = sign;  break;
                    case RoundMode.TowardsZero:          overflowToInf = false; break;
                }

                result = overflowToInf ? FPInfinity(sign) : FPMaxNormal(sign);

                FPProcessException(FpExc.Overflow, state);
                FPProcessException(FpExc.Inexact,  state);
            }
            else if (state.GetFpcrFlag(Fpcr.Fz) && (Math.Abs(value) >= Math.Pow(2d, 1022)))
            {
                result = FPZero(sign);

                state.SetFpsrFlag(Fpsr.Ufc);
            }
            else
            {
                ulong fraction = op & 0x000FFFFFFFFFFFFFul;
                uint exp = (uint)((op & 0x7FF0000000000000ul) >> 52);

                if (exp == 0u)
                {
                    if ((fraction & 0x0008000000000000ul) == 0ul)
                    {
                        fraction = (fraction & 0x0003FFFFFFFFFFFFul) << 2;
                        exp -= 1u;
                    }
                    else
                    {
                        fraction = (fraction & 0x0007FFFFFFFFFFFFul) << 1;
                    }
                }

                uint scaled = (uint)(((fraction & 0x000FF00000000000ul) | 0x0010000000000000ul) >> 44);

                uint resultExp = 2045u - exp;

                uint estimate = (uint)SoftFloat.RecipEstimateTable[scaled - 256u] + 256u;

                fraction = (ulong)(estimate & 0xFFu) << 44;

                if (resultExp == 0u)
                {
                    fraction = ((fraction & 0x000FFFFFFFFFFFFEul) | 0x0010000000000000ul) >> 1;
                }
                else if (resultExp + 1u == 0u)
                {
                    fraction = ((fraction & 0x000FFFFFFFFFFFFCul) | 0x0010000000000000ul) >> 2;
                    resultExp = 0u;
                }

                result = BitConverter.Int64BitsToDouble(
                    (long)((sign ? 1ul : 0ul) << 63 | (resultExp & 0x7FFul) << 52 | (fraction & 0x000FFFFFFFFFFFFFul)));
            }

            return result;
        }

        public static double FPRecipStepFused(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPRecipStepFused: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPNeg();

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPTwo(false);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else
                {
                    // TODO: When available, use: T Math.FusedMultiplyAdd(T, T, T);
                    // https://github.com/dotnet/corefx/issues/31903

                    result = 2d + (value1 * value2);

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        public static double FPRecpX(double value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPRecpX: state.Fpcr = 0x{state.CFpcr:X8}");

            value.FPUnpack(out FpType type, out bool sign, out ulong op, state);

            double result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else
            {
                ulong notExp = (~op >> 52) & 0x7FFul;
                ulong maxExp = 0x7FEul;

                result = BitConverter.Int64BitsToDouble(
                    (long)((sign ? 1ul : 0ul) << 63 | (notExp == 0x7FFul ? maxExp : notExp) << 52));
            }

            return result;
        }

        public static double FPRSqrtEstimate(double value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPRSqrtEstimate: state.Fpcr = 0x{state.CFpcr:X8}");

            value.FPUnpack(out FpType type, out bool sign, out ulong op, state);

            double result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else if (type == FpType.Zero)
            {
                result = FPInfinity(sign);

                FPProcessException(FpExc.DivideByZero, state);
            }
            else if (sign)
            {
                result = FPDefaultNaN();

                FPProcessException(FpExc.InvalidOp, state);
            }
            else if (type == FpType.Infinity)
            {
                result = FPZero(false);
            }
            else
            {
                ulong fraction = op & 0x000FFFFFFFFFFFFFul;
                uint exp = (uint)((op & 0x7FF0000000000000ul) >> 52);

                if (exp == 0u)
                {
                    while ((fraction & 0x0008000000000000ul) == 0ul)
                    {
                        fraction = (fraction & 0x0007FFFFFFFFFFFFul) << 1;
                        exp -= 1u;
                    }

                    fraction = (fraction & 0x0007FFFFFFFFFFFFul) << 1;
                }

                uint scaled;

                if ((exp & 1u) == 0u)
                {
                    scaled = (uint)(((fraction & 0x000FF00000000000ul) | 0x0010000000000000ul) >> 44);
                }
                else
                {
                    scaled = (uint)(((fraction & 0x000FE00000000000ul) | 0x0010000000000000ul) >> 45);
                }

                uint resultExp = (3068u - exp) >> 1;

                uint estimate = (uint)SoftFloat.RecipSqrtEstimateTable[scaled - 128u] + 256u;

                result = BitConverter.Int64BitsToDouble((long)((resultExp & 0x7FFul) << 52 | (estimate & 0xFFul) << 44));
            }

            return result;
        }

        public static double FPRSqrtStepFused(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPRSqrtStepFused: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPNeg();

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if ((inf1 && zero2) || (zero1 && inf2))
                {
                    result = FPOnePointFive(false);
                }
                else if (inf1 || inf2)
                {
                    result = FPInfinity(sign1 ^ sign2);
                }
                else
                {
                    // TODO: When available, use: T Math.FusedMultiplyAdd(T, T, T);
                    // https://github.com/dotnet/corefx/issues/31903

                    result = (3d + (value1 * value2)) / 2d;

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        public static double FPSqrt(double value, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPSqrt: state.Fpcr = 0x{state.CFpcr:X8}");

            value = value.FPUnpack(out FpType type, out bool sign, out ulong op, state);

            double result;

            if (type == FpType.SNaN || type == FpType.QNaN)
            {
                result = FPProcessNaN(type, op, state);
            }
            else if (type == FpType.Zero)
            {
                result = FPZero(sign);
            }
            else if (type == FpType.Infinity && !sign)
            {
                result = FPInfinity(sign);
            }
            else if (sign)
            {
                result = FPDefaultNaN();

                FPProcessException(FpExc.InvalidOp, state);
            }
            else
            {
                result = Math.Sqrt(value);

                if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                {
                    state.SetFpsrFlag(Fpsr.Ufc);

                    result = FPZero(result < 0d);
                }
            }

            return result;
        }

        public static double FPSub(double value1, double value2, CpuThreadState state)
        {
            Debug.WriteLineIf(state.CFpcr != 0, $"SoftFloat64.FPSub: state.Fpcr = 0x{state.CFpcr:X8}");

            value1 = value1.FPUnpack(out FpType type1, out bool sign1, out ulong op1, state);
            value2 = value2.FPUnpack(out FpType type2, out bool sign2, out ulong op2, state);

            double result = FPProcessNaNs(type1, type2, op1, op2, out bool done, state);

            if (!done)
            {
                bool inf1 = type1 == FpType.Infinity; bool zero1 = type1 == FpType.Zero;
                bool inf2 = type2 == FpType.Infinity; bool zero2 = type2 == FpType.Zero;

                if (inf1 && inf2 && sign1 == sign2)
                {
                    result = FPDefaultNaN();

                    FPProcessException(FpExc.InvalidOp, state);
                }
                else if ((inf1 && !sign1) || (inf2 && sign2))
                {
                    result = FPInfinity(false);
                }
                else if ((inf1 && sign1) || (inf2 && !sign2))
                {
                    result = FPInfinity(true);
                }
                else if (zero1 && zero2 && sign1 == !sign2)
                {
                    result = FPZero(sign1);
                }
                else
                {
                    result = value1 - value2;

                    if (state.GetFpcrFlag(Fpcr.Fz) && double.IsSubnormal(result))
                    {
                        state.SetFpsrFlag(Fpsr.Ufc);

                        result = FPZero(result < 0d);
                    }
                }
            }

            return result;
        }

        private static double FPDefaultNaN()
        {
            return -double.NaN;
        }

        private static double FPInfinity(bool sign)
        {
            return sign ? double.NegativeInfinity : double.PositiveInfinity;
        }

        private static double FPZero(bool sign)
        {
            return sign ? -0d : +0d;
        }

        private static double FPMaxNormal(bool sign)
        {
            return sign ? double.MinValue : double.MaxValue;
        }

        private static double FPTwo(bool sign)
        {
            return sign ? -2d : +2d;
        }

        private static double FPOnePointFive(bool sign)
        {
            return sign ? -1.5d : +1.5d;
        }

        private static double FPNeg(this double value)
        {
            return -value;
        }

        private static double ZerosOrOnes(bool ones)
        {
            return BitConverter.Int64BitsToDouble(ones ? -1L : 0L);
        }

        private static double FPUnpack(
            this double value,
            out FpType type,
            out bool sign,
            out ulong valueBits,
            CpuThreadState state)
        {
            valueBits = (ulong)BitConverter.DoubleToInt64Bits(value);

            sign = (~valueBits & 0x8000000000000000ul) == 0ul;

            if ((valueBits & 0x7FF0000000000000ul) == 0ul)
            {
                if ((valueBits & 0x000FFFFFFFFFFFFFul) == 0ul || state.GetFpcrFlag(Fpcr.Fz))
                {
                    type  = FpType.Zero;
                    value = FPZero(sign);

                    if ((valueBits & 0x000FFFFFFFFFFFFFul) != 0ul)
                    {
                        FPProcessException(FpExc.InputDenorm, state);
                    }
                }
                else
                {
                    type = FpType.Nonzero;
                }
            }
            else if ((~valueBits & 0x7FF0000000000000ul) == 0ul)
            {
                if ((valueBits & 0x000FFFFFFFFFFFFFul) == 0ul)
                {
                    type = FpType.Infinity;
                }
                else
                {
                    type  = (~valueBits & 0x0008000000000000ul) == 0ul ? FpType.QNaN : FpType.SNaN;
                    value = FPZero(sign);
                }
            }
            else
            {
                type = FpType.Nonzero;
            }

            return value;
        }

        private static double FPProcessNaNs(
            FpType type1,
            FpType type2,
            ulong op1,
            ulong op2,
            out bool done,
            CpuThreadState state)
        {
            done = true;

            if (type1 == FpType.SNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.SNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }
            else if (type1 == FpType.QNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.QNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }

            done = false;

            return FPZero(false);
        }

        private static double FPProcessNaNs3(
            FpType type1,
            FpType type2,
            FpType type3,
            ulong op1,
            ulong op2,
            ulong op3,
            out bool done,
            CpuThreadState state)
        {
            done = true;

            if (type1 == FpType.SNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.SNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }
            else if (type3 == FpType.SNaN)
            {
                return FPProcessNaN(type3, op3, state);
            }
            else if (type1 == FpType.QNaN)
            {
                return FPProcessNaN(type1, op1, state);
            }
            else if (type2 == FpType.QNaN)
            {
                return FPProcessNaN(type2, op2, state);
            }
            else if (type3 == FpType.QNaN)
            {
                return FPProcessNaN(type3, op3, state);
            }

            done = false;

            return FPZero(false);
        }

        private static double FPProcessNaN(FpType type, ulong op, CpuThreadState state)
        {
            if (type == FpType.SNaN)
            {
                op |= 1ul << 51;

                FPProcessException(FpExc.InvalidOp, state);
            }

            if (state.GetFpcrFlag(Fpcr.Dn))
            {
                return FPDefaultNaN();
            }

            return BitConverter.Int64BitsToDouble((long)op);
        }

        private static void FPProcessException(FpExc exc, CpuThreadState state)
        {
            int enable = (int)exc + 8;

            if ((state.CFpcr & (1 << enable)) != 0)
            {
                throw new NotImplementedException("Floating-point trap handling.");
            }
            else
            {
                state.CFpsr |= 1 << (int)exc;
            }
        }
    }
}
