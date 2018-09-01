// https://github.com/LDj3SNuD/ARM_v8-A_AArch64_Instructions_Tester/blob/master/Tester/Pseudocode.cs

// https://developer.arm.com/products/architecture/a-profile/exploration-tools
// ..\A64_v83A_ISA_xml_00bet6.1\ISA_v83A_A64_xml_00bet6.1_OPT\xhtml\

// https://alastairreid.github.io/asl-lexical-syntax/

// | ------------------------|-------------------------------- |
// | ASL                     | C#                              |
// | ------------------------|-------------------------------- |
// | bit, bits(1); boolean   | bool                            |
// | bits                    | Bits                            |
// | integer                 | BigInteger, int                 |
// | real                    | decimal; double, float          |
// | ------------------------|-------------------------------- |
// | '0'; FALSE              | false                           |
// | '1'; TRUE               | true                            |
// | '010'                   | "010"                           |
// | DIV, MOD                | /, %                            |
// | ------------------------|-------------------------------- |

using System;
using System.Numerics;

namespace Ryujinx.Tests.Cpu.Tester
{
    using Types;

    using static Shared;

    internal static class AArch64
    {
#region "exceptions/exceptions/"
        /* shared_pseudocode.html#AArch64.ResetControlRegisters.1 */
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
            AArch64.ResetSIMDFPRegisters();
            AArch64.ResetSpecialRegisters();
        }
#endregion

#region "functions/registers/"
        /* shared_pseudocode.html#AArch64.ResetGeneralRegisters.0 */
        public static void ResetGeneralRegisters()
        {
            for (int i = 0; i <= 30; i++)
            {
                /* X[i] = bits(64) UNKNOWN; */
                _R[i].SetAll(false);
            }
        }

        /* shared_pseudocode.html#AArch64.ResetSIMDFPRegisters.0 */
        public static void ResetSIMDFPRegisters()
        {
            for (int i = 0; i <= 31; i++)
            {
                /* V[i] = bits(128) UNKNOWN; */
                _V[i].SetAll(false);
            }
        }

        /* shared_pseudocode.html#AArch64.ResetSpecialRegisters.0 */
        public static void ResetSpecialRegisters()
        {
            // AArch64 special registers
            /* SP_EL0 = bits(64) UNKNOWN; */
            SP_EL0.SetAll(false);
            /* SP_EL1 = bits(64) UNKNOWN; */
            SP_EL1.SetAll(false);

            FPCR.SetAll(false); // TODO: Add named fields.
            FPSR.SetAll(false); // TODO: Add named fields.
        }

        // shared_pseudocode.html#impl-aarch64.SP.write.0
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
                        break;/*
                    case Bits bits when bits == EL2:
                        SP_EL2 = ZeroExtend(64, value);
                        break;
                    case Bits bits when bits == EL3:
                        SP_EL3 = ZeroExtend(64, value);
                        break;*/
                }
            }
        }

        // shared_pseudocode.html#impl-aarch64.SP.read.0
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
                        return SP_EL1[width - 1, 0];/*
                    case Bits bits when bits == EL2:
                        return SP_EL2[width - 1, 0];
                    case Bits bits when bits == EL3:
                        return SP_EL3[width - 1, 0];*/
                }
            }
        }

        // shared_pseudocode.html#impl-aarch64.V.write.1
        public static void V(int n, Bits value)
        {
            /* int width = value.Count; */

            /* assert n >= 0 && n <= 31; */
            /* assert width IN {8,16,32,64,128}; */

            _V[n] = ZeroExtend(128, value);
        }

        /* shared_pseudocode.html#impl-aarch64.V.read.1 */
        public static Bits V(int width, int n)
        {
            /* assert n >= 0 && n <= 31; */
            /* assert width IN {8,16,32,64,128}; */

            return _V[n][width - 1, 0];
        }

        /* shared_pseudocode.html#impl-aarch64.Vpart.read.2 */
        public static Bits Vpart(int width, int n, int part)
        {
            /* assert n >= 0 && n <= 31; */
            /* assert part IN {0, 1}; */

            if (part == 0)
            {
                /* assert width IN {8,16,32,64}; */
                return _V[n][width - 1, 0];
            }
            else
            {
                /* assert width == 64; */
                return _V[n][(width * 2) - 1, width];
            }
        }

        // shared_pseudocode.html#impl-aarch64.Vpart.write.2
        public static void Vpart(int n, int part, Bits value)
        {
            int width = value.Count;

            /* assert n >= 0 && n <= 31; */
            /* assert part IN {0, 1}; */

            if (part == 0)
            {
                /* assert width IN {8,16,32,64}; */
                _V[n] = ZeroExtend(128, value);
            }
            else
            {
                /* assert width == 64; */
                _V[n][(width * 2) - 1, width] = value[width - 1, 0];
            }
        }

        // shared_pseudocode.html#impl-aarch64.X.write.1
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

        /* shared_pseudocode.html#impl-aarch64.X.read.1 */
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

