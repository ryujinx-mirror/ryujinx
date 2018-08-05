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

        public static double Round(double Value, int Fpcr)
        {
            switch ((ARoundMode)((Fpcr >> 22) & 3))
            {
                case ARoundMode.ToNearest:            return Math.Round   (Value);
                case ARoundMode.TowardsPlusInfinity:  return Math.Ceiling (Value);
                case ARoundMode.TowardsMinusInfinity: return Math.Floor   (Value);
                case ARoundMode.TowardsZero:          return Math.Truncate(Value);
            }

            throw new InvalidOperationException();
        }

        public static float RoundF(float Value, int Fpcr)
        {
            switch ((ARoundMode)((Fpcr >> 22) & 3))
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
            return BitConverter.Int64BitsToDouble(VectorExtractIntSx(Vector, Index, 3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long VectorExtractIntSx(Vector128<float> Vector, byte Index, int Size)
        {
            if (Sse41.IsSupported)
            {
                switch (Size)
                {
                    case 0:
                        return (sbyte)Sse41.Extract(Sse.StaticCast<float, byte>(Vector), Index);

                    case 1:
                        return (short)Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), Index);

                    case 2:
                        return Sse41.Extract(Sse.StaticCast<float, int>(Vector), Index);

                    case 3:
                        return Sse41.Extract(Sse.StaticCast<float, long>(Vector), Index);
                }

                throw new ArgumentOutOfRangeException(nameof(Size));
            }
            else if (Sse2.IsSupported)
            {
                switch (Size)
                {
                    case 0:
                        return (sbyte)VectorExtractIntZx(Vector, Index, Size);

                    case 1:
                        return (short)VectorExtractIntZx(Vector, Index, Size);

                    case 2:
                        return (int)VectorExtractIntZx(Vector, Index, Size);

                    case 3:
                        return (long)VectorExtractIntZx(Vector, Index, Size);
                }

                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong VectorExtractIntZx(Vector128<float> Vector, byte Index, int Size)
        {
            if (Sse41.IsSupported)
            {
                switch (Size)
                {
                    case 0:
                        return Sse41.Extract(Sse.StaticCast<float, byte>(Vector), Index);

                    case 1:
                        return Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), Index);

                    case 2:
                        return Sse41.Extract(Sse.StaticCast<float, uint>(Vector), Index);

                    case 3:
                        return Sse41.Extract(Sse.StaticCast<float, ulong>(Vector), Index);
                }

                throw new ArgumentOutOfRangeException(nameof(Size));
            }
            else if (Sse2.IsSupported)
            {
                int ShortIdx = Size == 0
                    ? Index >> 1
                    : Index << (Size - 1);

                ushort Value = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)ShortIdx);

                switch (Size)
                {
                    case 0:
                        return (byte)(Value >> (Index & 1) * 8);

                    case 1:
                        return Value;

                    case 2:
                    case 3:
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
                }

                throw new ArgumentOutOfRangeException(nameof(Size));
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
                switch (Size)
                {
                    case 0:
                        return Sse.StaticCast<byte, float>(Sse41.Insert(Sse.StaticCast<float, byte>(Vector), (byte)Value, Index));

                    case 1:
                        return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse.StaticCast<float, ushort>(Vector), (ushort)Value, Index));

                    case 2:
                        return Sse.StaticCast<uint, float>(Sse41.Insert(Sse.StaticCast<float, uint>(Vector), (uint)Value, Index));

                    case 3:
                        return Sse.StaticCast<ulong, float>(Sse41.Insert(Sse.StaticCast<float, ulong>(Vector), Value, Index));
                }

                throw new ArgumentOutOfRangeException(nameof(Size));
            }
            else if (Sse2.IsSupported)
            {
                Vector128<ushort> ShortVector = Sse.StaticCast<float, ushort>(Vector);

                int ShortIdx = Size == 0
                    ? Index >> 1
                    : Index << (Size - 1);

                switch (Size)
                {
                    case 0:
                    {
                        ushort ShortVal = Sse2.Extract(Sse.StaticCast<float, ushort>(Vector), (byte)ShortIdx);

                        int Shift = (Index & 1) * 8;

                        ShortVal &= (ushort)(0xff00 >> Shift);

                        ShortVal |= (ushort)((byte)Value << Shift);

                        return Sse.StaticCast<ushort, float>(Sse2.Insert(ShortVector, ShortVal, (byte)ShortIdx));
                    }

                    case 1:
                        return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse.StaticCast<float, ushort>(Vector), (ushort)Value, Index));

                    case 2:
                    case 3:
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
                }

                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> VectorInsertSingle(float Value, Vector128<float> Vector, byte Index)
        {
            if (Sse41.IsSupported)
            {
                return Sse41.Insert(Vector, Value, (byte)(Index << 4));
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
