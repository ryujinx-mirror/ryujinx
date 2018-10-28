#define Simd

using ChocolArm64.State;

using NUnit.Framework;

using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("Simd")] // Tested: second half of 2018.
    public sealed class CpuTestSimd : CpuTest
    {
#if Simd

#region "ValueSource (Types)"
        private static ulong[] _1B1H1S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x000000000000007Ful,
                                 0x0000000000000080ul, 0x00000000000000FFul,
                                 0x0000000000007FFFul, 0x0000000000008000ul,
                                 0x000000000000FFFFul, 0x000000007FFFFFFFul,
                                 0x0000000080000000ul, 0x00000000FFFFFFFFul,
                                 0x7FFFFFFFFFFFFFFFul, 0x8000000000000000ul,
                                 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _1H1S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x0000000000007FFFul,
                                 0x0000000000008000ul, 0x000000000000FFFFul,
                                 0x000000007FFFFFFFul, 0x0000000080000000ul,
                                 0x00000000FFFFFFFFul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _4H2S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B4H_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B4H2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B4H2S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static IEnumerable<ulong> _4H_F_()
        {
            yield return 0xFBFFFBFFFBFFFBFFul; // -Max Normal
            yield return 0x8400840084008400ul; // -Min Normal
            yield return 0x83FF83FF83FF83FFul; // -Max Subnormal
            yield return 0x8001800180018001ul; // -Min Subnormal
            yield return 0x7BFF7BFF7BFF7BFFul; // +Max Normal
            yield return 0x0400040004000400ul; // +Min Normal
            yield return 0x03FF03FF03FF03FFul; // +Max Subnormal
            yield return 0x0001000100010001ul; // +Min Subnormal

            if (!NoZeros)
            {
                yield return 0x8000800080008000ul; // -Zero
                yield return 0x0000000000000000ul; // +Zero
            }

            if (!NoInfs)
            {
                yield return 0xFC00FC00FC00FC00ul; // -Infinity
                yield return 0x7C007C007C007C00ul; // +Infinity
            }

            if (!NoNaNs)
            {
                yield return 0xFE00FE00FE00FE00ul; // -QNaN (all zeros payload)
                yield return 0xFDFFFDFFFDFFFDFFul; // -SNaN (all ones  payload)
                yield return 0x7E007E007E007E00ul; // +QNaN (all zeros payload) (DefaultNaN)
                yield return 0x7DFF7DFF7DFF7DFFul; // +SNaN (all ones  payload)
            }

            for (int Cnt = 1; Cnt <= RndCnt; Cnt++)
            {
                uint Rnd1 = (uint)GenNormal_H();
                uint Rnd2 = (uint)GenSubnormal_H();

                yield return (Rnd1 << 48) | (Rnd1 << 32) | (Rnd1 << 16) | Rnd1;
                yield return (Rnd2 << 48) | (Rnd2 << 32) | (Rnd2 << 16) | Rnd2;
            }
        }

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
        private static uint[] _F_Cvt_S_SD_()
        {
            return new uint[]
            {
                0x1E22C020u // FCVT D0, S1
            };
        }

        private static uint[] _F_Cvt_S_DS_()
        {
            return new uint[]
            {
                0x1E624020u // FCVT S0, D1
            };
        }

        private static uint[] _F_Cvt_NZ_SU_S_S_()
        {
            return new uint[]
            {
                0x5E21A820u, // FCVTNS S0, S1
                0x7E21A820u, // FCVTNU S0, S1
                0x5EA1B820u, // FCVTZS S0, S1
                0x7EA1B820u  // FCVTZU S0, S1
            };
        }

        private static uint[] _F_Cvt_NZ_SU_S_D_()
        {
            return new uint[]
            {
                0x5E61A820u, // FCVTNS D0, D1
                0x7E61A820u, // FCVTNU D0, D1
                0x5EE1B820u, // FCVTZS D0, D1
                0x7EE1B820u  // FCVTZU D0, D1
            };
        }

        private static uint[] _F_Cvt_NZ_SU_V_2S_4S_()
        {
            return new uint[]
            {
                0x0E21A800u, // FCVTNS V0.2S, V0.2S
                0x2E21A800u, // FCVTNU V0.2S, V0.2S
                0x0EA1B800u, // FCVTZS V0.2S, V0.2S
                0x2EA1B800u  // FCVTZU V0.2S, V0.2S
            };
        }

        private static uint[] _F_Cvt_NZ_SU_V_2D_()
        {
            return new uint[]
            {
                0x4E61A800u, // FCVTNS V0.2D, V0.2D
                0x6E61A800u, // FCVTNU V0.2D, V0.2D
                0x4EE1B800u, // FCVTZS V0.2D, V0.2D
                0x6EE1B800u  // FCVTZU V0.2D, V0.2D
            };
        }

        private static uint[] _F_Cvtl_V_4H4S_8H4S_()
        {
            return new uint[]
            {
                0x0E217800u // FCVTL V0.4S, V0.4H
            };
        }

        private static uint[] _F_Cvtl_V_2S2D_4S2D_()
        {
            return new uint[]
            {
                0x0E617800u // FCVTL V0.2D, V0.2S
            };
        }

        private static uint[] _F_Cvtn_V_4S4H_4S8H_()
        {
            return new uint[]
            {
                0x0E216800u // FCVTN V0.4H, V0.4S
            };
        }

        private static uint[] _F_Cvtn_V_2D2S_2D4S_()
        {
            return new uint[]
            {
                0x0E616800u // FCVTN V0.2S, V0.2D
            };
        }

        private static uint[] _F_Recpx_Sqrt_S_S_()
        {
            return new uint[]
            {
                0x5EA1F820u, // FRECPX S0, S1
                0x1E21C020u  // FSQRT  S0, S1
            };
        }

        private static uint[] _F_Recpx_Sqrt_S_D_()
        {
            return new uint[]
            {
                0x5EE1F820u, // FRECPX D0, D1
                0x1E61C020u  // FSQRT  D0, D1
            };
        }

        private static uint[] _F_Sqrt_V_2S_4S_()
        {
            return new uint[]
            {
                0x2EA1F800u // FSQRT V0.2S, V0.2S
            };
        }

        private static uint[] _F_Sqrt_V_2D_()
        {
            return new uint[]
            {
                0x6EE1F800u // FSQRT V0.2D, V0.2D
            };
        }

        private static uint[] _Sha1h_Sha1su1_V_()
        {
            return new uint[]
            {
                0x5E280800u, // SHA1H   S0,    S0
                0x5E281800u  // SHA1SU1 V0.4S, V0.4S
            };
        }

        private static uint[] _Sha256su0_V_()
        {
            return new uint[]
            {
                0x5E282800u // SHA256SU0 V0.4S, V0.4S
            };
        }
#endregion

        private const int RndCnt = 2;

        private static readonly bool NoZeros = false;
        private static readonly bool NoInfs  = false;
        private static readonly bool NoNaNs  = false;

        [Test, Pairwise, Description("ABS <V><d>, <V><n>")]
        public void Abs_S_D([Values(0u)]     uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE0B800; // ABS D0, D0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_8B_4H_2S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E20B800; // ABS V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E20B800; // ABS V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDP <V><d>, <Vn>.<T>")]
        public void Addp_S_2DD([Values(0u)]     uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EF1B800; // ADDP D0, V0.2D
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDV <V><d>, <Vn>.<T>")]
        public void Addv_V_8BB_4HH([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u)] uint size) // <8BB, 4HH>
        {
            uint Opcode = 0x0E31B800; // ADDV B0, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDV <V><d>, <Vn>.<T>")]
        public void Addv_V_16BB_8HH_4SS([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u)] uint size) // <16BB, 8HH, 4SS>
        {
            uint Opcode = 0x4E31B800; // ADDV B0, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLS <Vd>.<T>, <Vn>.<T>")]
        public void Cls_V_8B_4H_2S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E204800; // CLS V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLS <Vd>.<T>, <Vn>.<T>")]
        public void Cls_V_16B_8H_4S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E204800; // CLS V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Vd>.<T>, <Vn>.<T>")]
        public void Clz_V_8B_4H_2S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E204800; // CLZ V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Vd>.<T>, <Vn>.<T>")]
        public void Clz_V_16B_8H_4S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E204800; // CLZ V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <V><d>, <V><n>, #0")]
        public void Cmeq_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE09800; // CMEQ D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmeq_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E209800; // CMEQ V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmeq_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E209800; // CMEQ V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <V><d>, <V><n>, #0")]
        public void Cmge_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x7EE08800; // CMGE D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmge_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E208800; // CMGE V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmge_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E208800; // CMGE V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <V><d>, <V><n>, #0")]
        public void Cmgt_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE08800; // CMGT D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmgt_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E208800; // CMGT V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmgt_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E208800; // CMGT V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLE <V><d>, <V><n>, #0")]
        public void Cmle_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x7EE09800; // CMLE D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmle_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E209800; // CMLE V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmle_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E209800; // CMLE V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLT <V><d>, <V><n>, #0")]
        public void Cmlt_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE0A800; // CMLT D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmlt_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E20A800; // CMLT V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmlt_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E20A800; // CMLT V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CNT <Vd>.<T>, <Vn>.<T>")]
        public void Cnt_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x0E205800; // CNT V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CNT <Vd>.<T>, <Vn>.<T>")]
        public void Cnt_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x4E205800; // CNT V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_S_SD([ValueSource("_F_Cvt_S_SD_")] uint Opcodes,
                               [ValueSource("_1S_F_")] ulong A)
        {
            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE1(Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_S_DS([ValueSource("_F_Cvt_S_DS_")] uint Opcodes,
                               [ValueSource("_1D_F_")] ulong A)
        {
            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_S_S([ValueSource("_F_Cvt_NZ_SU_S_S_")] uint Opcodes,
                                    [ValueSource("_1S_F_")] ulong A)
        {
            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_S_D([ValueSource("_F_Cvt_NZ_SU_S_D_")] uint Opcodes,
                                    [ValueSource("_1D_F_")] ulong A)
        {
            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE1(Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_V_2S_4S([ValueSource("_F_Cvt_NZ_SU_V_2S_4S_")] uint Opcodes,
                                        [Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_2S_F_")] ulong Z,
                                        [ValueSource("_2S_F_")] ulong A,
                                        [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_V_2D([ValueSource("_F_Cvt_NZ_SU_V_2D_")] uint Opcodes,
                                     [Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1D_F_")] ulong Z,
                                     [ValueSource("_1D_F_")] ulong A)
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvtl_V_4H4S_8H4S([ValueSource("_F_Cvtl_V_4H4S_8H4S_")] uint Opcodes,
                                       [Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_4H_F_")] ulong Z,
                                       [ValueSource("_4H_F_")] ulong A,
                                       [Values(0b0u, 0b1u)] uint Q, // <4H, 8H>
                                       [Values(RMode.RN)] RMode RMode)
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Q == 0u ? Z : 0ul, Q == 1u ? Z : 0ul);
            Vector128<float> V1 = MakeVectorE0E1(Q == 0u ? A : 0ul, Q == 1u ? A : 0ul);

            int Rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int Fpcr = (int)RMode << (int)FPCR.RMode;
            Fpcr |= Rnd & (1 << (int)FPCR.FZ);
            Fpcr |= Rnd & (1 << (int)FPCR.DN);
            Fpcr |= Rnd & (1 << (int)FPCR.AHP);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC | FPSR.OFC | FPSR.UFC | FPSR.IXC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvtl_V_2S2D_4S2D([ValueSource("_F_Cvtl_V_2S2D_4S2D_")] uint Opcodes,
                                       [Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_2S_F_")] ulong Z,
                                       [ValueSource("_2S_F_")] ulong A,
                                       [Values(0b0u, 0b1u)] uint Q, // <2S, 4S>
                                       [Values(RMode.RN)] RMode RMode)
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Q == 0u ? Z : 0ul, Q == 1u ? Z : 0ul);
            Vector128<float> V1 = MakeVectorE0E1(Q == 0u ? A : 0ul, Q == 1u ? A : 0ul);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvtn_V_4S4H_4S8H([ValueSource("_F_Cvtn_V_4S4H_4S8H_")] uint Opcodes,
                                       [Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_2S_F_")] ulong Z,
                                       [ValueSource("_2S_F_")] ulong A,
                                       [Values(0b0u, 0b1u)] uint Q, // <4H, 8H>
                                       [Values(RMode.RN)] RMode RMode) // Unicorn seems to default all rounding modes to RMode.RN.
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            int Rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int Fpcr = (int)RMode << (int)FPCR.RMode;
            Fpcr |= Rnd & (1 << (int)FPCR.FZ);
            Fpcr |= Rnd & (1 << (int)FPCR.DN);
            Fpcr |= Rnd & (1 << (int)FPCR.AHP);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC | FPSR.OFC | FPSR.UFC | FPSR.IXC | FPSR.IDC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvtn_V_2D2S_2D4S([ValueSource("_F_Cvtn_V_2D2S_2D4S_")] uint Opcodes,
                                       [Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_1D_F_")] ulong Z,
                                       [ValueSource("_1D_F_")] ulong A,
                                       [Values(0b0u, 0b1u)] uint Q, // <2S, 4S>
                                       [Values(RMode.RN)] RMode RMode) // Unicorn seems to default all rounding modes to RMode.RN.
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Recpx_Sqrt_S_S([ValueSource("_F_Recpx_Sqrt_S_S_")] uint Opcodes,
                                     [ValueSource("_1S_F_")] ulong A)
        {
            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Recpx_Sqrt_S_D([ValueSource("_F_Recpx_Sqrt_S_D_")] uint Opcodes,
                                     [ValueSource("_1D_F_")] ulong A)
        {
            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE1(Z);
            Vector128<float> V1 = MakeVectorE0(A);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Sqrt_V_2S_4S([ValueSource("_F_Sqrt_V_2S_4S_")] uint Opcodes,
                                   [Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_2S_F_")] ulong Z,
                                   [ValueSource("_2S_F_")] ulong A,
                                   [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise] [Explicit]
        public void F_Sqrt_V_2D([ValueSource("_F_Sqrt_V_2D_")] uint Opcodes,
                                [Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [ValueSource("_1D_F_")] ulong Z,
                                [ValueSource("_1D_F_")] ulong A)
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            int Fpcr = (int)TestContext.CurrentContext.Random.NextUInt() & (1 << (int)FPCR.DN);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1, Fpcr: Fpcr);

            CompareAgainstUnicorn(FpsrMask: FPSR.IOC);
        }

        [Test, Pairwise, Description("NEG <V><d>, <V><n>")]
        public void Neg_S_D([Values(0u)]     uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x7EE0B800; // NEG D0, D0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_8B_4H_2S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E20B800; // NEG V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E20B800; // NEG V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NOT <Vd>.<T>, <Vn>.<T>")]
        public void Not_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x2E205800; // NOT V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NOT <Vd>.<T>, <Vn>.<T>")]
        public void Not_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x6E205800; // NOT V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Vd>.<T>, <Vn>.<T>")]
        public void Rbit_V_8B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x2E605800; // RBIT V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Vd>.<T>, <Vn>.<T>")]
        public void Rbit_V_16B([Values(0u)]     uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x6E605800; // RBIT V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Vd>.<T>, <Vn>.<T>")]
        public void Rev16_V_8B([Values(0u)]     uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x0E201800; // REV16 V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Vd>.<T>, <Vn>.<T>")]
        public void Rev16_V_16B([Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                                [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x4E201800; // REV16 V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV32 <Vd>.<T>, <Vn>.<T>")]
        public void Rev32_V_8B_4H([Values(0u)]     uint Rd,
                                  [Values(1u, 0u)] uint Rn,
                                  [ValueSource("_8B4H_")] [Random(RndCnt)] ulong Z,
                                  [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                                  [Values(0b00u, 0b01u)] uint size) // <8B, 4H>
        {
            uint Opcode = 0x2E200800; // REV32 V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV32 <Vd>.<T>, <Vn>.<T>")]
        public void Rev32_V_16B_8H([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u)] uint size) // <16B, 8H>
        {
            uint Opcode = 0x6E200800; // REV32 V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV64 <Vd>.<T>, <Vn>.<T>")]
        public void Rev64_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E200800; // REV64 V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV64 <Vd>.<T>, <Vn>.<T>")]
        public void Rev64_V_16B_8H_4S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E200800; // REV64 V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Sadalp_V_8B4H_4H2S_2S1D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint Opcode = 0x0E206800; // SADALP V0.4H, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Sadalp_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x4E206800; // SADALP V0.8H, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Saddlp_V_8B4H_4H2S_2S1D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint Opcode = 0x0E202800; // SADDLP V0.4H, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Saddlp_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x4E202800; // SADDLP V0.8H, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Sha1h_Sha1su1_V([ValueSource("_Sha1h_Sha1su1_V_")] uint Opcodes,
                                    [Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Random(RndCnt / 2)] ulong Z0, [Random(RndCnt / 2)] ulong Z1,
                                    [Random(RndCnt / 2)] ulong A0, [Random(RndCnt / 2)] ulong A1)
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z0, Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Sha256su0_V([ValueSource("_Sha256su0_V_")] uint Opcodes,
                                [Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [Random(RndCnt / 2)] ulong Z0, [Random(RndCnt / 2)] ulong Z1,
                                [Random(RndCnt / 2)] ulong A0, [Random(RndCnt / 2)] ulong A1)
        {
            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            Vector128<float> V0 = MakeVectorE0E1(Z0, Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SQABS <V><d>, <V><n>")]
        public void Sqabs_S_B_H_S_D([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x5E207800; // SQABS B0, B0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQABS <Vd>.<T>, <Vn>.<T>")]
        public void Sqabs_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E207800; // SQABS V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQABS <Vd>.<T>, <Vn>.<T>")]
        public void Sqabs_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E207800; // SQABS V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQNEG <V><d>, <V><n>")]
        public void Sqneg_S_B_H_S_D([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x7E207800; // SQNEG B0, B0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQNEG <Vd>.<T>, <Vn>.<T>")]
        public void Sqneg_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E207800; // SQNEG V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQNEG <Vd>.<T>, <Vn>.<T>")]
        public void Sqneg_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E207800; // SQNEG V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQXTN <Vb><d>, <Va><n>")]
        public void Sqxtn_S_HB_SH_DS([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint Opcode = 0x5E214800; // SQXTN B0, H0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E214800; // SQXTN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E214800; // SQXTN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQXTUN <Vb><d>, <Va><n>")]
        public void Sqxtun_S_HB_SH_DS([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong A,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint Opcode = 0x7E212800; // SQXTUN B0, H0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQXTUN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtun_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E212800; // SQXTUN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SQXTUN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtun_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E212800; // SQXTUN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SUQADD <V><d>, <V><n>")]
        public void Suqadd_S_B_H_S_D([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x5E203800; // SUQADD B0, B0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SUQADD <Vd>.<T>, <Vn>.<T>")]
        public void Suqadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E203800; // SUQADD V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("SUQADD <Vd>.<T>, <Vn>.<T>")]
        public void Suqadd_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                          [Values(1u, 0u)] uint Rn,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                          [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E203800; // SUQADD V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("UADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uadalp_V_8B4H_4H2S_2S1D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint Opcode = 0x2E206800; // UADALP V0.4H, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uadalp_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x6E206800; // UADALP V0.8H, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uaddlp_V_8B4H_4H2S_2S1D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint Opcode = 0x2E202800; // UADDLP V0.4H, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uaddlp_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x6E202800; // UADDLP V0.8H, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UQXTN <Vb><d>, <Va><n>")]
        public void Uqxtn_S_HB_SH_DS([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint Opcode = 0x7E214800; // UQXTN B0, H0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("UQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Uqxtn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E214800; // UQXTN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("UQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Uqxtn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E214800; // UQXTN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("USQADD <V><d>, <V><n>")]
        public void Usqadd_S_B_H_S_D([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x7E203800; // USQADD B0, B0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("USQADD <Vd>.<T>, <Vn>.<T>")]
        public void Usqadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E203800; // USQADD V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("USQADD <Vd>.<T>, <Vn>.<T>")]
        public void Usqadd_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                          [Values(1u, 0u)] uint Rn,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                          [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E203800; // USQADD V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn(FpsrMask: FPSR.QC);
        }

        [Test, Pairwise, Description("XTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Xtn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E212800; // XTN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("XTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Xtn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                          [Values(1u, 0u)] uint Rn,
                                          [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                          [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                          [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E212800; // XTN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }
#endif
    }
}
