// https://github.com/LDj3SNuD/ARM_v8-A_AArch64_Instructions_Tester/blob/master/Tester/Pseudocode.cs

// https://meriac.github.io/archex/A64_v83A_ISA/shared_pseudocode.xml
// https://alastairreid.github.io/asl-lexical-syntax/

// | ------------------------|----------------------------------- |
// | ASL                     | C#                                 |
// | ------------------------|----------------------------------- |
// | bit, bits(1); boolean   | bool                               |
// | bits                    | Bits                               |
// | integer                 | BigInteger, int                    |
// | real                    | decimal                            |
// | ------------------------|----------------------------------- |
// | '0'; FALSE              | false                              |
// | '1'; TRUE               | true                               |
// | '010'                   | "010"                              |
// | bitsX IN {bitsY, bitsZ} | (bitsX == bitsY || bitsX == bitsZ) |
// | DIV                     | /                                  |
// | MOD                     | %                                  |
// | ------------------------|----------------------------------- |

using System;
using System.Numerics;

namespace Ryujinx.Tests.Cpu.Tester
{
    using Types;

    using static Shared;

    internal static class AArch64
    {
#region "exceptions/exceptions/"
        /* #AArch64.ResetControlRegisters.1 */
        public static void ResetControlRegisters(bool cold_reset)
        {
            PSTATE.N = cold_reset;
            PSTATE.Z = cold_reset;
            PSTATE.C = cold_reset;
            PSTATE.V = cold_reset;
        }

        /* */
        public static void TakeReset(bool cold_reset)
        {
            /* assert !HighestELUsingAArch32(); */

            // Enter the highest implemented Exception level in AArch64 state
            if (HaveEL(EL3))
            {
                PSTATE.EL = EL3;
            }
            else if (HaveEL(EL2))
            {
                PSTATE.EL = EL2;
            }
            else
            {
                PSTATE.EL = EL1;
            }

            // Reset the system registers and other system components
            AArch64.ResetControlRegisters(cold_reset);

            // Reset all other PSTATE fields
            PSTATE.SP = true; // Select stack pointer

            // All registers, bits and fields not reset by the above pseudocode or by the BranchTo() call
            // below are UNKNOWN bitstrings after reset. In particular, the return information registers
            // ELR_ELx and SPSR_ELx have UNKNOWN values, so that it
            // is impossible to return from a reset in an architecturally defined way.
            AArch64.ResetGeneralRegisters();
            AArch64.ResetSpecialRegisters();
        }
#endregion

#region "functions/registers/"
        /* #AArch64.ResetGeneralRegisters.0 */
        public static void ResetGeneralRegisters()
        {
            for (int i = 0; i <= 30; i++)
            {
                /* X[i] = bits(64) UNKNOWN; */
                _R[i].SetAll(false);
            }
        }

        /* #AArch64.ResetSpecialRegisters.0 */
        public static void ResetSpecialRegisters()
        {
            // AArch64 special registers
            /* SP_EL0 = bits(64) UNKNOWN; */
            SP_EL0.SetAll(false);
            /* SP_EL1 = bits(64) UNKNOWN; */
            SP_EL1.SetAll(false);
        }

        // #impl-aarch64.SP.write.0
        public static void SP(Bits value)
        {
            /* int width = value.Count; */

            /* assert width IN {32,64}; */

            if (!PSTATE.SP)
            {
                SP_EL0 = ZeroExtend(64, value);
            }
            else
            {
                switch (PSTATE.EL)
                {
                    case Bits bits when bits == EL0:
                        SP_EL0 = ZeroExtend(64, value);
                        break;
                    default:
                    case Bits bits when bits == EL1:
                        SP_EL1 = ZeroExtend(64, value);
                        break;
                    /*case Bits bits when bits == EL2:
                        SP_EL2 = ZeroExtend(64, value);
                        break;
                    case Bits bits when bits == EL3:
                        SP_EL3 = ZeroExtend(64, value);
                        break;*/
                }
            }
        }

        // #impl-aarch64.SP.read.0
        public static Bits SP(int width)
        {
            /* assert width IN {8,16,32,64}; */

            if (!PSTATE.SP)
            {
                return SP_EL0[width - 1, 0];
            }
            else
            {
                switch (PSTATE.EL)
                {
                    case Bits bits when bits == EL0:
                        return SP_EL0[width - 1, 0];
                    default:
                    case Bits bits when bits == EL1:
                        return SP_EL1[width - 1, 0];
                    /*case Bits bits when bits == EL2:
                        return SP_EL2[width - 1, 0];
                    case Bits bits when bits == EL3:
                        return SP_EL3[width - 1, 0];*/
                }
            }
        }