#region "instrs/countop/"
        // shared_pseudocode.html#CountOp
        public enum CountOp {CountOp_CLZ, CountOp_CLS, CountOp_CNT};
#endregion

#region "instrs/extendreg/"
        /* shared_pseudocode.html#impl-aarch64.DecodeRegExtend.1 */
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

        /* shared_pseudocode.html#impl-aarch64.ExtendReg.3 */
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

        // shared_pseudocode.html#ExtendType
        public enum ExtendType {ExtendType_SXTB, ExtendType_SXTH, ExtendType_SXTW, ExtendType_SXTX,
                                ExtendType_UXTB, ExtendType_UXTH, ExtendType_UXTW, ExtendType_UXTX};
#endregion

#region "instrs/integer/bitmasks/"
        /* shared_pseudocode.html#impl-aarch64.DecodeBitMasks.4 */
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
        /* shared_pseudocode.html#impl-aarch64.DecodeShift.1 */
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

        /* shared_pseudocode.html#impl-aarch64.ShiftReg.3 */
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

        // shared_pseudocode.html#ShiftType
        public enum ShiftType {ShiftType_LSL, ShiftType_LSR, ShiftType_ASR, ShiftType_ROR};
#endregion

#region "instrs/vector/arithmetic/unary/cmp/compareop/"
        // shared_pseudocode.html#CompareOp
        public enum CompareOp {CompareOp_GT, CompareOp_GE, CompareOp_EQ, CompareOp_LE, CompareOp_LT};
#endregion

