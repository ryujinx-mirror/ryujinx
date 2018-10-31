#define SimdRegElemF

using ChocolArm64.State;

using NUnit.Framework;

using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdRegElemF")] // Tested: second half of 2018.
    public sealed class CpuTestSimdRegElemF : CpuTest
    {
#if SimdRegElemF

#region "ValueSource (Types)"
        private static IEnumerable<ulong> _1S_F_()
        {
            yield return 0x00000000FF7FFFFFul; // -Max Normal    (float.MinValue)
            yield return 0x0000000080800000ul; // -Min Normal
            yield return 0x00000000807FFFFFul; // -Max Subnormal
            yield return 0x0000000080000001ul; // -Min Subnormal (-float.Epsilon)
            yield return 0x000000007F7FFFFFul; // +Max Normal    (float.MaxValue)
            yield return 0x0000000000800000ul; // +Min Normal
            yield return 0x00000000007FFFFFul; // +Max Subnormal
            yield return 0x0000000000000001ul; // +Min Subnormal (float.Epsilon)

            if (!NoZeros)
            {
                yield return 0x0000000080000000ul; // -Zero
                yield return 0x0000000000000000ul; // +Zero
            }

            if (!NoInfs)
            {
                yield return 0x00000000FF800000ul; // -Infinity
                yield return 0x000000007F800000ul; // +Infinity
            }

            if (!NoNaNs)
            {
                yield return 0x00000000FFC00000ul; // -QNaN (all zeros payload) (float.NaN)
                yield return 0x00000000FFBFFFFFul; // -SNaN (all ones  payload)
                yield return 0x000000007FC00000ul; // +QNaN (all zeros payload) (-float.NaN) (DefaultNaN)
                yield return 0x000000007FBFFFFFul; // +SNaN (all ones  payload)
            }

            for (int Cnt = 1; Cnt <= RndCnt; Cnt++)
            {
                ulong Grbg = TestContext.CurrentContext.Random.NextUInt();
                ulong Rnd1 = GenNormal_S();
                ulong Rnd2 = GenSubnormal_S();

                yield return (Grbg << 32) | Rnd1;
                yield return (Grbg << 32) | Rnd2;
            }
        }

        private static IEnumerable<ulong> _2S_F_()
        {
            yield return 0xFF7FFFFFFF7FFFFFul; // -Max Normal    (float.MinValue)
            yield return 0x8080000080800000ul; // -Min Normal
            yield return 0x807FFFFF807FFFFFul; // -Max Subnormal
            yield return 0x8000000180000001ul; // -Min Subnormal (-float.Epsilon)
            yield return 0x7F7FFFFF7F7FFFFFul; // +Max Normal    (float.MaxValue)
            yield return 0x0080000000800000ul; // +Min Normal
            yield return 0x007FFFFF007FFFFFul; // +Max Subnormal
            yield return 0x0000000100000001ul; // +Min Subnormal (float.Epsilon)

            if (!NoZeros)
            {
                yield return 0x8000000080000000ul; // -Zero
                yield return 0x0000000000000000ul; // +Zero
            }

            if (!NoInfs)
            {
                yield return 0xFF800000FF800000ul; // -Infinity
                yield return 0x7F8000007F800000ul; // +Infinity
            }

            if (!NoNaNs)
            {
                yield return 0xFFC00000FFC00000ul; // -QNaN (all zeros payload) (float.NaN)
                yield return 0xFFBFFFFFFFBFFFFFul; // -SNaN (all ones  payload)
                yield return 0x7FC000007FC00000ul; // +QNaN (all zeros payload) (-float.NaN) (DefaultNaN)
                yield return 0x7FBFFFFF7FBFFFFFul; // +SNaN (all ones  payload)
            }

            for (int Cnt = 1; Cnt <= RndCnt; Cnt++)
            {
                ulong Rnd1 = GenNormal_S();
                ulong Rnd2 = GenSubnormal_S();

                yield return (Rnd1 << 32) | Rnd1;
                yield return (Rnd2 << 32) | Rnd2;
            }
        }

        private static IEnumerable<ulong> _1D_F_()
        {
            yield return 0xFFEFFFFFFFFFFFFFul; // -Max Normal    (double.MinValue)
            yield return 0x8010000000000000ul; // -Min Normal
            yield return 0x800FFFFFFFFFFFFFul; // -Max Subnormal
            yield return 0x8000000000000001ul; // -Min Subnormal (-double.Epsilon)
            yield return 0x7FEFFFFFFFFFFFFFul; // +Max Normal    (double.MaxValue)
            yield return 0x0010000000000000ul; // +Min Normal
            yield return 0x000FFFFFFFFFFFFFul; // +Max Subnormal
            yield return 0x0000000000000001ul; // +Min Subnormal (double.Epsilon)

            if (!NoZeros)
            {
                yield return 0x8000000000000000ul; // -Zero
                yield return 0x0000000000000000ul; // +Zero
            }

            if (!NoInfs)
            {
                yield return 0xFFF0000000000000ul; // -Infinity
                yield return 0x7FF0000000000000ul; // +Infinity
            }

            if (!NoNaNs)
            {
                yield return 0xFFF8000000000000ul; // -QNaN (all zeros payload) (double.NaN)
                yield return 0xFFF7FFFFFFFFFFFFul; // -SNaN (all ones  payload)
                yield return 0x7FF8000000000000ul; // +QNaN (all zeros payload) (-double.NaN) (DefaultNaN)
                yield return 0x7FF7FFFFFFFFFFFFul; // +SNaN (all ones  payload)
            }

            for (int Cnt = 1; Cnt <= RndCnt; Cnt++)
            {
                ulong Rnd1 = GenNormal_D();
                ulong Rnd2 = GenSubnormal_D();

                yield return Rnd1;
                yield return Rnd2;
            }
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _F_Mla_Mls_Se_S_()
        {
            return new uint[]
            {
                0x5F821020u, // FMLA S0, S1, V2.S[0]
                0x5F825020u  // FMLS S0, S1, V2.S[0]
            };
        }

        private static uint[] _F_Mla_Mls_Se_D_()
        {
            return new uint[]
            {
                0x5FC21020u, // FMLA D0, D1, V2.D[0]
                0x5FC25020u  // FMLS D0, D1, V2.D[0]
            };
        }

        private static uint[] _F_Mla_Mls_Ve_2S_4S_()
        {
            return new uint[]
            {
                0x0F801000u, // FMLA V0.2S, V0.2S, V0.S[0]
                0x0F805000u  // FMLS V0.2S, V0.2S, V0.S[0]
            };
        }

        private static uint[] _F_Mla_Mls_Ve_2D_()
        {
            return new uint[]
            {
                0x4FC01000u, // FMLA V0.2D, V0.2D, V0.D[0]
                0x4FC05000u  // FMLS V0.2D, V0.2D, V0.D[0]
            };
        }

        private static uint[] _F_Mul_Mulx_Se_S_()
        {
            return new uint[]
            {
                0x5F829020u, // FMUL  S0, S1, V2.S[0]
                0x7F829020u  // FMULX S0, S1, V2.S[0]
            };
        }

        private static uint[] _F_Mul_Mulx_Se_D_()
        {
            return new uint[]
            {
                0x5FC29020u, // FMUL  D0, D1, V2.D[0]
                0x7FC29020u  // FMULX D0, D1, V2.D[0]
            };
        }

        private static uint[] _F_Mul_Mulx_Ve_2S_4S_()
        {
            return new uint[]
            {
                0x0F809000u, // FMUL  V0.2S, V0.2S, V0.S[0]
                0x2F809000u  // FMULX V0.2S, V0.2S, V0.S[0]
            };
        }

        private static uint[] _F_Mul_Mulx_Ve_2D_()
        {
            return new uint[]
            {
                0x4FC09000u, // FMUL  V0.2D, V0.2D, V0.D[0]
                0x6FC09000u  // FMULX V0.2D, V0.2D, V0.D[0]
            };
        }
#endregion

        private const int RndCnt = 2;

        private static readonly bool NoZeros = false;
        private static readonly bool NoInfs  = false;
        private static readonly bool NoNaNs  = false;

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Mla_Mls_Se_S([ValueSource("_F_Mla_Mls_Se_S_")] uint Opcodes,
                                   [ValueSource("_1S_F_")] ulong Z,
                                   [ValueSource("_1S_F_")] ulong A,
                                   [ValueSource("_2S_F_")] ulong B,
                                   [Values(0u, 1u, 2u, 3u)] uint Index)
        {
            uint H = (Index >> 1) & 1;
            uint L = Index & 1;

            Opcodes |= (L << 21) | (H << 11);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FPSR.IOC, FpSkips.IfUnderflow, FpTolerances.UpToOneUlps_S);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Mla_Mls_Se_D([ValueSource("_F_Mla_Mls_Se_D_")] uint Opcodes,
                                   [ValueSource("_1D_F_")] ulong Z,
                                   [ValueSource("_1D_F_")] ulong A,
                                   [ValueSource("_1D_F_")] ulong B,
                                   [Values(0u, 1u)] uint Index)
        {
            uint H = Index & 1;

            Opcodes |= H << 11;

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FPSR.IOC, FpSkips.IfUnderflow, FpTolerances.UpToOneUlps_D);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Mla_Mls_Ve_2S_4S([ValueSource("_F_Mla_Mls_Ve_2S_4S_")] uint Opcodes,
                                       [Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [Values(2u, 0u)] uint Rm,
                                       [ValueSource("_2S_F_")] ulong Z,
                                       [ValueSource("_2S_F_")] ulong A,
                                       [ValueSource("_2S_F_")] ulong B,
                                       [Values(0u, 1u, 2u, 3u)] uint Index,
                                       [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            uint H = (Index >> 1) & 1;
            uint L = Index & 1;

            Opcodes |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (L << 21) | (H << 11);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FPSR.IOC, FpSkips.IfUnderflow, FpTolerances.UpToOneUlps_S);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Mla_Mls_Ve_2D([ValueSource("_F_Mla_Mls_Ve_2D_")] uint Opcodes,
                                    [Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_1D_F_")] ulong Z,
                                    [ValueSource("_1D_F_")] ulong A,
                                    [ValueSource("_1D_F_")] ulong B,
                                    [Values(0u, 1u)] uint Index)
        {
            uint H = Index & 1;

            Opcodes |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= H << 11;

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FPSR.IOC, FpSkips.IfUnderflow, FpTolerances.UpToOneUlps_D);
        }

        [Test, Pairwise] [Explicit]
        public void F_Mul_Mulx_Se_S([ValueSource("_F_Mul_Mulx_Se_S_")] uint Opcodes,
                                    [ValueSource("_1S_F_")] ulong A,
                                    [ValueSource("_2S_F_")] ulong B,
                                    [Values(0u, 1u, 2u, 3u)] uint Index)
        {
            uint H = (Index >> 1) & 1;
            uint L = Index & 1;

            Opcodes |= (L << 21) | (H << 11);

            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Mul_Mulx_Se_D([ValueSource("_F_Mul_Mulx_Se_D_")] uint Opcodes,
                                    [ValueSource("_1D_F_")] ulong A,
                                    [ValueSource("_1D_F_")] ulong B,
                                    [Values(0u, 1u)] uint Index)
        {
            uint H = Index & 1;

            Opcodes |= H << 11;

            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE1(Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Mul_Mulx_Ve_2S_4S([ValueSource("_F_Mul_Mulx_Ve_2S_4S_")] uint Opcodes,
                                        [Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_2S_F_")] ulong Z,
                                        [ValueSource("_2S_F_")] ulong A,
                                        [ValueSource("_2S_F_")] ulong B,
                                        [Values(0u, 1u, 2u, 3u)] uint Index,
                                        [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            uint H = (Index >> 1) & 1;
            uint L = Index & 1;

            Opcodes |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (L << 21) | (H << 11);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Mul_Mulx_Ve_2D([ValueSource("_F_Mul_Mulx_Ve_2D_")] uint Opcodes,
                                     [Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_1D_F_")] ulong Z,
                                     [ValueSource("_1D_F_")] ulong A,
                                     [ValueSource("_1D_F_")] ulong B,
                                     [Values(0u, 1u)] uint Index)
        {
            uint H = Index & 1;

            Opcodes |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= H << 11;

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B * H);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            CpuThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, V2: V2, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }
#endif
    }
}