        // #impl-aarch64.X.write.1
        public static void X(int n, Bits value)
        {
            /* int width = value.Count; */

            /* assert n >= 0 && n <= 31; */
            /* assert width IN {32,64}; */

            if (n != 31)
            {
                _R[n] = ZeroExtend(64, value);
            }
        }

        /* #impl-aarch64.X.read.1 */
        public static Bits X(int width, int n)
        {
            /* assert n >= 0 && n <= 31; */
            /* assert width IN {8,16,32,64}; */

            if (n != 31)
            {
                return _R[n][width - 1, 0];
            }
            else
            {
                return Zeros(width);
            }
        }
#endregion

#region "instrs/extendreg/"
        /* #impl-aarch64.DecodeRegExtend.1 */
        public static ExtendType DecodeRegExtend(Bits op)
        {
            switch (op)
            {
                default:
                case Bits bits when bits == "000":
                    return ExtendType.ExtendType_UXTB;
                case Bits bits when bits == "001":
                    return ExtendType.ExtendType_UXTH;
                case Bits bits when bits == "010":
                    return ExtendType.ExtendType_UXTW;
                case Bits bits when bits == "011":
                    return ExtendType.ExtendType_UXTX;
                case Bits bits when bits == "100":
                    return ExtendType.ExtendType_SXTB;
                case Bits bits when bits == "101":
                    return ExtendType.ExtendType_SXTH;
                case Bits bits when bits == "110":
                    return ExtendType.ExtendType_SXTW;
                case Bits bits when bits == "111":
                    return ExtendType.ExtendType_SXTX;
            }
        }

        /* #impl-aarch64.ExtendReg.3 */
        public static Bits ExtendReg(int N, int reg, ExtendType type, int shift)
        {
            /* assert shift >= 0 && shift <= 4; */
            Bits val = X(N, reg);
            bool unsigned;
            int len;

            switch (type)
            {
                default:
                case ExtendType.ExtendType_SXTB:
                    unsigned = false; len = 8;
                    break;
                case ExtendType.ExtendType_SXTH:
                    unsigned = false; len = 16;
                    break;
                case ExtendType.ExtendType_SXTW:
                    unsigned = false; len = 32;
                    break;
                case ExtendType.ExtendType_SXTX:
                    unsigned = false; len = 64;
                    break;
                case ExtendType.ExtendType_UXTB:
                    unsigned = true;  len = 8;
                    break;
                case ExtendType.ExtendType_UXTH:
                    unsigned = true;  len = 16;
                    break;
                case ExtendType.ExtendType_UXTW:
                    unsigned = true;  len = 32;
                    break;
                case ExtendType.ExtendType_UXTX:
                    unsigned = true;  len = 64;
                    break;
            }

            // Note the extended width of the intermediate value and
            // that sign extension occurs from bit <len+shift-1>, not
            // from bit <len-1>. This is equivalent to the instruction
            //   [SU]BFIZ Rtmp, Rreg, #shift, #len
            // It may also be seen as a sign/zero extend followed by a shift:
            //   LSL(Extend(val<len-1:0>, N, unsigned), shift);

            len = Min(len, N - shift);
            return Extend(Bits.Concat(val[len - 1, 0], Zeros(shift)), N, unsigned);
        }

