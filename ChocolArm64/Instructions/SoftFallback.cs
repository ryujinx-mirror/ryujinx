using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instructions
{
    using static VectorHelper;

    static class SoftFallback
    {
        public static void EmitCall(ILEmitterCtx context, string mthdName)
        {
            context.EmitCall(typeof(SoftFallback), mthdName);
        }

#region "ShlReg"
        public static long SignedShlReg(long value, long shift, bool round, int size)
        {
            int eSize = 8 << size;

            int shiftLsB = (sbyte)shift;

            if (shiftLsB < 0)
            {
                return SignedShrReg(value, -shiftLsB, round, eSize);
            }
            else if (shiftLsB > 0)
            {
                if (shiftLsB >= eSize)
                {
                    return 0L;
                }

                return value << shiftLsB;
            }
            else /* if (shiftLsB == 0) */
            {
                return value;
            }
        }

        public static ulong UnsignedShlReg(ulong value, ulong shift, bool round, int size)
        {
            int eSize = 8 << size;

            int shiftLsB = (sbyte)shift;

            if (shiftLsB < 0)
            {
                return UnsignedShrReg(value, -shiftLsB, round, eSize);
            }
            else if (shiftLsB > 0)
            {
                if (shiftLsB >= eSize)
                {
                    return 0UL;
                }

                return value << shiftLsB;
            }
            else /* if (shiftLsB == 0) */
            {
                return value;
            }
        }

        public static long SignedShlRegSatQ(long value, long shift, bool round, int size, CpuThreadState state)
        {
            int eSize = 8 << size;

            int shiftLsB = (sbyte)shift;

            if (shiftLsB < 0)
            {
                return SignedShrReg(value, -shiftLsB, round, eSize);
            }
            else if (shiftLsB > 0)
            {
                if (shiftLsB >= eSize)
                {
                    return SignedSignSatQ(value, eSize, state);
                }

                if (eSize == 64)
                {
                    long shl = value << shiftLsB;
                    long shr = shl   >> shiftLsB;

                    if (shr != value)
                    {
                        return SignedSignSatQ(value, eSize, state);
                    }
                    else /* if (shr == value) */
                    {
                        return shl;
                    }
                }
                else /* if (eSize != 64) */
                {
                    return SignedSrcSignedDstSatQ(value << shiftLsB, size, state);
                }
            }
            else /* if (shiftLsB == 0) */
            {
                return value;
            }
        }

        public static ulong UnsignedShlRegSatQ(ulong value, ulong shift, bool round, int size, CpuThreadState state)
        {
            int eSize = 8 << size;

            int shiftLsB = (sbyte)shift;

            if (shiftLsB < 0)
            {
                return UnsignedShrReg(value, -shiftLsB, round, eSize);
            }
            else if (shiftLsB > 0)
            {
                if (shiftLsB >= eSize)
                {
                    return UnsignedSignSatQ(value, eSize, state);
                }

                if (eSize == 64)
                {
                    ulong shl = value << shiftLsB;
                    ulong shr = shl   >> shiftLsB;

                    if (shr != value)
                    {
                        return UnsignedSignSatQ(value, eSize, state);
                    }
                    else /* if (shr == value) */
                    {
                        return shl;
                    }
                }
                else /* if (eSize != 64) */
                {
                    return UnsignedSrcUnsignedDstSatQ(value << shiftLsB, size, state);
                }
            }
            else /* if (shiftLsB == 0) */
            {
                return value;
            }
        }

        private static long SignedShrReg(long value, int shift, bool round, int eSize) // shift := [1, 128]; eSize := {8, 16, 32, 64}.
        {
            if (round)
            {
                if (shift >= eSize)
                {
                    return 0L;
                }

                long roundConst = 1L << (shift - 1);

                long add = value + roundConst;

                if (eSize == 64)
                {
                    if ((~value & (value ^ add)) < 0L)
                    {
                        return (long)((ulong)add >> shift);
                    }
                    else
                    {
                        return add >> shift;
                    }
                }
                else /* if (eSize != 64) */
                {
                    return add >> shift;
                }
            }
            else /* if (!round) */
            {
                if (shift >= eSize)
                {
                    if (value < 0L)
                    {
                        return -1L;
                    }
                    else /* if (value >= 0L) */
                    {
                        return 0L;
                    }
                }

                return value >> shift;
            }
        }

        private static ulong UnsignedShrReg(ulong value, int shift, bool round, int eSize) // shift := [1, 128]; eSize := {8, 16, 32, 64}.
        {
            if (round)
            {
                if (shift > 64)
                {
                    return 0UL;
                }

                ulong roundConst = 1UL << (shift - 1);

                ulong add = value + roundConst;

                if (eSize == 64)
                {
                    if ((add < value) && (add < roundConst))
                    {
                        if (shift == 64)
                        {
                            return 1UL;
                        }

                        return (add >> shift) | (0x8000000000000000UL >> (shift - 1));
                    }
                    else
                    {
                        if (shift == 64)
                        {
                            return 0UL;
                        }

                        return add >> shift;
                    }
                }
                else /* if (eSize != 64) */
                {
                    if (shift == 64)
                    {
                        return 0UL;
                    }

                    return add >> shift;
                }
            }
            else /* if (!round) */
            {
                if (shift >= eSize)
                {
                    return 0UL;
                }

                return value >> shift;
            }
        }

        private static long SignedSignSatQ(long op, int eSize, CpuThreadState state) // eSize := {8, 16, 32, 64}.
        {
            long tMaxValue =  (1L << (eSize - 1)) - 1L;
            long tMinValue = -(1L << (eSize - 1));

            if (op > 0L)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMaxValue;
            }
            else if (op < 0L)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMinValue;
            }
            else
            {
                return 0L;
            }
        }

        private static ulong UnsignedSignSatQ(ulong op, int eSize, CpuThreadState state) // eSize := {8, 16, 32, 64}.
        {
            ulong tMaxValue = ulong.MaxValue >> (64 - eSize);

            if (op > 0UL)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMaxValue;
            }
            else
            {
                return 0UL;
            }
        }
