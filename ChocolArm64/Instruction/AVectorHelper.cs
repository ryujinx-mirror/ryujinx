using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instruction
{
    static class AVectorHelper
    {
        private static readonly Vector128<float> Zero32_128Mask;

        static AVectorHelper()
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            Zero32_128Mask = Sse.StaticCast<uint, float>(Sse2.SetVector128(0, 0, 0, 0xffffffff));
        }

        public static void EmitCall(AILEmitterCtx Context, string Name64, string Name128)
        {
            bool IsSimd64 = Context.CurrOp.RegisterSize == ARegisterSize.SIMD64;

            Context.EmitCall(typeof(AVectorHelper), IsSimd64 ? Name64 : Name128);
        }

        public static void EmitCall(AILEmitterCtx Context, string MthdName)
        {
            Context.EmitCall(typeof(AVectorHelper), MthdName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SatF32ToS32(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SatF32ToS64(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SatF32ToU32(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SatF32ToU64(float Value)
        {
            if (float.IsNaN(Value)) return 0;

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SatF64ToS32(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > int.MaxValue ? int.MaxValue :
                   Value < int.MinValue ? int.MinValue : (int)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SatF64ToS64(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > long.MaxValue ? long.MaxValue :
                   Value < long.MinValue ? long.MinValue : (long)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SatF64ToU32(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > uint.MaxValue ? uint.MaxValue :
                   Value < uint.MinValue ? uint.MinValue : (uint)Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SatF64ToU64(double Value)
        {
            if (double.IsNaN(Value)) return 0;

            return Value > ulong.MaxValue ? ulong.MaxValue :
                   Value < ulong.MinValue ? ulong.MinValue : (ulong)Value;
        }

        public static double Round(double Value, AThreadState State)
        {
            switch (State.FPRoundingMode())
            {
                case ARoundMode.ToNearest:            return Math.Round   (Value);
                case ARoundMode.TowardsPlusInfinity:  return Math.Ceiling (Value);
                case ARoundMode.TowardsMinusInfinity: return Math.Floor   (Value);
                case ARoundMode.TowardsZero:          return Math.Truncate(Value);
            }

            throw new InvalidOperationException();
        }

        public static float RoundF(float Value, AThreadState State)
        {
            switch (State.FPRoundingMode())
            {
                case ARoundMode.ToNearest:            return MathF.Round   (Value);
                case ARoundMode.TowardsPlusInfinity:  return MathF.Ceiling (Value);
                case ARoundMode.TowardsMinusInfinity: return MathF.Floor   (Value);
                case ARoundMode.TowardsZero:          return MathF.Truncate(Value);
            }

            throw new InvalidOperationException();
        }

        public static Vector128<float> Tbl1_V64(
            Vector128<float> Vector,
            Vector128<float> Tb0)
        {
            return Tbl(Vector, 8, Tb0);
        }

        public static Vector128<float> Tbl1_V128(
            Vector128<float> Vector,
            Vector128<float> Tb0)
        {
            return Tbl(Vector, 16, Tb0);
        }

        public static Vector128<float> Tbl2_V64(
            Vector128<float> Vector,
            Vector128<float> Tb0,
            Vector128<float> Tb1)
        {
            return Tbl(Vector, 8, Tb0, Tb1);
        }

        public static Vector128<float> Tbl2_V128(
            Vector128<float> Vector,
            Vector128<float> Tb0,
            Vector128<float> Tb1)
        {
            return Tbl(Vector, 16, Tb0, Tb1);
        }

        public static Vector128<float> Tbl3_V64(
            Vector128<float> Vector,
            Vector128<float> Tb0,
            Vector128<float> Tb1,
            Vector128<float> Tb2)
        {
            return Tbl(Vector, 8, Tb0, Tb1, Tb2);
        }

        public static Vector128<float> Tbl3_V128(
            Vector128<float> Vector,
            Vector128<float> Tb0,
            Vector128<float> Tb1,
            Vector128<float> Tb2)
        {
            return Tbl(Vector, 16, Tb0, Tb1, Tb2);
        }

        public static Vector128<float> Tbl4_V64(
            Vector128<float> Vector,
            Vector128<float> Tb0,
            Vector128<float> Tb1,
            Vector128<float> Tb2,
            Vector128<float> Tb3)
        {
            return Tbl(Vector, 8, Tb0, Tb1, Tb2, Tb3);
        }

        public static Vector128<float> Tbl4_V128(
            Vector128<float> Vector,
            Vector128<float> Tb0,
            Vector128<float> Tb1,
            Vector128<float> Tb2,
            Vector128<float> Tb3)
        {
            return Tbl(Vector, 16, Tb0, Tb1, Tb2, Tb3);
        }

        private static Vector128<float> Tbl(Vector128<float> Vector, int Bytes, params Vector128<float>[] Tb)
        {
            Vector128<float> Res = new Vector128<float>();

            byte[] Table = new byte[Tb.Length * 16];

            for (byte Index  = 0; Index  < Tb.Length; Index++)
            for (byte Index2 = 0; Index2 < 16;        Index2++)
            {
                Table[Index * 16 + Index2] = (byte)VectorExtractIntZx(Tb[Index], Index2, 0);
            }

            for (byte Index = 0; Index < Bytes; Index++)
            {
                byte TblIdx = (byte)VectorExtractIntZx(Vector, Index, 0);

                if (TblIdx < Table.Length)
                {
                    Res = VectorInsertInt(Table[TblIdx], Res, Index, 0);
                }
            }

            return Res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double VectorExtractDouble(Vector128<float> Vector, byte Index)
        {
            if (Sse41.IsSupported)
            {
                return BitConverter.Int64BitsToDouble(Sse41.Extract(Sse.StaticCast<float, long>(Vector), Index));
            }
            else if (Sse2.IsSupported)
            {
                return BitConverter.Int64BitsToDouble((long)VectorExtractIntZx(Vector, Index, 3));
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long VectorExtractIntSx(Vector128<float> Vector, byte Index, int Size)
        {
            if (Sse41.IsSupported)
            {
                if (Size == 0)
                {
                    return (sbyte)Sse41.Extract(Sse.StaticCast<float, byte>(Vector), Index);
                }
                else if (Size == 1)
                {
                    return (short)Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), Index);
                }
                else if (Size == 2)
                {
                    return Sse41.Extract(Sse.StaticCast<float, int>(Vector), Index);
                }
                else if (Size == 3)
                {
                    return Sse41.Extract(Sse.StaticCast<float, long>(Vector), Index);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }
            }
            else if (Sse2.IsSupported)
            {
                if (Size == 0)
                {
                    return (sbyte)VectorExtractIntZx(Vector, Index, Size);
                }
                else if (Size == 1)
                {
                    return (short)VectorExtractIntZx(Vector, Index, Size);
                }
                else if (Size == 2)
                {
                    return (int)VectorExtractIntZx(Vector, Index, Size);
                }
                else if (Size == 3)
                {
                    return (long)VectorExtractIntZx(Vector, Index, Size);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong VectorExtractIntZx(Vector128<float> Vector, byte Index, int Size)
        {
            if (Sse41.IsSupported)
            {
                if (Size == 0)
                {
                    return Sse41.Extract(Sse.StaticCast<float, byte>(Vector), Index);
                }
                else if (Size == 1)
                {
                    return Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), Index);
                }
                else if (Size == 2)
                {
                    return Sse41.Extract(Sse.StaticCast<float, uint>(Vector), Index);
                }
                else if (Size == 3)
                {
                    return Sse41.Extract(Sse.StaticCast<float, ulong>(Vector), Index);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }
            }
            else if (Sse2.IsSupported)
            {
                int ShortIdx = Size == 0
                    ? Index >> 1
                    : Index << (Size - 1);

                ushort Value = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)ShortIdx);

                if (Size == 0)
                {
                    return (byte)(Value >> (Index & 1) * 8);
                }
                else if (Size == 1)
                {
                    return Value;
                }
                else if (Size == 2 || Size == 3)
                {
                    ushort Value1 = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)(ShortIdx + 1));

                    if (Size == 2)
                    {
                        return (uint)(Value | (Value1 << 16));
                    }

                    ushort Value2 = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)(ShortIdx + 2));
                    ushort Value3 = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)(ShortIdx + 3));

                    return ((ulong)Value  <<  0) |
                           ((ulong)Value1 << 16) |
                           ((ulong)Value2 << 32) |
                           ((ulong)Value3 << 48);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float VectorExtractSingle(Vector128<float> Vector, byte Index)
        {
            if (Sse41.IsSupported)
            {
                return Sse41.Extract(Vector, Index);
            }
            else if (Sse2.IsSupported)
            {
                Vector128<ushort> ShortVector = Sse.StaticCast<float, ushort>(Vector);

                int Low  = Sse2.Extract(ShortVector, (byte)(Index * 2 + 0));
                int High = Sse2.Extract(ShortVector, (byte)(Index * 2 + 1));

                return BitConverter.Int32BitsToSingle(Low | (High << 16));
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertDouble(double Value, Vector128<float> Vector, byte Index)
        {
            return VectorInsertInt((ulong)BitConverter.DoubleToInt64Bits(Value), Vector, Index, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertInt(ulong Value, Vector128<float> Vector, byte Index, int Size)
        {
            if (Sse41.IsSupported)
            {
                if (Size == 0)
                {
                    return Sse.StaticCast<byte, float>(Sse41.Insert(Sse.StaticCast<float, byte>(Vector), (byte)Value, Index));
                }
                else if (Size == 1)
                {
                    return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse.StaticCast<float, ushort>(Vector), (ushort)Value, Index));
                }
                else if (Size == 2)
                {
                    return Sse.StaticCast<uint, float>(Sse41.Insert(Sse.StaticCast<float, uint>(Vector), (uint)Value, Index));
                }
                else if (Size == 3)
                {
                    return Sse.StaticCast<ulong, float>(Sse41.Insert(Sse.StaticCast<float, ulong>(Vector), Value, Index));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }
            }
            else if (Sse2.IsSupported)
            {
                Vector128<ushort> ShortVector = Sse.StaticCast<float, ushort>(Vector);

                int ShortIdx = Size == 0
                    ? Index >> 1
                    : Index << (Size - 1);

                if (Size == 0)
                {
                    ushort ShortVal = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)ShortIdx);

                    int Shift = (Index & 1) * 8;

                    ShortVal &= (ushort)(0xff00 >> Shift);

                    ShortVal |= (ushort)((byte)Value << Shift);

                    return Sse.StaticCast<ushort, float>(Sse2.Insert(ShortVector, ShortVal, (byte)ShortIdx));
                }
                else if (Size == 1)
                {
                    return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse.StaticCast<float, ushort>(Vector), (ushort)Value, Index));
                }
                else if (Size == 2 || Size == 3)
                {
                    ShortVector = Sse2.Insert(ShortVector, (ushort)(Value >>  0), (byte)(ShortIdx + 0));
                    ShortVector = Sse2.Insert(ShortVector, (ushort)(Value >> 16), (byte)(ShortIdx + 1));

                    if (Size == 3)
                    {
                        ShortVector = Sse2.Insert(ShortVector, (ushort)(Value >> 32), (byte)(ShortIdx + 2));
                        ShortVector = Sse2.Insert(ShortVector, (ushort)(Value >> 48), (byte)(ShortIdx + 3));
                    }

                    return Sse.StaticCast<ushort, float>(ShortVector);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertSingle(float Value, Vector128<float> Vector, byte Index)
        {
            if (Sse41.IsSupported)
            {
                //Note: The if/else if is necessary to enable the JIT to
                //produce a single INSERTPS instruction instead of the
                //jump table fallback.
                if (Index == 0)
                {
                    return Sse41.Insert(Vector, Value, 0x00);
                }
                else if (Index == 1)
                {
                    return Sse41.Insert(Vector, Value, 0x10);
                }
                else if (Index == 2)
                {
                    return Sse41.Insert(Vector, Value, 0x20);
                }
                else if (Index == 3)
                {
                    return Sse41.Insert(Vector, Value, 0x30);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Index));
                }
            }
            else if (Sse2.IsSupported)
            {
                int IntValue = BitConverter.SingleToInt32Bits(Value);

                ushort Low  = (ushort)(IntValue >> 0);
                ushort High = (ushort)(IntValue >> 16);

                Vector128<ushort> ShortVector = Sse.StaticCast<float, ushort>(Vector);

                ShortVector = Sse2.Insert(ShortVector, Low,  (byte)(Index * 2 + 0));
                ShortVector = Sse2.Insert(ShortVector, High, (byte)(Index * 2 + 1));

                return Sse.StaticCast<ushort, float>(ShortVector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Sse41VectorInsertScalarSingle(float Value, Vector128<float> Vector)
        {
            //Note: 0b1110 is the mask to zero the upper bits.
            return Sse41.Insert(Vector, Value, 0b1110);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<sbyte> VectorSByteZero()
        {
            if (Sse2.IsSupported)
            {
                return Sse2.SetZeroVector128<sbyte>();
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<short> VectorInt16Zero()
        {
            if (Sse2.IsSupported)
            {
                return Sse2.SetZeroVector128<short>();
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> VectorInt32Zero()
        {
            if (Sse2.IsSupported)
            {
                return Sse2.SetZeroVector128<int>();
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<long> VectorInt64Zero()
        {
            if (Sse2.IsSupported)
            {
                return Sse2.SetZeroVector128<long>();
            }

            throw new PlatformNotSupportedException();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<double> VectorDoubleZero()
        {
            if (Sse2.IsSupported)
            {
                return Sse2.SetZeroVector128<double>();
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorZero32_128(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.And(Vector, Zero32_128Mask);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<sbyte> VectorSingleToSByte(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, sbyte>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<short> VectorSingleToInt16(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, short>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> VectorSingleToInt32(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, int>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<long> VectorSingleToInt64(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, long>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<byte> VectorSingleToByte(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, byte>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ushort> VectorSingleToUInt16(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, ushort>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<uint> VectorSingleToUInt32(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, uint>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ulong> VectorSingleToUInt64(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, ulong>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<double> VectorSingleToDouble(Vector128<float> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<float, double>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorSByteToSingle(Vector128<sbyte> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<sbyte, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInt16ToSingle(Vector128<short> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<short, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInt32ToSingle(Vector128<int> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<int, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInt64ToSingle(Vector128<long> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<long, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorByteToSingle(Vector128<byte> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<byte, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorUInt16ToSingle(Vector128<ushort> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<ushort, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorUInt32ToSingle(Vector128<uint> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<uint, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorUInt64ToSingle(Vector128<ulong> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<ulong, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorDoubleToSingle(Vector128<double> Vector)
        {
            if (Sse.IsSupported)
            {
                return Sse.StaticCast<double, float>(Vector);
            }

            throw new PlatformNotSupportedException();
        }
    }
}