        // #ExtendType
        public enum ExtendType {ExtendType_SXTB, ExtendType_SXTH, ExtendType_SXTW, ExtendType_SXTX,
                                ExtendType_UXTB, ExtendType_UXTH, ExtendType_UXTW, ExtendType_UXTX};
#endregion

#region "instrs/integer/bitmasks/"
        /* #impl-aarch64.DecodeBitMasks.4 */
        public static (Bits, Bits) DecodeBitMasks(int M, bool immN, Bits imms, Bits immr, bool immediate)
        {
            Bits tmask, wmask;
            Bits tmask_and, wmask_and;
            Bits tmask_or, wmask_or;
            Bits levels;

            // Compute log2 of element size
            // 2^len must be in range [2, M]
            int len = HighestSetBit(Bits.Concat(immN, NOT(imms)));
            /* if len < 1 then ReservedValue(); */
            /* assert M >= (1 << len); */

            // Determine S, R and S - R parameters
            levels = ZeroExtend(Ones(len), 6);

            // For logical immediates an all-ones value of S is reserved
            // since it would generate a useless all-ones result (many times)
            /* if immediate && (imms AND levels) == levels then ReservedValue(); */

            BigInteger S = UInt(AND(imms, levels));
            BigInteger R = UInt(AND(immr, levels));
            BigInteger diff = S - R; // 6-bit subtract with borrow

            // Compute "top mask"
            tmask_and = OR(diff.SubBigInteger(5, 0), NOT(levels));
            tmask_or = AND(diff.SubBigInteger(5, 0), levels);

            tmask = Ones(64);
            tmask = OR(AND(tmask, Replicate(Bits.Concat(Replicate(tmask_and[0],  1), Ones( 1)), 32)), Replicate(Bits.Concat(Zeros( 1), Replicate(tmask_or[0],  1)), 32));
            tmask = OR(AND(tmask, Replicate(Bits.Concat(Replicate(tmask_and[1],  2), Ones( 2)), 16)), Replicate(Bits.Concat(Zeros( 2), Replicate(tmask_or[1],  2)), 16));
            tmask = OR(AND(tmask, Replicate(Bits.Concat(Replicate(tmask_and[2],  4), Ones( 4)),  8)), Replicate(Bits.Concat(Zeros( 4), Replicate(tmask_or[2],  4)),  8));
            tmask = OR(AND(tmask, Replicate(Bits.Concat(Replicate(tmask_and[3],  8), Ones( 8)),  4)), Replicate(Bits.Concat(Zeros( 8), Replicate(tmask_or[3],  8)),  4));
            tmask = OR(AND(tmask, Replicate(Bits.Concat(Replicate(tmask_and[4], 16), Ones(16)),  2)), Replicate(Bits.Concat(Zeros(16), Replicate(tmask_or[4], 16)),  2));
            tmask = OR(AND(tmask, Replicate(Bits.Concat(Replicate(tmask_and[5], 32), Ones(32)),  1)), Replicate(Bits.Concat(Zeros(32), Replicate(tmask_or[5], 32)),  1));

            // Compute "wraparound mask"
            wmask_and = OR(immr, NOT(levels));
            wmask_or = AND(immr, levels);

            wmask = Zeros(64);
            wmask = OR(AND(wmask, Replicate(Bits.Concat(Ones( 1), Replicate(wmask_and[0],  1)), 32)), Replicate(Bits.Concat(Replicate(wmask_or[0],  1), Zeros( 1)), 32));
            wmask = OR(AND(wmask, Replicate(Bits.Concat(Ones( 2), Replicate(wmask_and[1],  2)), 16)), Replicate(Bits.Concat(Replicate(wmask_or[1],  2), Zeros( 2)), 16));
            wmask = OR(AND(wmask, Replicate(Bits.Concat(Ones( 4), Replicate(wmask_and[2],  4)),  8)), Replicate(Bits.Concat(Replicate(wmask_or[2],  4), Zeros( 4)),  8));
            wmask = OR(AND(wmask, Replicate(Bits.Concat(Ones( 8), Replicate(wmask_and[3],  8)),  4)), Replicate(Bits.Concat(Replicate(wmask_or[3],  8), Zeros( 8)),  4));
            wmask = OR(AND(wmask, Replicate(Bits.Concat(Ones(16), Replicate(wmask_and[4], 16)),  2)), Replicate(Bits.Concat(Replicate(wmask_or[4], 16), Zeros(16)),  2));
            wmask = OR(AND(wmask, Replicate(Bits.Concat(Ones(32), Replicate(wmask_and[5], 32)),  1)), Replicate(Bits.Concat(Replicate(wmask_or[5], 32), Zeros(32)),  1));

            if (diff.SubBigInteger(6)) // borrow from S - R
            {
                wmask = AND(wmask, tmask);
            }
            else
            {
                wmask = OR(wmask, tmask);
            }

            return (wmask[M - 1, 0], tmask[M - 1, 0]);
        }
#endregion

#region "instrs/integer/shiftreg/"
        /* #impl-aarch64.DecodeShift.1 */
        public static ShiftType DecodeShift(Bits op)
        {
            switch (op)
            {
                default:
                case Bits bits when bits == "00":
                    return ShiftType.ShiftType_LSL;
                case Bits bits when bits == "01":
                    return ShiftType.ShiftType_LSR;
                case Bits bits when bits == "10":
                    return ShiftType.ShiftType_ASR;
                case Bits bits when bits == "11":
                    return ShiftType.ShiftType_ROR;
            }
        }