#endregion

#region "ShrImm64"
        public static long SignedShrImm64(long value, long roundConst, int shift)
        {
            if (roundConst == 0L)
            {
                if (shift <= 63)
                {
                    return value >> shift;
                }
                else /* if (shift == 64) */
                {
                    if (value < 0L)
                    {
                        return -1L;
                    }
                    else /* if (value >= 0L) */
                    {
                        return 0L;
                    }
                }
            }
            else /* if (roundConst == 1L << (shift - 1)) */
            {
                if (shift <= 63)
                {
                    long add = value + roundConst;

                    if ((~value & (value ^ add)) < 0L)
                    {
                        return (long)((ulong)add >> shift);
                    }
                    else
                    {
                        return add >> shift;
                    }
                }
                else /* if (shift == 64) */
                {
                    return 0L;
                }
            }
        }

        public static ulong UnsignedShrImm64(ulong value, long roundConst, int shift)
        {
            if (roundConst == 0L)
            {
                if (shift <= 63)
                {
                    return value >> shift;
                }
                else /* if (shift == 64) */
                {
                    return 0UL;
                }
            }
            else /* if (roundConst == 1L << (shift - 1)) */
            {
                ulong add = value + (ulong)roundConst;

                if ((add < value) && (add < (ulong)roundConst))
                {
                    if (shift <= 63)
                    {
                        return (add >> shift) | (0x8000000000000000UL >> (shift - 1));
                    }
                    else /* if (shift == 64) */
                    {
                        return 1UL;
                    }
                }
                else
                {
                    if (shift <= 63)
                    {
                        return add >> shift;
                    }
                    else /* if (shift == 64) */
                    {
                        return 0UL;
                    }
                }
            }
        }
#endregion

