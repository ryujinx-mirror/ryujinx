using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instructions
{
    static class VectorHelper
    {
        public static void EmitCall(ILEmitterCtx context, string name64, string name128)
        {
            bool isSimd64 = context.CurrOp.RegisterSize == RegisterSize.Simd64;

            context.EmitCall(typeof(VectorHelper), isSimd64 ? name64 : name128);
        }

        public static void EmitCall(ILEmitterCtx context, string mthdName)
        {
            context.EmitCall(typeof(VectorHelper), mthdName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SatF32ToS32(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= int.MaxValue ? int.MaxValue :
                   value <= int.MinValue ? int.MinValue : (int)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SatF32ToS64(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= long.MaxValue ? long.MaxValue :
                   value <= long.MinValue ? long.MinValue : (long)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SatF32ToU32(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= uint.MaxValue ? uint.MaxValue :
                   value <= uint.MinValue ? uint.MinValue : (uint)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SatF32ToU64(float value)
        {
            if (float.IsNaN(value)) return 0;

            return value >= ulong.MaxValue ? ulong.MaxValue :
                   value <= ulong.MinValue ? ulong.MinValue : (ulong)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SatF64ToS32(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= int.MaxValue ? int.MaxValue :
                   value <= int.MinValue ? int.MinValue : (int)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SatF64ToS64(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= long.MaxValue ? long.MaxValue :
                   value <= long.MinValue ? long.MinValue : (long)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SatF64ToU32(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= uint.MaxValue ? uint.MaxValue :
                   value <= uint.MinValue ? uint.MinValue : (uint)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SatF64ToU64(double value)
        {
            if (double.IsNaN(value)) return 0;

            return value >= ulong.MaxValue ? ulong.MaxValue :
                   value <= ulong.MinValue ? ulong.MinValue : (ulong)value;
        }

        public static double Round(double value, CpuThreadState state)
        {
            switch (state.FPRoundingMode())
            {
                case RoundMode.ToNearest:            return Math.Round   (value);
                case RoundMode.TowardsPlusInfinity:  return Math.Ceiling (value);
                case RoundMode.TowardsMinusInfinity: return Math.Floor   (value);
                case RoundMode.TowardsZero:          return Math.Truncate(value);
            }

            throw new InvalidOperationException();
        }

        public static float RoundF(float value, CpuThreadState state)
        {
            switch (state.FPRoundingMode())
            {
                case RoundMode.ToNearest:            return MathF.Round   (value);
                case RoundMode.TowardsPlusInfinity:  return MathF.Ceiling (value);
                case RoundMode.TowardsMinusInfinity: return MathF.Floor   (value);
                case RoundMode.TowardsZero:          return MathF.Truncate(value);
            }

            throw new InvalidOperationException();
        }

        public static Vector128<float> Tbl1_V64(
            Vector128<float> vector,
            Vector128<float> tb0)
        {
            return Tbl(vector, 8, tb0);
        }

        public static Vector128<float> Tbl1_V128(
            Vector128<float> vector,
            Vector128<float> tb0)
        {
            return Tbl(vector, 16, tb0);
        }

        public static Vector128<float> Tbl2_V64(
            Vector128<float> vector,
            Vector128<float> tb0,
            Vector128<float> tb1)
        {
            return Tbl(vector, 8, tb0, tb1);
        }

        public static Vector128<float> Tbl2_V128(
            Vector128<float> vector,
            Vector128<float> tb0,
            Vector128<float> tb1)
        {
            return Tbl(vector, 16, tb0, tb1);
        }

        public static Vector128<float> Tbl3_V64(
            Vector128<float> vector,
            Vector128<float> tb0,
            Vector128<float> tb1,
            Vector128<float> tb2)
        {
            return Tbl(vector, 8, tb0, tb1, tb2);
        }

        public static Vector128<float> Tbl3_V128(
            Vector128<float> vector,
            Vector128<float> tb0,
            Vector128<float> tb1,
            Vector128<float> tb2)
        {
            return Tbl(vector, 16, tb0, tb1, tb2);
        }

        public static Vector128<float> Tbl4_V64(
            Vector128<float> vector,
            Vector128<float> tb0,
            Vector128<float> tb1,
            Vector128<float> tb2,
            Vector128<float> tb3)
        {
            return Tbl(vector, 8, tb0, tb1, tb2, tb3);
        }

        public static Vector128<float> Tbl4_V128(
            Vector128<float> vector,
            Vector128<float> tb0,
            Vector128<float> tb1,
            Vector128<float> tb2,
            Vector128<float> tb3)
        {
            return Tbl(vector, 16, tb0, tb1, tb2, tb3);
        }

        private static Vector128<float> Tbl(Vector128<float> vector, int bytes, params Vector128<float>[] tb)
        {
            Vector128<float> res = new Vector128<float>();

            byte[] table = new byte[tb.Length * 16];

            for (byte index  = 0; index  < tb.Length; index++)
            for (byte index2 = 0; index2 < 16;        index2++)
            {
                table[index * 16 + index2] = (byte)VectorExtractIntZx(tb[index], index2, 0);
            }

            for (byte index = 0; index < bytes; index++)
            {
                byte tblIdx = (byte)VectorExtractIntZx(vector, index, 0);

                if (tblIdx < table.Length)
                {
                    res = VectorInsertInt(table[tblIdx], res, index, 0);
                }
            }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double VectorExtractDouble(Vector128<float> vector, byte index)
        {
            if (Sse41.IsSupported)
            {
                return BitConverter.Int64BitsToDouble(Sse41.Extract(Sse.StaticCast<float, long>(vector), index));
            }
            else if (Sse2.IsSupported)
            {
                return BitConverter.Int64BitsToDouble((long)VectorExtractIntZx(vector, index, 3));
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long VectorExtractIntSx(Vector128<float> vector, byte index, int size)
        {
            if (Sse41.IsSupported)
            {
                if (size == 0)
                {
                    return (sbyte)Sse41.Extract(Sse.StaticCast<float, byte>(vector), index);
                }
                else if (size == 1)
                {
                    return (short)Sse2.Extract(Sse.StaticCast<float, ushort>(vector), index);
                }
                else if (size == 2)
                {
                    return Sse41.Extract(Sse.StaticCast<float, int>(vector), index);
                }
                else if (size == 3)
                {
                    return Sse41.Extract(Sse.StaticCast<float, long>(vector), index);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }
            else if (Sse2.IsSupported)
            {
                if (size == 0)
                {
                    return (sbyte)VectorExtractIntZx(vector, index, size);
                }
                else if (size == 1)
                {
                    return (short)VectorExtractIntZx(vector, index, size);
                }
                else if (size == 2)
                {
                    return (int)VectorExtractIntZx(vector, index, size);
                }
                else if (size == 3)
                {
                    return (long)VectorExtractIntZx(vector, index, size);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong VectorExtractIntZx(Vector128<float> vector, byte index, int size)
        {
            if (Sse41.IsSupported)
            {
                if (size == 0)
                {
                    return Sse41.Extract(Sse.StaticCast<float, byte>(vector), index);
                }
                else if (size == 1)
                {
                    return Sse2.Extract(Sse.StaticCast<float, ushort>(vector), index);
                }
                else if (size == 2)
                {
                    return Sse41.Extract(Sse.StaticCast<float, uint>(vector), index);
                }
                else if (size == 3)
                {
                    return Sse41.Extract(Sse.StaticCast<float, ulong>(vector), index);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }
            else if (Sse2.IsSupported)
            {
                int shortIdx = size == 0
                    ? index >> 1
                    : index << (size - 1);

                ushort value = Sse2.Extract(Sse.StaticCast<float, ushort>(vector), (byte)shortIdx);

                if (size == 0)
                {
                    return (byte)(value >> (index & 1) * 8);
                }
                else if (size == 1)
                {
                    return value;
                }
                else if (size == 2 || size == 3)
                {
                    ushort value1 = Sse2.Extract(Sse.StaticCast<float, ushort>(vector), (byte)(shortIdx + 1));

                    if (size == 2)
                    {
                        return (uint)(value | (value1 << 16));
                    }

                    ushort value2 = Sse2.Extract(Sse.StaticCast<float, ushort>(vector), (byte)(shortIdx + 2));
                    ushort value3 = Sse2.Extract(Sse.StaticCast<float, ushort>(vector), (byte)(shortIdx + 3));

                    return ((ulong)value  <<  0) |
                           ((ulong)value1 << 16) |
                           ((ulong)value2 << 32) |
                           ((ulong)value3 << 48);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float VectorExtractSingle(Vector128<float> vector, byte index)
        {
            if (Sse41.IsSupported)
            {
                return Sse41.Extract(vector, index);
            }
            else if (Sse2.IsSupported)
            {
                Vector128<ushort> shortVector = Sse.StaticCast<float, ushort>(vector);

                int low  = Sse2.Extract(shortVector, (byte)(index * 2 + 0));
                int high = Sse2.Extract(shortVector, (byte)(index * 2 + 1));

                return BitConverter.Int32BitsToSingle(low | (high << 16));
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertDouble(double value, Vector128<float> vector, byte index)
        {
            return VectorInsertInt((ulong)BitConverter.DoubleToInt64Bits(value), vector, index, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertInt(ulong value, Vector128<float> vector, byte index, int size)
        {
            if (Sse41.IsSupported)
            {
                if (size == 0)
                {
                    return Sse.StaticCast<byte, float>(Sse41.Insert(Sse.StaticCast<float, byte>(vector), (byte)value, index));
                }
                else if (size == 1)
                {
                    return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse.StaticCast<float, ushort>(vector), (ushort)value, index));
                }
                else if (size == 2)
                {
                    return Sse.StaticCast<uint, float>(Sse41.Insert(Sse.StaticCast<float, uint>(vector), (uint)value, index));
                }
                else if (size == 3)
                {
                    return Sse.StaticCast<ulong, float>(Sse41.Insert(Sse.StaticCast<float, ulong>(vector), value, index));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }
            else if (Sse2.IsSupported)
            {
                Vector128<ushort> shortVector = Sse.StaticCast<float, ushort>(vector);

                int shortIdx = size == 0
                    ? index >> 1
                    : index << (size - 1);

                if (size == 0)
                {
                    ushort shortVal = Sse2.Extract(Sse.StaticCast<float, ushort>(vector), (byte)shortIdx);

                    int shift = (index & 1) * 8;

                    shortVal &= (ushort)(0xff00 >> shift);

                    shortVal |= (ushort)((byte)value << shift);

                    return Sse.StaticCast<ushort, float>(Sse2.Insert(shortVector, shortVal, (byte)shortIdx));
                }
                else if (size == 1)
                {
                    return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse.StaticCast<float, ushort>(vector), (ushort)value, index));
                }
                else if (size == 2 || size == 3)
                {
                    shortVector = Sse2.Insert(shortVector, (ushort)(value >>  0), (byte)(shortIdx + 0));
                    shortVector = Sse2.Insert(shortVector, (ushort)(value >> 16), (byte)(shortIdx + 1));

                    if (size == 3)
                    {
                        shortVector = Sse2.Insert(shortVector, (ushort)(value >> 32), (byte)(shortIdx + 2));
                        shortVector = Sse2.Insert(shortVector, (ushort)(value >> 48), (byte)(shortIdx + 3));
                    }

                    return Sse.StaticCast<ushort, float>(shortVector);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertSingle(float value, Vector128<float> vector, byte index)
        {
            if (Sse41.IsSupported)
            {
                //Note: The if/else if is necessary to enable the JIT to
                //produce a single INSERTPS instruction instead of the
                //jump table fallback.
                if (index == 0)
                {
                    return Sse41.Insert(vector, value, 0x00);
                }
                else if (index == 1)
                {
                    return Sse41.Insert(vector, value, 0x10);
                }
                else if (index == 2)
                {
                    return Sse41.Insert(vector, value, 0x20);
                }
                else if (index == 3)
                {
                    return Sse41.Insert(vector, value, 0x30);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
            else if (Sse2.IsSupported)
            {
                int intValue = BitConverter.SingleToInt32Bits(value);

                ushort low  = (ushort)(intValue >>  0);
                ushort high = (ushort)(intValue >> 16);

                Vector128<ushort> shortVector = Sse.StaticCast<float, ushort>(vector);

                shortVector = Sse2.Insert(shortVector, low,  (byte)(index * 2 + 0));
                shortVector = Sse2.Insert(shortVector, high, (byte)(index * 2 + 1));

                return Sse.StaticCast<ushort, float>(shortVector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Sse41VectorInsertScalarSingle(float value, Vector128<float> vector)
        {
            //Note: 0b1110 is the mask to zero the upper bits.
            return Sse41.Insert(vector, value, 0b1110);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorSingleZero()
        {
            if (Sse.IsSupported)
            {
                return Sse.SetZeroVector128();
            }

            throw new PlatformNotSupportedException();
        }
    }
}