        /* #impl-aarch64.ShiftReg.3 */
        public static Bits ShiftReg(int N, int reg, ShiftType type, int amount)
        {
            Bits result = X(N, reg);

            switch (type)
            {
                default:
                case ShiftType.ShiftType_LSL:
                    result = LSL(result, amount);
                    break;
                case ShiftType.ShiftType_LSR:
                    result = LSR(result, amount);
                    break;
                case ShiftType.ShiftType_ASR:
                    result = ASR(result, amount);
                    break;
                case ShiftType.ShiftType_ROR:
                    result = ROR(result, amount);
                    break;
            }

            return result;
        }

        // #ShiftType
        public enum ShiftType {ShiftType_LSL, ShiftType_LSR, ShiftType_ASR, ShiftType_ROR};
#endregion
    }

    internal static class Shared
    {
        static Shared()
        {
            _R = new Bits[31];
            for (int i = 0; i <= 30; i++)
            {
                _R[i] = new Bits(64, false);
            }

            SP_EL0 = new Bits(64, false);
            SP_EL1 = new Bits(64, false);

            PSTATE.N = false;
            PSTATE.Z = false;
            PSTATE.C = false;
            PSTATE.V = false;
            PSTATE.EL = EL1;
            PSTATE.SP = true;
        }

#region "functions/common/"
        /* */
        public static Bits AND(Bits x, Bits y)
        {
            return x.And(y);
        }

        // #impl-shared.ASR.2
        public static Bits ASR(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x);
            }
            else
            {
                (result, _) = ASR_C(x, shift);
            }