#region "Saturating"
        public static long SignedSrcSignedDstSatQ(long op, int size, CpuThreadState state)
        {
            int eSize = 8 << size;

            long tMaxValue =  (1L << (eSize - 1)) - 1L;
            long tMinValue = -(1L << (eSize - 1));

            if (op > tMaxValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMaxValue;
            }
            else if (op < tMinValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMinValue;
            }
            else
            {
                return op;
            }
        }

        public static ulong SignedSrcUnsignedDstSatQ(long op, int size, CpuThreadState state)
        {
            int eSize = 8 << size;

            ulong tMaxValue = (1UL << eSize) - 1UL;
            ulong tMinValue =  0UL;

            if (op > (long)tMaxValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMaxValue;
            }
            else if (op < (long)tMinValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMinValue;
            }
            else
            {
                return (ulong)op;
            }
        }

        public static long UnsignedSrcSignedDstSatQ(ulong op, int size, CpuThreadState state)
        {
            int eSize = 8 << size;

            long tMaxValue = (1L << (eSize - 1)) - 1L;

            if (op > (ulong)tMaxValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMaxValue;
            }
            else
            {
                return (long)op;
            }
        }

        public static ulong UnsignedSrcUnsignedDstSatQ(ulong op, int size, CpuThreadState state)
        {
            int eSize = 8 << size;

            ulong tMaxValue = (1UL << eSize) - 1UL;

            if (op > tMaxValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return tMaxValue;
            }
            else
            {
                return op;
            }
        }

        public static long UnarySignedSatQAbsOrNeg(long op, CpuThreadState state)
        {
            if (op == long.MinValue)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return long.MaxValue;
            }
            else
            {
                return op;
            }
        }

        public static long BinarySignedSatQAdd(long op1, long op2, CpuThreadState state)
        {
            long add = op1 + op2;

            if ((~(op1 ^ op2) & (op1 ^ add)) < 0L)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                if (op1 < 0L)
                {
                    return long.MinValue;
                }
                else
                {
                    return long.MaxValue;
                }
            }
            else
            {
                return add;
            }
        }

        public static ulong BinaryUnsignedSatQAdd(ulong op1, ulong op2, CpuThreadState state)
        {
            ulong add = op1 + op2;

            if ((add < op1) && (add < op2))
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return ulong.MaxValue;
            }
            else
            {
                return add;
            }
        }

        public static long BinarySignedSatQSub(long op1, long op2, CpuThreadState state)
        {
            long sub = op1 - op2;

            if (((op1 ^ op2) & (op1 ^ sub)) < 0L)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                if (op1 < 0L)
                {
                    return long.MinValue;
                }
                else
                {
                    return long.MaxValue;
                }
            }
            else
            {
                return sub;
            }
        }

        public static ulong BinaryUnsignedSatQSub(ulong op1, ulong op2, CpuThreadState state)
        {
            ulong sub = op1 - op2;

            if (op1 < op2)
            {
                state.SetFpsrFlag(Fpsr.Qc);

                return ulong.MinValue;
            }
            else
            {
                return sub;
            }
        }

        public static long BinarySignedSatQAcc(ulong op1, long op2, CpuThreadState state)
        {
            if (op1 <= (ulong)long.MaxValue)
            {
                // op1 from ulong.MinValue to (ulong)long.MaxValue
                // op2 from long.MinValue to long.MaxValue

                long add = (long)op1 + op2;

                if ((~op2 & add) < 0L)
                {
                    state.SetFpsrFlag(Fpsr.Qc);

                    return long.MaxValue;
                }
                else
                {
                    return add;
                }
            }
            else if (op2 >= 0L)
            {
                // op1 from (ulong)long.MaxValue + 1UL to ulong.MaxValue
                // op2 from (long)ulong.MinValue to long.MaxValue

                state.SetFpsrFlag(Fpsr.Qc);

                return long.MaxValue;
            }
            else
            {
                // op1 from (ulong)long.MaxValue + 1UL to ulong.MaxValue
                // op2 from long.MinValue to (long)ulong.MinValue - 1L

                ulong add = op1 + (ulong)op2;

                if (add > (ulong)long.MaxValue)
                {
                    state.SetFpsrFlag(Fpsr.Qc);

                    return long.MaxValue;
                }
                else
                {
                    return (long)add;
                }
            }
        }

        public static ulong BinaryUnsignedSatQAcc(long op1, ulong op2, CpuThreadState state)
        {
            if (op1 >= 0L)
            {
                // op1 from (long)ulong.MinValue to long.MaxValue
                // op2 from ulong.MinValue to ulong.MaxValue

                ulong add = (ulong)op1 + op2;

                if ((add < (ulong)op1) && (add < op2))
                {
                    state.SetFpsrFlag(Fpsr.Qc);

                    return ulong.MaxValue;
                }
                else
                {
                    return add;
                }
            }
            else if (op2 > (ulong)long.MaxValue)
            {
                // op1 from long.MinValue to (long)ulong.MinValue - 1L
                // op2 from (ulong)long.MaxValue + 1UL to ulong.MaxValue

                return (ulong)op1 + op2;
            }
            else
            {
                // op1 from long.MinValue to (long)ulong.MinValue - 1L
                // op2 from ulong.MinValue to (ulong)long.MaxValue

                long add = op1 + (long)op2;

                if (add < (long)ulong.MinValue)
                {
                    state.SetFpsrFlag(Fpsr.Qc);

                    return ulong.MinValue;
                }
                else
                {
                    return (ulong)add;
                }
            }
        }