#region "instrs/vector/reduce/reduceop/"
        // shared_pseudocode.html#impl-aarch64.Reduce.3
        public static Bits Reduce(ReduceOp op, Bits input, int esize)
        {
            int N = input.Count;

            int half;
            Bits hi;
            Bits lo;
            Bits result = new Bits(esize);

            if (N == esize)
            {
                return new Bits(input); // Clone.
            }

            half = N / 2;
            hi = Reduce(op, input[N - 1, half], esize);
            lo = Reduce(op, input[half - 1, 0], esize);

            switch (op)
            {
                case ReduceOp.ReduceOp_FMINNUM:
                    /* result = FPMinNum(lo, hi, FPCR); */
                    break;
                case ReduceOp.ReduceOp_FMAXNUM:
                    /* result = FPMaxNum(lo, hi, FPCR); */
                    break;
                case ReduceOp.ReduceOp_FMIN:
                    /* result = FPMin(lo, hi, FPCR); */
                    break;
                case ReduceOp.ReduceOp_FMAX:
                    /* result = FPMax(lo, hi, FPCR); */
                    break;
                case ReduceOp.ReduceOp_FADD:
                    /* result = FPAdd(lo, hi, FPCR); */
                    break;
                default:
                case ReduceOp.ReduceOp_ADD:
                    result = lo + hi;
                    break;
            }

            return result;
        }

        // shared_pseudocode.html#ReduceOp
        public enum ReduceOp {ReduceOp_FMINNUM, ReduceOp_FMAXNUM,
                              ReduceOp_FMIN, ReduceOp_FMAX,
                              ReduceOp_FADD, ReduceOp_ADD};
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

            _V = new Bits[32];
            for (int i = 0; i <= 31; i++)
            {
                _V[i] = new Bits(128, false);
            }

            SP_EL0 = new Bits(64, false);
            SP_EL1 = new Bits(64, false);

            FPCR = new Bits(32, false); // TODO: Add named fields.
            FPSR = new Bits(32, false); // TODO: Add named fields.

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

        // shared_pseudocode.html#impl-shared.ASR.2
        public static Bits ASR(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x); // Clone.
            }
            else
            {
                (result, _) = ASR_C(x, shift);
            }

            return result;
        }

        // shared_pseudocode.html#impl-shared.ASR_C.2
        public static (Bits, bool) ASR_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift > 0; */

            Bits extended_x = SignExtend(x, shift + N);
            Bits result = extended_x[shift + N - 1, shift];
            bool carry_out = extended_x[shift - 1];

            return (result, carry_out);
        }

        // shared_pseudocode.html#impl-shared.Abs.1
        public static BigInteger Abs(BigInteger x)
        {
            return (x >= 0 ? x : -x);
        }

        // shared_pseudocode.html#impl-shared.BitCount.1
        public static int BitCount(Bits x)
        {
            int N = x.Count;

            int result = 0;

            for (int i = 0; i <= N - 1; i++)
            {
                if (x[i])
                {
                    result = result + 1;
                }
            }

            return result;
        }

        // shared_pseudocode.html#impl-shared.CountLeadingSignBits.1
        public static int CountLeadingSignBits(Bits x)
        {
            int N = x.Count;

            return CountLeadingZeroBits(EOR(x[N - 1, 1], x[N - 2, 0]));
        }

        // shared_pseudocode.html#impl-shared.CountLeadingZeroBits.1
        public static int CountLeadingZeroBits(Bits x)
        {
            int N = x.Count;

            return (N - 1 - HighestSetBit(x));
        }

        // shared_pseudocode.html#impl-shared.Elem.read.3
        public static Bits Elem(/*in */Bits vector, int e, int size)
        {
            /* int N = vector.Count; */

            /* assert e >= 0 && (e+1)*size <= N; */

            return vector[e * size + size - 1, e * size];
        }

        // shared_pseudocode.html#impl-shared.Elem.write.3
        public static void Elem(/*out */Bits vector, int e, int size, Bits value)
        {
            /* int N = vector.Count; */

            /* assert e >= 0 && (e+1)*size <= N; */

            vector[(e + 1) * size - 1, e * size] = value;
        }

        /* */
        public static Bits EOR(Bits x, Bits y)
        {
            return x.Xor(y);
        }

        // shared_pseudocode.html#impl-shared.Extend.3
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

        /* shared_pseudocode.html#impl-shared.Extend.2 */
        public static Bits Extend(int N, Bits x, bool unsigned)
        {
            return Extend(x, N, unsigned);
        }

        // shared_pseudocode.html#impl-shared.HighestSetBit.1
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

        // shared_pseudocode.html#impl-shared.Int.2
        public static BigInteger Int(Bits x, bool unsigned)
        {
            return (unsigned ? UInt(x) : SInt(x));
        }

        // shared_pseudocode.html#impl-shared.IsOnes.1
        public static bool IsOnes(Bits x)
        {
            int N = x.Count;

            return (x == Ones(N));
        }

        // shared_pseudocode.html#impl-shared.IsZero.1
        public static bool IsZero(Bits x)
        {
            int N = x.Count;

            return (x == Zeros(N));
        }

        // shared_pseudocode.html#impl-shared.IsZeroBit.1
        public static bool IsZeroBit(Bits x)
        {
            return IsZero(x);
        }

        // shared_pseudocode.html#impl-shared.LSL.2
        public static Bits LSL(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x); // Clone.
            }
            else
            {
                (result, _) = LSL_C(x, shift);
            }

            return result;
        }

        // shared_pseudocode.html#impl-shared.LSL_C.2
        public static (Bits, bool) LSL_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift > 0; */

            Bits extended_x = Bits.Concat(x, Zeros(shift));
            Bits result = extended_x[N - 1, 0];
            bool carry_out = extended_x[N];

            return (result, carry_out);
        }

        // shared_pseudocode.html#impl-shared.LSR.2
        public static Bits LSR(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x); // Clone.
            }
            else
            {
                (result, _) = LSR_C(x, shift);
            }

            return result;
        }

        // shared_pseudocode.html#impl-shared.LSR_C.2
        public static (Bits, bool) LSR_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift > 0; */

            Bits extended_x = ZeroExtend(x, shift + N);
            Bits result = extended_x[shift + N - 1, shift];
            bool carry_out = extended_x[shift - 1];

            return (result, carry_out);
        }

        // shared_pseudocode.html#impl-shared.Min.2
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

        /* shared_pseudocode.html#impl-shared.NOT.1 */
        public static Bits NOT(Bits x)
        {
            return x.Not();
        }

        // shared_pseudocode.html#impl-shared.Ones.1
        /* shared_pseudocode.html#impl-shared.Ones.0 */
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

        /* */
        public static float Real_32(BigInteger value)
        {
            if (value == BigInteger.Pow((BigInteger)2.0f, 1000))
            {
                return float.PositiveInfinity;
            }
            if (value == -BigInteger.Pow((BigInteger)2.0f, 1000))
            {
                return float.NegativeInfinity;
            }

            return (float)value;
        }

        /* */
        public static double Real_64(BigInteger value)
        {
            if (value == BigInteger.Pow((BigInteger)2.0, 10000))
            {
                return double.PositiveInfinity;
            }
            if (value == -BigInteger.Pow((BigInteger)2.0, 10000))
            {
                return double.NegativeInfinity;
            }

            return (double)value;
        }

        // shared_pseudocode.html#impl-shared.ROR.2
        public static Bits ROR(Bits x, int shift)
        {
            /* assert shift >= 0; */

            Bits result;

            if (shift == 0)
            {
                result = new Bits(x); // Clone.
            }
            else
            {
                (result, _) = ROR_C(x, shift);
            }

            return result;
        }

        // shared_pseudocode.html#impl-shared.ROR_C.2
        public static (Bits, bool) ROR_C(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift != 0; */

            int m = shift % N;
            Bits result = OR(LSR(x, m), LSL(x, N - m));
            bool carry_out = result[N - 1];

            return (result, carry_out);
        }

        /* shared_pseudocode.html#impl-shared.Replicate.1 */
        public static Bits Replicate(int N, Bits x)
        {
            int M = x.Count;

            /* assert N MOD M == 0; */

            return Replicate(x, N / M);
        }

        /* shared_pseudocode.html#impl-shared.Replicate.2 */
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

        /* shared_pseudocode.html#impl-shared.RoundDown.1 */
        public static BigInteger RoundDown(decimal x)
        {
            return (BigInteger)Decimal.Floor(x);
        }

        /* */
        public static BigInteger RoundDown_32(float x)
        {
            if (float.IsPositiveInfinity(x))
            {
                return BigInteger.Pow((BigInteger)2.0f, 1000);
            }
            if (float.IsNegativeInfinity(x))
            {
                return -BigInteger.Pow((BigInteger)2.0f, 1000);
            }

            return (BigInteger)MathF.Floor(x);
        }

        /* */
        public static BigInteger RoundDown_64(double x)
        {
            if (double.IsPositiveInfinity(x))
            {
                return BigInteger.Pow((BigInteger)2.0, 10000);
            }
            if (double.IsNegativeInfinity(x))
            {
                return -BigInteger.Pow((BigInteger)2.0, 10000);
            }

            return (BigInteger)Math.Floor(x);
        }

        // shared_pseudocode.html#impl-shared.RoundTowardsZero.1
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

        /* shared_pseudocode.html#impl-shared.RoundUp.1 */
        public static BigInteger RoundUp(decimal x)
        {
            return (BigInteger)Decimal.Ceiling(x);
        }

        // shared_pseudocode.html#impl-shared.SInt.1
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

        // shared_pseudocode.html#impl-shared.SignExtend.2
        public static Bits SignExtend(Bits x, int N)
        {
            int M = x.Count;

            /* assert N >= M; */

            return Bits.Concat(Replicate(x[M - 1], N - M), x);
        }

        /* shared_pseudocode.html#impl-shared.SignExtend.1 */
        public static Bits SignExtend(int N, Bits x)
        {
            return SignExtend(x, N);
        }

        // shared_pseudocode.html#impl-shared.UInt.1
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

        // shared_pseudocode.html#impl-shared.ZeroExtend.2
        public static Bits ZeroExtend(Bits x, int N)
        {
            int M = x.Count;

            /* assert N >= M; */

            return Bits.Concat(Zeros(N - M), x);
        }

        /* shared_pseudocode.html#impl-shared.ZeroExtend.1 */
        public static Bits ZeroExtend(int N, Bits x)
        {
            return ZeroExtend(x, N);
        }

        // shared_pseudocode.html#impl-shared.Zeros.1
        /* shared_pseudocode.html#impl-shared.Zeros.0 */
        public static Bits Zeros(int N)
        {
            return Replicate(false, N);
        }