            return result;
        }

        // #impl-shared.ASR_C.2
        public static (Bits, bool) ASR_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift > 0; */

            Bits extended_x = SignExtend(x, shift + N);
            Bits result = extended_x[shift + N - 1, shift];
            bool carry_out = extended_x[shift - 1];

            return (result, carry_out);
        }

        // #impl-shared.CountLeadingSignBits.1
        public static int CountLeadingSignBits(Bits x)
        {
            int N = x.Count;

            return CountLeadingZeroBits(EOR(x[N - 1, 1], x[N - 2, 0]));
        }

        // #impl-shared.CountLeadingZeroBits.1
        public static int CountLeadingZeroBits(Bits x)
        {
            int N = x.Count;

            return (N - 1 - HighestSetBit(x));
        }

        /* */
        public static Bits EOR(Bits x, Bits y)
        {
            return x.Xor(y);
        }

        // #impl-shared.Extend.3
        public static Bits Extend(Bits x, int N, bool unsigned)
        {
            if (unsigned)
            {
                return ZeroExtend(x, N);
            }
            else
            {
                return SignExtend(x, N);
            }
        }

        /* #impl-shared.Extend.2 */
        public static Bits Extend(int N, Bits x, bool unsigned)
        {
            return Extend(x, N, unsigned);
        }

        // #impl-shared.HighestSetBit.1
        public static int HighestSetBit(Bits x)
        {
            int N = x.Count;

            for (int i = N - 1; i >= 0; i--)
            {
                if (x[i])
                {
                    return i;
                }
            }

            return -1;
        }

        // #impl-shared.Int.2
        public static BigInteger Int(Bits x, bool unsigned)
        {
            return (unsigned ? UInt(x) : SInt(x));
        }

        // #impl-shared.IsOnes.1
        public static bool IsOnes(Bits x)
        {
            int N = x.Count;

            return (x == Ones(N));
        }

        // #impl-shared.IsZero.1
        public static bool IsZero(Bits x)
        {
            int N = x.Count;

            return (x == Zeros(N));
        }

        // #impl-shared.IsZeroBit.1
        public static bool IsZeroBit(Bits x)
        {
            return IsZero(x);
        }

        // #impl-shared.LSL.2
        public static Bits LSL(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x);
            }
            else
            {
                (result, _) = LSL_C(x, shift);
            }

            return result;
        }

        // #impl-shared.LSL_C.2
        public static (Bits, bool) LSL_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift > 0; */

            Bits extended_x = Bits.Concat(x, Zeros(shift));
            Bits result = extended_x[N - 1, 0];
            bool carry_out = extended_x[N];

            return (result, carry_out);
        }

        // #impl-shared.LSR.2
        public static Bits LSR(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x);
            }
            else
            {
                (result, _) = LSR_C(x, shift);
            }

            return result;
        }

        // #impl-shared.LSR_C.2
        public static (Bits, bool) LSR_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift > 0; */

            Bits extended_x = ZeroExtend(x, shift + N);
            Bits result = extended_x[shift + N - 1, shift];
            bool carry_out = extended_x[shift - 1];

            return (result, carry_out);
        }

        // #impl-shared.Min.2
        public static int Min(int a, int b)
        {
            if (a <= b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        /* #impl-shared.NOT.1 */
        public static Bits NOT(Bits x)
        {
            return x.Not();
        }

        // #impl-shared.Ones.1
        public static Bits Ones(int N)
        {
            return Replicate(true, N);
        }

        /* */
        public static Bits OR(Bits x, Bits y)
        {
            return x.Or(y);
        }

        /* */
        public static decimal Real(BigInteger value)
        {
            return (decimal)value;
        }

        // #impl-shared.ROR.2
        public static Bits ROR(Bits x, int shift)
        {
            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x);
            }
            else
            {
                (result, _) = ROR_C(x, shift);
            }

            return result;
        }

        // #impl-shared.ROR_C.2
        public static (Bits, bool) ROR_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift != 0; */

            int m = shift % N;
            Bits result = OR(LSR(x, m), LSL(x, N - m));
            bool carry_out = result[N - 1];

            return (result, carry_out);
        }

        /* #impl-shared.Replicate.1 */
        public static Bits Replicate(int N, Bits x)
        {
            int M = x.Count;

            /* assert N MOD M == 0; */

            return Replicate(x, N / M);
        }

        /* #impl-shared.Replicate.2 */
        public static Bits Replicate(Bits x, int N)
        {
            int M = x.Count;

            bool[] dst = new bool[M * N];

            for (int i = 0; i < N; i++)
            {
                x.CopyTo(dst, i * M);
            }

            return new Bits(dst);
        }

        /* #impl-shared.RoundDown.1 */
        public static BigInteger RoundDown(decimal x)
        {
            return (BigInteger)Decimal.Floor(x);
        }

        // #impl-shared.RoundTowardsZero.1
        public static BigInteger RoundTowardsZero(decimal x)
        {
            if (x == 0.0m)
            {
                return (BigInteger)0m;
            }
            else if (x >= 0.0m)
            {
                return RoundDown(x);
            }
            else
            {
                return RoundUp(x);
            }
        }

        /* #impl-shared.RoundUp.1 */
        public static BigInteger RoundUp(decimal x)
        {
            return (BigInteger)Decimal.Ceiling(x);
        }

        // #impl-shared.SInt.1
        public static BigInteger SInt(Bits x)
        {
            int N = x.Count;

            BigInteger result = 0;

            for (int i = 0; i <= N - 1; i++)
            {
                if (x[i])
                {
                    result = result + BigInteger.Pow(2, i);
                }
            }

            if (x[N - 1])
            {
                result = result - BigInteger.Pow(2, N);
            }

            return result;
        }

        // #impl-shared.SignExtend.2
        public static Bits SignExtend(Bits x, int N)
        {
            int M = x.Count;

            /* assert N >= M; */

            return Bits.Concat(Replicate(x[M - 1], N - M), x);
        }

        /* #impl-shared.SignExtend.1 */
        public static Bits SignExtend(int N, Bits x)
        {
            return SignExtend(x, N);
        }

        // #impl-shared.UInt.1
        public static BigInteger UInt(Bits x)
        {
            int N = x.Count;

            BigInteger result = 0;

            for (int i = 0; i <= N - 1; i++)
            {
                if (x[i])
                {
                    result = result + BigInteger.Pow(2, i);
                }
            }

            return result;
        }

        // #impl-shared.ZeroExtend.2
        public static Bits ZeroExtend(Bits x, int N)
        {
            int M = x.Count;

            /* assert N >= M; */

            return Bits.Concat(Zeros(N - M), x);
        }

        /* #impl-shared.ZeroExtend.1 */
        public static Bits ZeroExtend(int N, Bits x)
        {
            return ZeroExtend(x, N);
        }

        // #impl-shared.Zeros.1
        /* #impl-shared.Zeros.0 */
        public static Bits Zeros(int N)
        {
            return Replicate(false, N);
        }