#endregion

#region "Count"
        public static ulong CountLeadingSigns(ulong value, int size) // size is 8, 16, 32 or 64 (SIMD&FP or Base Inst.).
        {
            value ^= value >> 1;

            int highBit = size - 2;

            for (int bit = highBit; bit >= 0; bit--)
            {
                if (((value >> bit) & 0b1) != 0)
                {
                    return (ulong)(highBit - bit);
                }
            }

            return (ulong)(size - 1);
        }

        private static readonly byte[] ClzNibbleTbl = { 4, 3, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static ulong CountLeadingZeros(ulong value, int size) // size is 8, 16, 32 or 64 (SIMD&FP or Base Inst.).
        {
            if (value == 0ul)
            {
                return (ulong)size;
            }

            int nibbleIdx = size;
            int preCount, count = 0;

            do
            {
                nibbleIdx -= 4;
                preCount = ClzNibbleTbl[(value >> nibbleIdx) & 0b1111];
                count += preCount;
            }
            while (preCount == 4);

            return (ulong)count;
        }

        public static ulong CountSetBits8(ulong value) // "size" is 8 (SIMD&FP Inst.).
        {
            if (value == 0xfful)
            {
                return 8ul;
            }

            value = ((value >> 1) & 0x55ul) + (value & 0x55ul);
            value = ((value >> 2) & 0x33ul) + (value & 0x33ul);

            return (value >> 4) + (value & 0x0ful);
        }
#endregion

#region "Crc32"
        private const uint Crc32RevPoly  = 0xedb88320;
        private const uint Crc32CRevPoly = 0x82f63b78;

        public static uint Crc32B(uint crc, byte   val) => Crc32 (crc, Crc32RevPoly, val);
        public static uint Crc32H(uint crc, ushort val) => Crc32H(crc, Crc32RevPoly, val);
        public static uint Crc32W(uint crc, uint   val) => Crc32W(crc, Crc32RevPoly, val);
        public static uint Crc32X(uint crc, ulong  val) => Crc32X(crc, Crc32RevPoly, val);

        public static uint Crc32Cb(uint crc, byte   val) => Crc32 (crc, Crc32CRevPoly, val);
        public static uint Crc32Ch(uint crc, ushort val) => Crc32H(crc, Crc32CRevPoly, val);
        public static uint Crc32Cw(uint crc, uint   val) => Crc32W(crc, Crc32CRevPoly, val);
        public static uint Crc32Cx(uint crc, ulong  val) => Crc32X(crc, Crc32CRevPoly, val);

        private static uint Crc32H(uint crc, uint poly, ushort val)
        {
            crc = Crc32(crc, poly, (byte)(val >> 0));
            crc = Crc32(crc, poly, (byte)(val >> 8));

            return crc;
        }

        private static uint Crc32W(uint crc, uint poly, uint val)
        {
            crc = Crc32(crc, poly, (byte)(val >> 0 ));
            crc = Crc32(crc, poly, (byte)(val >> 8 ));
            crc = Crc32(crc, poly, (byte)(val >> 16));
            crc = Crc32(crc, poly, (byte)(val >> 24));

            return crc;
        }

        private static uint Crc32X(uint crc, uint poly, ulong val)
        {
            crc = Crc32(crc, poly, (byte)(val >> 0 ));
            crc = Crc32(crc, poly, (byte)(val >> 8 ));
            crc = Crc32(crc, poly, (byte)(val >> 16));
            crc = Crc32(crc, poly, (byte)(val >> 24));
            crc = Crc32(crc, poly, (byte)(val >> 32));
            crc = Crc32(crc, poly, (byte)(val >> 40));
            crc = Crc32(crc, poly, (byte)(val >> 48));
            crc = Crc32(crc, poly, (byte)(val >> 56));

            return crc;
        }

        private static uint Crc32(uint crc, uint poly, byte val)
        {
            crc ^= val;

            for (int bit = 7; bit >= 0; bit--)
            {
                uint mask = (uint)(-(int)(crc & 1));

                crc = (crc >> 1) ^ (poly & mask);
            }

            return crc;
        }
#endregion

#region "Aes"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Decrypt(Vector128<float> value, Vector128<float> roundKey)
        {
            if (!Sse.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return CryptoHelper.AesInvSubBytes(CryptoHelper.AesInvShiftRows(Sse.Xor(value, roundKey)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Encrypt(Vector128<float> value, Vector128<float> roundKey)
        {
            if (!Sse.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            return CryptoHelper.AesSubBytes(CryptoHelper.AesShiftRows(Sse.Xor(value, roundKey)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> InverseMixColumns(Vector128<float> value)
        {
            return CryptoHelper.AesInvMixColumns(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> MixColumns(Vector128<float> value)
        {
            return CryptoHelper.AesMixColumns(value);
        }
#endregion

#region "Sha1"
        public static Vector128<float> HashChoose(Vector128<float> hash_abcd, uint hash_e, Vector128<float> wk)
        {
            for (int e = 0; e <= 3; e++)
            {
                uint t = ShaChoose((uint)VectorExtractIntZx(hash_abcd, (byte)1, 2),
                                   (uint)VectorExtractIntZx(hash_abcd, (byte)2, 2),
                                   (uint)VectorExtractIntZx(hash_abcd, (byte)3, 2));

                hash_e += Rol((uint)VectorExtractIntZx(hash_abcd, (byte)0, 2), 5) + t;
                hash_e += (uint)VectorExtractIntZx(wk, (byte)e, 2);

                t = Rol((uint)VectorExtractIntZx(hash_abcd, (byte)1, 2), 30);
                hash_abcd = VectorInsertInt((ulong)t, hash_abcd, (byte)1, 2);

                Rol32_160(ref hash_e, ref hash_abcd);
            }

            return hash_abcd;
        }

        public static uint FixedRotate(uint hash_e)
        {
            return hash_e.Rol(30);
        }

        public static Vector128<float> HashMajority(Vector128<float> hash_abcd, uint hash_e, Vector128<float> wk)
        {
            for (int e = 0; e <= 3; e++)
            {
                uint t = ShaMajority((uint)VectorExtractIntZx(hash_abcd, (byte)1, 2),
                                     (uint)VectorExtractIntZx(hash_abcd, (byte)2, 2),
                                     (uint)VectorExtractIntZx(hash_abcd, (byte)3, 2));

                hash_e += Rol((uint)VectorExtractIntZx(hash_abcd, (byte)0, 2), 5) + t;
                hash_e += (uint)VectorExtractIntZx(wk, (byte)e, 2);

                t = Rol((uint)VectorExtractIntZx(hash_abcd, (byte)1, 2), 30);
                hash_abcd = VectorInsertInt((ulong)t, hash_abcd, (byte)1, 2);

                Rol32_160(ref hash_e, ref hash_abcd);
            }

            return hash_abcd;
        }

        public static Vector128<float> HashParity(Vector128<float> hash_abcd, uint hash_e, Vector128<float> wk)
        {
            for (int e = 0; e <= 3; e++)
            {
                uint t = ShaParity((uint)VectorExtractIntZx(hash_abcd, (byte)1, 2),
                                   (uint)VectorExtractIntZx(hash_abcd, (byte)2, 2),
                                   (uint)VectorExtractIntZx(hash_abcd, (byte)3, 2));

                hash_e += Rol((uint)VectorExtractIntZx(hash_abcd, (byte)0, 2), 5) + t;
                hash_e += (uint)VectorExtractIntZx(wk, (byte)e, 2);

                t = Rol((uint)VectorExtractIntZx(hash_abcd, (byte)1, 2), 30);
                hash_abcd = VectorInsertInt((ulong)t, hash_abcd, (byte)1, 2);

                Rol32_160(ref hash_e, ref hash_abcd);
            }

            return hash_abcd;
        }

        public static Vector128<float> Sha1SchedulePart1(Vector128<float> w0_3, Vector128<float> w4_7, Vector128<float> w8_11)
        {
            if (!Sse.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            Vector128<float> result = new Vector128<float>();

            ulong t2 = VectorExtractIntZx(w4_7, (byte)0, 3);
            ulong t1 = VectorExtractIntZx(w0_3, (byte)1, 3);

            result = VectorInsertInt((ulong)t1, result, (byte)0, 3);
            result = VectorInsertInt((ulong)t2, result, (byte)1, 3);

            return Sse.Xor(result, Sse.Xor(w0_3, w8_11));
        }

        public static Vector128<float> Sha1SchedulePart2(Vector128<float> tw0_3, Vector128<float> w12_15)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            Vector128<float> result = new Vector128<float>();

            Vector128<float> t = Sse.Xor(tw0_3, Sse.StaticCast<uint, float>(
                Sse2.ShiftRightLogical128BitLane(Sse.StaticCast<float, uint>(w12_15), (byte)4)));

            uint tE0 = (uint)VectorExtractIntZx(t, (byte)0, 2);
            uint tE1 = (uint)VectorExtractIntZx(t, (byte)1, 2);
            uint tE2 = (uint)VectorExtractIntZx(t, (byte)2, 2);
            uint tE3 = (uint)VectorExtractIntZx(t, (byte)3, 2);

            result = VectorInsertInt((ulong)tE0.Rol(1), result, (byte)0, 2);
            result = VectorInsertInt((ulong)tE1.Rol(1), result, (byte)1, 2);
            result = VectorInsertInt((ulong)tE2.Rol(1), result, (byte)2, 2);

            return VectorInsertInt((ulong)(tE3.Rol(1) ^ tE0.Rol(2)), result, (byte)3, 2);
        }

        private static void Rol32_160(ref uint y, ref Vector128<float> x)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            uint xE3 = (uint)VectorExtractIntZx(x, (byte)3, 2);

            x = Sse.StaticCast<uint, float>(Sse2.ShiftLeftLogical128BitLane(Sse.StaticCast<float, uint>(x), (byte)4));
            x = VectorInsertInt((ulong)y, x, (byte)0, 2);

            y = xE3;
        }

        private static uint ShaChoose(uint x, uint y, uint z)
        {
            return ((y ^ z) & x) ^ z;
        }

        private static uint ShaMajority(uint x, uint y, uint z)
        {
            return (x & y) | ((x | y) & z);
        }

        private static uint ShaParity(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }

        private static uint Rol(this uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
#endregion

#region "Sha256"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> HashLower(Vector128<float> hash_abcd, Vector128<float> hash_efgh, Vector128<float> wk)
        {
            return Sha256Hash(hash_abcd, hash_efgh, wk, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> HashUpper(Vector128<float> hash_efgh, Vector128<float> hash_abcd, Vector128<float> wk)
        {
            return Sha256Hash(hash_abcd, hash_efgh, wk, false);
        }

        public static Vector128<float> Sha256SchedulePart1(Vector128<float> w0_3, Vector128<float> w4_7)
        {
            Vector128<float> result = new Vector128<float>();

            for (int e = 0; e <= 3; e++)
            {
                uint elt = (uint)VectorExtractIntZx(e <= 2 ? w0_3 : w4_7, (byte)(e <= 2 ? e + 1 : 0), 2);

                elt = elt.Ror(7) ^ elt.Ror(18) ^ elt.Lsr(3);

                elt += (uint)VectorExtractIntZx(w0_3, (byte)e, 2);

                result = VectorInsertInt((ulong)elt, result, (byte)e, 2);
            }

            return result;
        }

        public static Vector128<float> Sha256SchedulePart2(Vector128<float> w0_3, Vector128<float> w8_11, Vector128<float> w12_15)
        {
            Vector128<float> result = new Vector128<float>();

            ulong t1 = VectorExtractIntZx(w12_15, (byte)1, 3);

            for (int e = 0; e <= 1; e++)
            {
                uint elt = t1.ULongPart(e);

                elt = elt.Ror(17) ^ elt.Ror(19) ^ elt.Lsr(10);

                elt += (uint)VectorExtractIntZx(w0_3, (byte)e, 2);
                elt += (uint)VectorExtractIntZx(w8_11, (byte)(e + 1), 2);

                result = VectorInsertInt((ulong)elt, result, (byte)e, 2);
            }

            t1 = VectorExtractIntZx(result, (byte)0, 3);

            for (int e = 2; e <= 3; e++)
            {
                uint elt = t1.ULongPart(e - 2);

                elt = elt.Ror(17) ^ elt.Ror(19) ^ elt.Lsr(10);

                elt += (uint)VectorExtractIntZx(w0_3, (byte)e, 2);
                elt += (uint)VectorExtractIntZx(e == 2 ? w8_11 : w12_15, (byte)(e == 2 ? 3 : 0), 2);

                result = VectorInsertInt((ulong)elt, result, (byte)e, 2);
            }

            return result;
        }

        private static Vector128<float> Sha256Hash(Vector128<float> x, Vector128<float> y, Vector128<float> w, bool part1)
        {
            for (int e = 0; e <= 3; e++)
            {
                uint chs = ShaChoose((uint)VectorExtractIntZx(y, (byte)0, 2),
                                     (uint)VectorExtractIntZx(y, (byte)1, 2),
                                     (uint)VectorExtractIntZx(y, (byte)2, 2));

                uint maj = ShaMajority((uint)VectorExtractIntZx(x, (byte)0, 2),
                                       (uint)VectorExtractIntZx(x, (byte)1, 2),
                                       (uint)VectorExtractIntZx(x, (byte)2, 2));

                uint t1 = (uint)VectorExtractIntZx(y, (byte)3, 2);
                t1 += ShaHashSigma1((uint)VectorExtractIntZx(y, (byte)0, 2)) + chs;
                t1 += (uint)VectorExtractIntZx(w, (byte)e, 2);

                uint t2 = t1 + (uint)VectorExtractIntZx(x, (byte)3, 2);
                x = VectorInsertInt((ulong)t2, x, (byte)3, 2);
                t2 = t1 + ShaHashSigma0((uint)VectorExtractIntZx(x, (byte)0, 2)) + maj;
                y = VectorInsertInt((ulong)t2, y, (byte)3, 2);

                Rol32_256(ref y, ref x);
            }

            return part1 ? x : y;
        }

        private static void Rol32_256(ref Vector128<float> y, ref Vector128<float> x)
        {
            if (!Sse2.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            uint yE3 = (uint)VectorExtractIntZx(y, (byte)3, 2);
            uint xE3 = (uint)VectorExtractIntZx(x, (byte)3, 2);

            y = Sse.StaticCast<uint, float>(Sse2.ShiftLeftLogical128BitLane(Sse.StaticCast<float, uint>(y), (byte)4));
            x = Sse.StaticCast<uint, float>(Sse2.ShiftLeftLogical128BitLane(Sse.StaticCast<float, uint>(x), (byte)4));

            y = VectorInsertInt((ulong)xE3, y, (byte)0, 2);
            x = VectorInsertInt((ulong)yE3, x, (byte)0, 2);
        }

        private static uint ShaHashSigma0(uint x)
        {
            return x.Ror(2) ^ x.Ror(13) ^ x.Ror(22);
        }

        private static uint ShaHashSigma1(uint x)
        {
            return x.Ror(6) ^ x.Ror(11) ^ x.Ror(25);
        }

        private static uint Ror(this uint value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }

        private static uint Lsr(this uint value, int count)
        {
            return value >> count;
        }

        private static uint ULongPart(this ulong value, int part)
        {
            return part == 0
                ? (uint)(value & 0xFFFFFFFFUL)
                : (uint)(value >> 32);
        }
#endregion

#region "Reverse"
        public static uint ReverseBits8(uint value)
        {
            value = ((value & 0xaa) >> 1) | ((value & 0x55) << 1);
            value = ((value & 0xcc) >> 2) | ((value & 0x33) << 2);

            return (value >> 4) | ((value & 0x0f) << 4);
        }

        public static uint ReverseBits32(uint value)
        {
            value = ((value & 0xaaaaaaaa) >> 1) | ((value & 0x55555555) << 1);
            value = ((value & 0xcccccccc) >> 2) | ((value & 0x33333333) << 2);
            value = ((value & 0xf0f0f0f0) >> 4) | ((value & 0x0f0f0f0f) << 4);
            value = ((value & 0xff00ff00) >> 8) | ((value & 0x00ff00ff) << 8);

            return (value >> 16) | (value << 16);
        }

        public static ulong ReverseBits64(ulong value)
        {
            value = ((value & 0xaaaaaaaaaaaaaaaa) >> 1 ) | ((value & 0x5555555555555555) << 1 );
            value = ((value & 0xcccccccccccccccc) >> 2 ) | ((value & 0x3333333333333333) << 2 );
            value = ((value & 0xf0f0f0f0f0f0f0f0) >> 4 ) | ((value & 0x0f0f0f0f0f0f0f0f) << 4 );
            value = ((value & 0xff00ff00ff00ff00) >> 8 ) | ((value & 0x00ff00ff00ff00ff) << 8 );
            value = ((value & 0xffff0000ffff0000) >> 16) | ((value & 0x0000ffff0000ffff) << 16);

            return (value >> 32) | (value << 32);
        }

        public static uint ReverseBytes16_32(uint value) => (uint)ReverseBytes16_64(value);
        public static uint ReverseBytes32_32(uint value) => (uint)ReverseBytes32_64(value);

        public static ulong ReverseBytes16_64(ulong value) => ReverseBytes(value, RevSize.Rev16);
        public static ulong ReverseBytes32_64(ulong value) => ReverseBytes(value, RevSize.Rev32);
        public static ulong ReverseBytes64(ulong value)    => ReverseBytes(value, RevSize.Rev64);

        private enum RevSize
        {
            Rev16,
            Rev32,
            Rev64
        }

        private static ulong ReverseBytes(ulong value, RevSize size)
        {
            value = ((value & 0xff00ff00ff00ff00) >> 8) | ((value & 0x00ff00ff00ff00ff) << 8);

            if (size == RevSize.Rev16)
            {
                return value;
            }

            value = ((value & 0xffff0000ffff0000) >> 16) | ((value & 0x0000ffff0000ffff) << 16);

            if (size == RevSize.Rev32)
            {
                return value;
            }

            value = ((value & 0xffffffff00000000) >> 32) | ((value & 0x00000000ffffffff) << 32);

            if (size == RevSize.Rev64)
            {
                return value;
            }

            throw new ArgumentException(nameof(size));
        }
#endregion

#region "MultiplyHigh"
        public static long SMulHi128(long left, long right)
        {
            long result = (long)UMulHi128((ulong)left, (ulong)right);

            if (left < 0)
            {
                result -= right;
            }

            if (right < 0)
            {
                result -= left;
            }

            return result;
        }

        public static ulong UMulHi128(ulong left, ulong right)
        {
            ulong lHigh = left  >> 32;
            ulong lLow  = left  &  0xFFFFFFFF;
            ulong rHigh = right >> 32;
            ulong rLow  = right &  0xFFFFFFFF;

            ulong z2 = lLow  * rLow;
            ulong t  = lHigh * rLow + (z2 >> 32);
            ulong z1 = t &  0xFFFFFFFF;
            ulong z0 = t >> 32;

            z1 += lLow * rHigh;

            return lHigh * rHigh + z0 + (z1 >> 32);
        }
#endregion
    }
}