#endregion

#region "functions/crc/"
        // shared_pseudocode.html#impl-shared.BitReverse.1
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

        // shared_pseudocode.html#impl-shared.Poly32Mod2.2
        public static Bits Poly32Mod2(Bits _data, Bits poly)
        {
            int N = _data.Count;

            /* assert N > 32; */

            Bits data = new Bits(_data); // Clone.

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

#region "functions/crypto/"
        // shared_pseudocode.html#impl-shared.ROL.2
        public static Bits ROL(Bits x, int shift)
        {
            int N = x.Count;

            /* assert shift >= 0 && shift <= N; */

            if (shift == 0)
            {
                return new Bits(x); // Clone.
            }

            return ROR(x, N - shift);
        }

        // shared_pseudocode.html#impl-shared.SHA256hash.4
        public static Bits SHA256hash(Bits _X, Bits _Y, Bits W, bool part1)
        {
            Bits X = new Bits(_X); // Clone.
            Bits Y = new Bits(_Y); // Clone.

            Bits chs, maj, t; // bits(32)

            for (int e = 0; e <= 3; e++)
            {
                chs = SHAchoose(Y[31, 0], Y[63, 32], Y[95, 64]);
                maj = SHAmajority(X[31, 0], X[63, 32], X[95, 64]);

                t = Y[127, 96] + SHAhashSIGMA1(Y[31, 0]) + chs + Elem(W, e, 32);

                X[127, 96] = t + X[127, 96];
                Y[127, 96] = t + SHAhashSIGMA0(X[31, 0]) + maj;

                // TODO: Implement ASL: "<,>" as C#: "Bits.Split()".
                /* <Y, X> = ROL(Y : X, 32); */
                Bits YX = ROL(Bits.Concat(Y, X), 32);
                Y = YX[255, 128];
                X = YX[127, 0];
            }

            return (part1 ? X : Y);
        }

        // shared_pseudocode.html#impl-shared.SHAchoose.3
        public static Bits SHAchoose(Bits x, Bits y, Bits z)
        {
            return EOR(AND(EOR(y, z), x), z);
        }

        // shared_pseudocode.html#impl-shared.SHAhashSIGMA0.1
        public static Bits SHAhashSIGMA0(Bits x)
        {
            return EOR(EOR(ROR(x, 2), ROR(x, 13)), ROR(x, 22));
        }

        // shared_pseudocode.html#impl-shared.SHAhashSIGMA1.1
        public static Bits SHAhashSIGMA1(Bits x)
        {
            return EOR(EOR(ROR(x, 6), ROR(x, 11)), ROR(x, 25));
        }

        // shared_pseudocode.html#impl-shared.SHAmajority.3
        public static Bits SHAmajority(Bits x, Bits y, Bits z)
        {
            return OR(AND(x, y), AND(OR(x, y), z));
        }
#endregion

#region "functions/float/fpdecoderounding/"
        /* shared_pseudocode.html#impl-shared.FPDecodeRounding.1 */
        public static FPRounding FPDecodeRounding(Bits rmode)
        {
            switch (rmode)
            {
                default:
                case Bits bits when bits == "00":
                    return FPRounding.FPRounding_TIEEVEN; // N
                case Bits bits when bits == "01":
                    return FPRounding.FPRounding_POSINF;  // P
                case Bits bits when bits == "10":
                    return FPRounding.FPRounding_NEGINF;  // M
                case Bits bits when bits == "11":
                    return FPRounding.FPRounding_ZERO;    // Z
            }
        }
#endregion

#region "functions/float/fpexc/"
        // shared_pseudocode.html#FPExc
        public enum FPExc {FPExc_InvalidOp, FPExc_DivideByZero, FPExc_Overflow,
                           FPExc_Underflow, FPExc_Inexact, FPExc_InputDenorm};
#endregion

#region "functions/float/fpprocessexception/"
        // shared_pseudocode.html#impl-shared.FPProcessException.2
        public static void FPProcessException(FPExc exception, Bits _fpcr)
        {
            Bits fpcr = new Bits(_fpcr); // Clone.

            int cumul;

            // Determine the cumulative exception bit number
            switch (exception)
            {
                default:
                case FPExc.FPExc_InvalidOp:    cumul = 0; break;
                case FPExc.FPExc_DivideByZero: cumul = 1; break;
                case FPExc.FPExc_Overflow:     cumul = 2; break;
                case FPExc.FPExc_Underflow:    cumul = 3; break;
                case FPExc.FPExc_Inexact:      cumul = 4; break;
                case FPExc.FPExc_InputDenorm:  cumul = 7; break;
            }

            int enable = cumul + 8;

            if (fpcr[enable])
            {
                // Trapping of the exception enabled.
                // It is IMPLEMENTATION DEFINED whether the enable bit may be set at all, and
                // if so then how exceptions may be accumulated before calling FPTrapException()
                /* IMPLEMENTATION_DEFINED "floating-point trap handling"; */

                throw new NotImplementedException();
            }/*
            else if (UsingAArch32())
            {
                // Set the cumulative exception bit
                FPSCR<cumul> = '1';
            }*/
            else
            {
                // Set the cumulative exception bit
                FPSR[cumul] = true;
            }
        }
#endregion

#region "functions/float/fprounding/"
        // shared_pseudocode.html#FPRounding
        public enum FPRounding {FPRounding_TIEEVEN, FPRounding_POSINF,
                                FPRounding_NEGINF, FPRounding_ZERO,
                                FPRounding_TIEAWAY, FPRounding_ODD};
#endregion

#region "functions/float/fptofixed/"
        /* shared_pseudocode.html#impl-shared.FPToFixed.5 */
        public static Bits FPToFixed(int M, Bits op, int fbits, bool unsigned, Bits _fpcr, FPRounding rounding)
        {
            int N = op.Count;

            /* assert N IN {16,32,64}; */
            /* assert M IN {16,32,64}; */
            /* assert fbits >= 0; */
            /* assert rounding != FPRounding_ODD; */

            Bits fpcr = new Bits(_fpcr); // Clone.

            if (N == 16)
            {
                throw new NotImplementedException();
            }
            else if (N == 32)
            {
                // Unpack using fpcr to determine if subnormals are flushed-to-zero
                (FPType type, bool sign, float value) = FPUnpack_32(op, fpcr);

                // If NaN, set cumulative flag or take exception
                if (type == FPType.FPType_SNaN || type == FPType.FPType_QNaN)
                {
                    FPProcessException(FPExc.FPExc_InvalidOp, fpcr);
                }

                // Scale by fractional bits and produce integer rounded towards minus-infinity
                value = value * MathF.Pow(2.0f, fbits);
                BigInteger int_result = RoundDown_32(value);
                float error = value - Real_32(int_result);

                if (float.IsNaN(error))
                {
                    error = 0.0f;
                }

                // Determine whether supplied rounding mode requires an increment
                bool round_up;

                switch (rounding)
                {
                    default:
                    case FPRounding.FPRounding_TIEEVEN:
                        round_up = (error > 0.5f || (error == 0.5f && int_result.SubBigInteger(0)));
                        break;
                    case FPRounding.FPRounding_POSINF:
                        round_up = (error != 0.0f);
                        break;
                    case FPRounding.FPRounding_NEGINF:
                        round_up = false;
                        break;
                    case FPRounding.FPRounding_ZERO:
                        round_up = (error != 0.0f && int_result < (BigInteger)0);
                        break;
                    case FPRounding.FPRounding_TIEAWAY:
                        round_up = (error > 0.5f || (error == 0.5f && int_result >= (BigInteger)0));
                        break;
                }

                if (round_up)
                {
                    int_result = int_result + 1;
                }

                // Generate saturated result and exceptions
                (Bits result, bool overflow) = SatQ(int_result, M, unsigned);

                if (overflow)
                {
                    FPProcessException(FPExc.FPExc_InvalidOp, fpcr);
                }
                else if (error != 0.0f)
                {
                    FPProcessException(FPExc.FPExc_Inexact, fpcr);
                }

                return result;
            }
            else /* if (N == 64) */
            {
                // Unpack using fpcr to determine if subnormals are flushed-to-zero
                (FPType type, bool sign, double value) = FPUnpack_64(op, fpcr);

                // If NaN, set cumulative flag or take exception
                if (type == FPType.FPType_SNaN || type == FPType.FPType_QNaN)
                {
                    FPProcessException(FPExc.FPExc_InvalidOp, fpcr);
                }

                // Scale by fractional bits and produce integer rounded towards minus-infinity
                value = value * Math.Pow(2.0, fbits);
                BigInteger int_result = RoundDown_64(value);
                double error = value - Real_64(int_result);

                if (double.IsNaN(error))
                {
                    error = 0.0;
                }

                // Determine whether supplied rounding mode requires an increment
                bool round_up;

                switch (rounding)
                {
                    default:
                    case FPRounding.FPRounding_TIEEVEN:
                        round_up = (error > 0.5 || (error == 0.5 && int_result.SubBigInteger(0)));
                        break;
                    case FPRounding.FPRounding_POSINF:
                        round_up = (error != 0.0);
                        break;
                    case FPRounding.FPRounding_NEGINF:
                        round_up = false;
                        break;
                    case FPRounding.FPRounding_ZERO:
                        round_up = (error != 0.0 && int_result < (BigInteger)0);
                        break;
                    case FPRounding.FPRounding_TIEAWAY:
                        round_up = (error > 0.5 || (error == 0.5 && int_result >= (BigInteger)0));
                        break;
                }

                if (round_up)
                {
                    int_result = int_result + 1;
                }

                // Generate saturated result and exceptions
                (Bits result, bool overflow) = SatQ(int_result, M, unsigned);

                if (overflow)
                {
                    FPProcessException(FPExc.FPExc_InvalidOp, fpcr);
                }
                else if (error != 0.0)
                {
                    FPProcessException(FPExc.FPExc_Inexact, fpcr);
                }

                return result;
            }
        }
#endregion

#region "functions/float/fptype/"
        // shared_pseudocode.html#FPType
        public enum FPType {FPType_Nonzero, FPType_Zero, FPType_Infinity,
                            FPType_QNaN, FPType_SNaN};
#endregion

#region "functions/float/fpunpack/"
        /* shared_pseudocode.html#impl-shared.FPUnpack.2 */
        /* shared_pseudocode.html#impl-shared.FPUnpackBase.2 */
        /*public static (FPType, bool, real) FPUnpack_16(Bits fpval, Bits _fpcr)
        {
            int N = fpval.Count;

            // assert N == 16;

            Bits fpcr = new Bits(_fpcr); // Clone.

            fpcr[26] = false;

            return FPUnpackBase_16(fpval, fpcr);
        }*/
        public static (FPType, bool, float) FPUnpack_32(Bits fpval, Bits _fpcr)
        {
            int N = fpval.Count;

            /* assert N == 32; */

            Bits fpcr = new Bits(_fpcr); // Clone.

            FPType type;
            float value;

            bool sign   = fpval[31];
            Bits exp32  = fpval[30, 23];
            Bits frac32 = fpval[22, 0];

            if (IsZero(exp32))
            {
                // Produce zero if value is zero or flush-to-zero is selected.
                if (IsZero(frac32) || fpcr[24])
                {
                    type = FPType.FPType_Zero;
                    value = 0.0f;

                    // Denormalized input flushed to zero
                    if (!IsZero(frac32))
                    {
                        FPProcessException(FPExc.FPExc_InputDenorm, fpcr);
                    }
                }
                else
                {
                    type = FPType.FPType_Nonzero;
                    value = MathF.Pow(2.0f, -126) * (Real_32(UInt(frac32)) * MathF.Pow(2.0f, -23));
                }
            }
            else if (IsOnes(exp32))
            {
                if (IsZero(frac32))
                {
                    type = FPType.FPType_Infinity;
                    /* value = 2.0^1000000; */
                    value = MathF.Pow(2.0f, 1000);
                }
                else
                {
                    type = frac32[22] ? FPType.FPType_QNaN : FPType.FPType_SNaN;
                    value = 0.0f;
                }
            }
            else
            {
                type = FPType.FPType_Nonzero;
                value = MathF.Pow(2.0f, (int)UInt(exp32) - 127) * (1.0f + Real_32(UInt(frac32)) * MathF.Pow(2.0f, -23));
            }

            if (sign)
            {
                value = -value;
            }

            return (type, sign, value);
        }
        public static (FPType, bool, double) FPUnpack_64(Bits fpval, Bits _fpcr)
        {
            int N = fpval.Count;

            /* assert N == 64; */

            Bits fpcr = new Bits(_fpcr); // Clone.

            FPType type;
            double value;

            bool sign   = fpval[63];
            Bits exp64  = fpval[62, 52];
            Bits frac64 = fpval[51, 0];

            if (IsZero(exp64))
            {
                // Produce zero if value is zero or flush-to-zero is selected.
                if (IsZero(frac64) || fpcr[24])
                {
                    type = FPType.FPType_Zero;
                    value = 0.0;

                    // Denormalized input flushed to zero
                    if (!IsZero(frac64))
                    {
                        FPProcessException(FPExc.FPExc_InputDenorm, fpcr);
                    }
                }
                else
                {
                    type = FPType.FPType_Nonzero;
                    value = Math.Pow(2.0, -1022) * (Real_64(UInt(frac64)) * Math.Pow(2.0, -52));
                }
            }
            else if (IsOnes(exp64))
            {
                if (IsZero(frac64))
                {
                    type = FPType.FPType_Infinity;
                    /* value = 2.0^1000000; */
                    value = Math.Pow(2.0, 10000);
                }
                else
                {
                    type = frac64[51] ? FPType.FPType_QNaN : FPType.FPType_SNaN;
                    value = 0.0;
                }
            }
            else
            {
                type = FPType.FPType_Nonzero;
                value = Math.Pow(2.0, (int)UInt(exp64) - 1023) * (1.0 + Real_64(UInt(frac64)) * Math.Pow(2.0, -52));
            }

            if (sign)
            {
                value = -value;
            }

            return (type, sign, value);
        }

        /* shared_pseudocode.html#impl-shared.FPUnpackCV.2 */
        /* shared_pseudocode.html#impl-shared.FPUnpackBase.2 */
        /*public static (FPType, bool, real) FPUnpackCV_16(Bits fpval, Bits _fpcr)
        {
            int N = fpval.Count;

            // assert N == 16;

            Bits fpcr = new Bits(_fpcr); // Clone.

            fpcr[19] = false;

            return FPUnpackBase_16(fpval, fpcr);
        }*/
        public static (FPType, bool, float) FPUnpackCV_32(Bits fpval, Bits _fpcr)
        {
            return FPUnpack_32(fpval, _fpcr);
        }
        public static (FPType, bool, double) FPUnpackCV_64(Bits fpval, Bits _fpcr)
        {
            return FPUnpack_64(fpval, _fpcr);
        }
#endregion

#region "functions/integer/"
        /* shared_pseudocode.html#impl-shared.AddWithCarry.3 */
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

        public static readonly Bits[] _V;

        public static Bits SP_EL0;
        public static Bits SP_EL1;

        public static Bits FPCR; // TODO: Add named fields.
        // [ 31 | 30 | 29 | 28 | 27 | 26  | 25 | 24 | 23 22 | 21 20  | 19   | 18 17 16 | 15  | 14 | 13 | 12  | 11  | 10  | 9   | 8   | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 ]
        // [ 0  | 0  | 0  | 0  | 0  | AHP | DN | FZ | RMode | Stride | FZ16 | Len      | IDE | 0  | 0  | IXE | UFE | OFE | DZE | IOE | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 ]
        public static Bits FPSR; // TODO: Add named fields.
        // [ 31 | 30 | 29 | 28 | 27 | 26 | 25 | 24 | 23 | 22 | 21 | 20 | 19 | 18 | 17 | 16 | 15 | 14 | 13 | 12 | 11 | 10 | 9 | 8 | 7   | 6 | 5 | 4   | 3   | 2   | 1   | 0   ]
        // [ N  | Z  | C  | V  | QC | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0  | 0 | 0 | IDC | 0 | 0 | IXC | UFC | OFC | DZC | IOC ]
#endregion

#region "functions/system/"
        // shared_pseudocode.html#impl-shared.ConditionHolds.1
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

        // shared_pseudocode.html#EL3
        public static readonly Bits EL3 = "11";
        // shared_pseudocode.html#EL2
        public static readonly Bits EL2 = "10";
        // shared_pseudocode.html#EL1
        public static readonly Bits EL1 = "01";
        // shared_pseudocode.html#EL0
        public static readonly Bits EL0 = "00";

        /* shared_pseudocode.html#impl-shared.HaveEL.1 */
        public static bool HaveEL(Bits el)
        {
            // TODO: Implement ASL: "IN" as C#: "Bits.In()".
            /* if el IN {EL1,EL0} then */
            if (el == EL1 || el == EL0)
            {
                return true; // EL1 and EL0 must exist
            }

            /* return boolean IMPLEMENTATION_DEFINED; */
            return false;
        }

        public static ProcState PSTATE;

        /* shared_pseudocode.html#ProcState */
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

#region "functions/vector/"
        // shared_pseudocode.html#impl-shared.SatQ.3
        public static (Bits, bool) SatQ(BigInteger i, int N, bool unsigned)
        {
            (Bits result, bool sat) = (unsigned ? UnsignedSatQ(i, N) : SignedSatQ(i, N));

            return (result, sat);
        }

        // shared_pseudocode.html#impl-shared.SignedSatQ.2
        public static (Bits, bool) SignedSatQ(BigInteger i, int N)
        {
            BigInteger result;
            bool saturated;

            if (i > BigInteger.Pow(2, N - 1) - 1)
            {
                result = BigInteger.Pow(2, N - 1) - 1;
                saturated = true;
            }
            else if (i < -(BigInteger.Pow(2, N - 1)))
            {
                result = -(BigInteger.Pow(2, N - 1));
                saturated = true;
            }
            else
            {
                result = i;
                saturated = false;
            }

            return (result.SubBigInteger(N - 1, 0), saturated);
        }

        // shared_pseudocode.html#impl-shared.UnsignedSatQ.2
        public static (Bits, bool) UnsignedSatQ(BigInteger i, int N)
        {
            BigInteger result;
            bool saturated;

            if (i > BigInteger.Pow(2, N) - 1)
            {
                result = BigInteger.Pow(2, N) - 1;
                saturated = true;
            }
            else if (i < (BigInteger)0)
            {
                result = (BigInteger)0;
                saturated = true;
            }
            else
            {
                result = i;
                saturated = false;
            }

            return (result.SubBigInteger(N - 1, 0), saturated);
        }
#endregion
    }
}