#endregion

#region "functions/crc/"
        // #impl-shared.BitReverse.1
        public static Bits BitReverse(Bits data)
        {
            int N = data.Count;

            Bits result = new Bits(N);

            for (int i = 0; i <= N - 1; i++)
            {
                result[N - i - 1] = data[i];
            }

            return result;
        }

        // #impl-shared.Poly32Mod2.2
        public static Bits Poly32Mod2(Bits _data, Bits poly)
        {
            int N = _data.Count;

            /* assert N > 32; */

            Bits data = new Bits(_data);

            for (int i = N - 1; i >= 32; i--)
            {
                if (data[i])
                {
                    data[i - 1, 0] = EOR(data[i - 1, 0], Bits.Concat(poly, Zeros(i - 32)));
                }
            }

            return data[31, 0];
        }
#endregion

#region "functions/integer/"
        /* #impl-shared.AddWithCarry.3 */
        public static (Bits, Bits) AddWithCarry(int N, Bits x, Bits y, bool carry_in)
        {
            BigInteger unsigned_sum = UInt(x) + UInt(y) + UInt(carry_in);
            BigInteger signed_sum = SInt(x) + SInt(y) + UInt(carry_in);

            Bits result = unsigned_sum.SubBigInteger(N - 1, 0); // same value as signed_sum<N-1:0>

            bool n = result[N - 1];
            bool z = IsZero(result);
            bool c = !(UInt(result) == unsigned_sum);
            bool v = !(SInt(result) == signed_sum);

            return (result, Bits.Concat(n, z, c, v));
        }
#endregion

#region "functions/registers/"
        public static readonly Bits[] _R;

        public static Bits SP_EL0;
        public static Bits SP_EL1;
#endregion

#region "functions/system/"
        // #impl-shared.ConditionHolds.1
        public static bool ConditionHolds(Bits cond)
        {
            bool result;

            // Evaluate base condition.
            switch (cond[3, 1])
            {
                case Bits bits when bits == "000":
                    result = (PSTATE.Z == true);                          // EQ or NE
                    break;
                case Bits bits when bits == "001":
                    result = (PSTATE.C == true);                          // CS or CC
                    break;
                case Bits bits when bits == "010":
                    result = (PSTATE.N == true);                          // MI or PL
                    break;
                case Bits bits when bits == "011":
                    result = (PSTATE.V == true);                          // VS or VC
                    break;
                case Bits bits when bits == "100":
                    result = (PSTATE.C == true && PSTATE.Z == false);     // HI or LS
                    break;
                case Bits bits when bits == "101":
                    result = (PSTATE.N == PSTATE.V);                      // GE or LT
                    break;
                case Bits bits when bits == "110":
                    result = (PSTATE.N == PSTATE.V && PSTATE.Z == false); // GT or LE
                    break;
                default:
                case Bits bits when bits == "111":
                    result = true;                                        // AL
                    break;
            }

            // Condition flag values in the set '111x' indicate always true
            // Otherwise, invert condition if necessary.
            if (cond[0] == true && cond != "1111")
            {
                result = !result;
            }

            return result;
        }

        // #EL3
        public static readonly Bits EL3 = "11";
        // #EL2
        public static readonly Bits EL2 = "10";
        // #EL1
        public static readonly Bits EL1 = "01";
        // #EL0
        public static readonly Bits EL0 = "00";

        /* #impl-shared.HaveEL.1 */
        public static bool HaveEL(Bits el)
        {
            if (el == EL1 || el == EL0)
            {
                return true; // EL1 and EL0 must exist
            }

            return false;
        }

        public static ProcState PSTATE;

        /* #ProcState */
        internal struct ProcState
        {
            public void NZCV(Bits nzcv) // ASL: ".<,,,>".
            {
                N = nzcv[3];
                Z = nzcv[2];
                C = nzcv[1];
                V = nzcv[0];
            }

            public void NZCV(bool n, bool z, bool c, bool v) // ASL: ".<,,,>".
            {
                N = n;
                Z = z;
                C = c;
                V = v;
            }

            public bool N;  // Negative condition flag
            public bool Z;  // Zero condition flag
            public bool C;  // Carry condition flag
            public bool V;  // oVerflow condition flag
            public Bits EL; // Exception Level
            public bool SP; // Stack pointer select: 0=SP0, 1=SPx [AArch64 only]
        }
#endregion
    }
}
