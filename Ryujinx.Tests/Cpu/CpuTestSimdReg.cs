#define SimdReg

using NUnit.Framework;

using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdReg")]
    public sealed class CpuTestSimdReg : CpuTest
    {
#if SimdReg

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

        private static ulong[] _1H1S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x0000000000007FFFul,
                                 0x0000000000008000ul, 0x000000000000FFFFul,
                                 0x000000007FFFFFFFul, 0x0000000080000000ul,
                                 0x00000000FFFFFFFFul };
        }

        private static ulong[] _4H2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
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

            for (int cnt = 1; cnt <= RndCnt; cnt++)
            {
                ulong grbg = TestContext.CurrentContext.Random.NextUInt();
                ulong rnd1 = GenNormalS();
                ulong rnd2 = GenSubnormalS();

                yield return (grbg << 32) | rnd1;
                yield return (grbg << 32) | rnd2;
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

            for (int cnt = 1; cnt <= RndCnt; cnt++)
            {
                ulong rnd1 = GenNormalS();
                ulong rnd2 = GenSubnormalS();

                yield return (rnd1 << 32) | rnd1;
                yield return (rnd2 << 32) | rnd2;
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

            for (int cnt = 1; cnt <= RndCnt; cnt++)
            {
                ulong rnd1 = GenNormalD();
                ulong rnd2 = GenSubnormalD();

                yield return rnd1;
                yield return rnd2;
            }
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _F_Add_Div_Mul_Mulx_Sub_S_S_()
        {
            return new uint[]
            {
                0x1E222820u, // FADD  S0, S1, S2
                0x1E221820u, // FDIV  S0, S1, S2
                0x1E220820u, // FMUL  S0, S1, S2
                0x5E22DC20u, // FMULX S0, S1, S2
                0x1E223820u  // FSUB  S0, S1, S2
            };
        }

        private static uint[] _F_Add_Div_Mul_Mulx_Sub_S_D_()
        {
            return new uint[]
            {
                0x1E622820u, // FADD  D0, D1, D2
                0x1E621820u, // FDIV  D0, D1, D2
                0x1E620820u, // FMUL  D0, D1, D2
                0x5E62DC20u, // FMULX D0, D1, D2
                0x1E623820u  // FSUB  D0, D1, D2
            };
        }

        private static uint[] _F_Add_Div_Mul_Mulx_Sub_V_2S_4S_()
        {
            return new uint[]
            {
                0x0E20D400u, // FADD  V0.2S, V0.2S, V0.2S
                0x2E20FC00u, // FDIV  V0.2S, V0.2S, V0.2S
                0x2E20DC00u, // FMUL  V0.2S, V0.2S, V0.2S
                0x0E20DC00u, // FMULX V0.2S, V0.2S, V0.2S
                0x0EA0D400u  // FSUB  V0.2S, V0.2S, V0.2S
            };
        }

        private static uint[] _F_Add_Div_Mul_Mulx_Sub_V_2D_()
        {
            return new uint[]
            {
                0x4E60D400u, // FADD  V0.2D, V0.2D, V0.2D
                0x6E60FC00u, // FDIV  V0.2D, V0.2D, V0.2D
                0x6E60DC00u, // FMUL  V0.2D, V0.2D, V0.2D
                0x4E60DC00u, // FMULX V0.2D, V0.2D, V0.2D
                0x4EE0D400u  // FSUB  V0.2D, V0.2D, V0.2D
            };
        }

        private static uint[] _F_Cmp_Cmpe_S_S_()
        {
            return new uint[]
            {
                0x1E222020u, // FCMP  S1, S2
                0x1E222030u  // FCMPE S1, S2
            };
        }

        private static uint[] _F_Cmp_Cmpe_S_D_()
        {
            return new uint[]
            {
                0x1E622020u, // FCMP  D1, D2
                0x1E622030u  // FCMPE D1, D2
            };
        }

        private static uint[] _F_Madd_Msub_S_S_()
        {
            return new uint[]
            {
                0x1F020C20u, // FMADD S0, S1, S2, S3
                0x1F028C20u  // FMSUB S0, S1, S2, S3
            };
        }

        private static uint[] _F_Madd_Msub_S_D_()
        {
            return new uint[]
            {
                0x1F420C20u, // FMADD D0, D1, D2, D3
                0x1F428C20u  // FMSUB D0, D1, D2, D3
            };
        }

        private static uint[] _F_Max_Min_Nm_S_S_()
        {
            return new uint[]
            {
                0x1E224820u, // FMAX   S0, S1, S2
                0x1E226820u, // FMAXNM S0, S1, S2
                0x1E225820u, // FMIN   S0, S1, S2
                0x1E227820u  // FMINNM S0, S1, S2
            };
        }

        private static uint[] _F_Max_Min_Nm_S_D_()
        {
            return new uint[]
            {
                0x1E624820u, // FMAX   D0, D1, D2
                0x1E626820u, // FMAXNM D0, D1, D2
                0x1E625820u, // FMIN   D0, D1, D2
                0x1E627820u  // FMINNM D0, D1, D2
            };
        }

        private static uint[] _F_Max_Min_Nm_P_V_2S_4S_()
        {
            return new uint[]
            {
                0x0E20F400u, // FMAX   V0.2S, V0.2S, V0.2S
                0x0E20C400u, // FMAXNM V0.2S, V0.2S, V0.2S
                0x2E20F400u, // FMAXP  V0.2S, V0.2S, V0.2S
                0x0EA0F400u, // FMIN   V0.2S, V0.2S, V0.2S
                0x0EA0C400u, // FMINNM V0.2S, V0.2S, V0.2S
                0x2EA0F400u  // FMINP  V0.2S, V0.2S, V0.2S
            };
        }

        private static uint[] _F_Max_Min_Nm_P_V_2D_()
        {
            return new uint[]
            {
                0x4E60F400u, // FMAX   V0.2D, V0.2D, V0.2D
                0x4E60C400u, // FMAXNM V0.2D, V0.2D, V0.2D
                0x6E60F400u, // FMAXP  V0.2D, V0.2D, V0.2D
                0x4EE0F400u, // FMIN   V0.2D, V0.2D, V0.2D
                0x4EE0C400u, // FMINNM V0.2D, V0.2D, V0.2D
                0x6EE0F400u  // FMINP  V0.2D, V0.2D, V0.2D
            };
        }

        private static uint[] _F_Mla_Mls_V_2S_4S_()
        {
            return new uint[]
            {
                0x0E20CC00u, // FMLA V0.2S, V0.2S, V0.2S
                0x0EA0CC00u  // FMLS V0.2S, V0.2S, V0.2S
            };
        }

        private static uint[] _F_Mla_Mls_V_2D_()
        {
            return new uint[]
            {
                0x4E60CC00u, // FMLA V0.2D, V0.2D, V0.2D
                0x4EE0CC00u  // FMLS V0.2D, V0.2D, V0.2D
            };
        }

        private static uint[] _F_Recps_Rsqrts_S_S_()
        {
            return new uint[]
            {
                0x5E22FC20u, // FRECPS  S0, S1, S2
                0x5EA2FC20u  // FRSQRTS S0, S1, S2
            };
        }

        private static uint[] _F_Recps_Rsqrts_S_D_()
        {
            return new uint[]
            {
                0x5E62FC20u, // FRECPS  D0, D1, D2
                0x5EE2FC20u  // FRSQRTS D0, D1, D2
            };
        }

        private static uint[] _F_Recps_Rsqrts_V_2S_4S_()
        {
            return new uint[]
            {
                0x0E20FC00u, // FRECPS  V0.2S, V0.2S, V0.2S
                0x0EA0FC00u  // FRSQRTS V0.2S, V0.2S, V0.2S
            };
        }

        private static uint[] _F_Recps_Rsqrts_V_2D_()
        {
            return new uint[]
            {
                0x4E60FC00u, // FRECPS  V0.2D, V0.2D, V0.2D
                0x4EE0FC00u  // FRSQRTS V0.2D, V0.2D, V0.2D
            };
        }

        private static uint[] _Sha1c_Sha1m_Sha1p_Sha1su0_V_()
        {
            return new uint[]
            {
                0x5E000000u, // SHA1C   Q0,    S0,    V0.4S
                0x5E002000u, // SHA1M   Q0,    S0,    V0.4S
                0x5E001000u, // SHA1P   Q0,    S0,    V0.4S
                0x5E003000u  // SHA1SU0 V0.4S, V0.4S, V0.4S
            };
        }

        private static uint[] _Sha256h_Sha256h2_Sha256su1_V_()
        {
            return new uint[]
            {
                0x5E004000u, // SHA256H   Q0,    Q0,    V0.4S
                0x5E005000u, // SHA256H2  Q0,    Q0,    V0.4S
                0x5E006000u  // SHA256SU1 V0.4S, V0.4S, V0.4S
            };
        }

        private static uint[] _S_Max_Min_P_V_()
        {
            return new uint[]
            {
                0x0E206400u, // SMAX  V0.8B, V0.8B, V0.8B
                0x0E20A400u, // SMAXP V0.8B, V0.8B, V0.8B
                0x0E206C00u, // SMIN  V0.8B, V0.8B, V0.8B
                0x0E20AC00u  // SMINP V0.8B, V0.8B, V0.8B
            };
        }

        private static uint[] _U_Max_Min_P_V_()
        {
            return new uint[]
            {
                0x2E206400u, // UMAX  V0.8B, V0.8B, V0.8B
                0x2E20A400u, // UMAXP V0.8B, V0.8B, V0.8B
                0x2E206C00u, // UMIN  V0.8B, V0.8B, V0.8B
                0x2E20AC00u  // UMINP V0.8B, V0.8B, V0.8B
            };
        }
#endregion

        private const int RndCnt = 2;

        private static readonly bool NoZeros = false;
        private static readonly bool NoInfs  = false;
        private static readonly bool NoNaNs  = false;

        [Test, Pairwise, Description("ADD <V><d>, <V><n>, <V><m>")]
        public void Add_S_D([Values(0u)]     uint rd,
                            [Values(1u, 0u)] uint rn,
                            [Values(2u, 0u)] uint rm,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x5EE08400; // ADD D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Add_V_8B_4H_2S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [Values(2u, 0u)] uint rm,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E208400; // ADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Add_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [Values(2u, 0u)] uint rm,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E208400; // ADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Addhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x0E204000; // ADDHN V0.8B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Addhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x4E204000; // ADDHN2 V0.16B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDP <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Addp_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E20BC00; // ADDP V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDP <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Addp_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E20BC00; // ADDP V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void And_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x0E201C00; // AND V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void And_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x4E201C00; // AND V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIC <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bic_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x0E601C00; // BIC V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIC <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bic_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x4E601C00; // BIC V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIF <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bif_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x2EE01C00; // BIF V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIF <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bif_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x6EE01C00; // BIF V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bit_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x2EA01C00; // BIT V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bit_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x6EA01C00; // BIT V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BSL <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bsl_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x2E601C00; // BSL V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BSL <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bsl_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x6E601C00; // BSL V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <V><d>, <V><n>, <V><m>")]
        public void Cmeq_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x7EE08C00; // CMEQ D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmeq_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E208C00; // CMEQ V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmeq_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E208C00; // CMEQ V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <V><d>, <V><n>, <V><m>")]
        public void Cmge_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x5EE03C00; // CMGE D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmge_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E203C00; // CMGE V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmge_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E203C00; // CMGE V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <V><d>, <V><n>, <V><m>")]
        public void Cmgt_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x5EE03400; // CMGT D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmgt_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E203400; // CMGT V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmgt_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E203400; // CMGT V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMHI <V><d>, <V><n>, <V><m>")]
        public void Cmhi_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x7EE03400; // CMHI D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMHI <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhi_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E203400; // CMHI V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMHI <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhi_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E203400; // CMHI V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMHS <V><d>, <V><n>, <V><m>")]
        public void Cmhs_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x7EE03C00; // CMHS D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMHS <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhs_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E203C00; // CMHS V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMHS <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhs_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E203C00; // CMHS V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMTST <V><d>, <V><n>, <V><m>")]
        public void Cmtst_S_D([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x5EE08C00; // CMTST D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMTST <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmtst_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E208C00; // CMTST V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMTST <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmtst_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [Values(2u, 0u)] uint rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E208C00; // CMTST V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Eor_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x2E201C00; // EOR V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Eor_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x6E201C00; // EOR V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Add_Div_Mul_Mulx_Sub_S_S([ValueSource("_F_Add_Div_Mul_Mulx_Sub_S_S_")] uint opcodes,
                                               [ValueSource("_1S_F_")] ulong a,
                                               [ValueSource("_1S_F_")] ulong b)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Add_Div_Mul_Mulx_Sub_S_D([ValueSource("_F_Add_Div_Mul_Mulx_Sub_S_D_")] uint opcodes,
                                               [ValueSource("_1D_F_")] ulong a,
                                               [ValueSource("_1D_F_")] ulong b)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Add_Div_Mul_Mulx_Sub_V_2S_4S([ValueSource("_F_Add_Div_Mul_Mulx_Sub_V_2S_4S_")] uint opcodes,
                                                   [Values(0u)]     uint rd,
                                                   [Values(1u, 0u)] uint rn,
                                                   [Values(2u, 0u)] uint rm,
                                                   [ValueSource("_2S_F_")] ulong z,
                                                   [ValueSource("_2S_F_")] ulong a,
                                                   [ValueSource("_2S_F_")] ulong b,
                                                   [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);
            Vector128<float> v2 = MakeVectorE0E1(b, b * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Add_Div_Mul_Mulx_Sub_V_2D([ValueSource("_F_Add_Div_Mul_Mulx_Sub_V_2D_")] uint opcodes,
                                                [Values(0u)]     uint rd,
                                                [Values(1u, 0u)] uint rn,
                                                [Values(2u, 0u)] uint rm,
                                                [ValueSource("_1D_F_")] ulong z,
                                                [ValueSource("_1D_F_")] ulong a,
                                                [ValueSource("_1D_F_")] ulong b)
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cmp_Cmpe_S_S([ValueSource("_F_Cmp_Cmpe_S_S_")] uint opcodes,
                                   [ValueSource("_1S_F_")] ulong a,
                                   [ValueSource("_1S_F_")] ulong b)
        {
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            bool v = TestContext.CurrentContext.Random.NextBool();
            bool c = TestContext.CurrentContext.Random.NextBool();
            bool z = TestContext.CurrentContext.Random.NextBool();
            bool n = TestContext.CurrentContext.Random.NextBool();

            SingleOpcode(opcodes, v1: v1, v2: v2, overflow: v, carry: c, zero: z, negative: n);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cmp_Cmpe_S_D([ValueSource("_F_Cmp_Cmpe_S_D_")] uint opcodes,
                                   [ValueSource("_1D_F_")] ulong a,
                                   [ValueSource("_1D_F_")] ulong b)
        {
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            bool v = TestContext.CurrentContext.Random.NextBool();
            bool c = TestContext.CurrentContext.Random.NextBool();
            bool z = TestContext.CurrentContext.Random.NextBool();
            bool n = TestContext.CurrentContext.Random.NextBool();

            SingleOpcode(opcodes, v1: v1, v2: v2, overflow: v, carry: c, zero: z, negative: n);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Madd_Msub_S_S([ValueSource("_F_Madd_Msub_S_S_")] uint opcodes,
                                    [ValueSource("_1S_F_")] ulong a,
                                    [ValueSource("_1S_F_")] ulong b,
                                    [ValueSource("_1S_F_")] ulong c)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);
            Vector128<float> v3 = MakeVectorE0(c);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v3: v3, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsS);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Madd_Msub_S_D([ValueSource("_F_Madd_Msub_S_D_")] uint opcodes,
                                    [ValueSource("_1D_F_")] ulong a,
                                    [ValueSource("_1D_F_")] ulong b,
                                    [ValueSource("_1D_F_")] ulong c)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);
            Vector128<float> v3 = MakeVectorE0(c);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, v3: v3, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsD);
        }

        [Test, Pairwise] [Explicit]
        public void F_Max_Min_Nm_S_S([ValueSource("_F_Max_Min_Nm_S_S_")] uint opcodes,
                                     [ValueSource("_1S_F_")] ulong a,
                                     [ValueSource("_1S_F_")] ulong b)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Max_Min_Nm_S_D([ValueSource("_F_Max_Min_Nm_S_D_")] uint opcodes,
                                     [ValueSource("_1D_F_")] ulong a,
                                     [ValueSource("_1D_F_")] ulong b)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Max_Min_Nm_P_V_2S_4S([ValueSource("_F_Max_Min_Nm_P_V_2S_4S_")] uint opcodes,
                                           [Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_2S_F_")] ulong z,
                                           [ValueSource("_2S_F_")] ulong a,
                                           [ValueSource("_2S_F_")] ulong b,
                                           [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);
            Vector128<float> v2 = MakeVectorE0E1(b, b * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Max_Min_Nm_P_V_2D([ValueSource("_F_Max_Min_Nm_P_V_2D_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_1D_F_")] ulong z,
                                        [ValueSource("_1D_F_")] ulong a,
                                        [ValueSource("_1D_F_")] ulong b)
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Mla_Mls_V_2S_4S([ValueSource("_F_Mla_Mls_V_2S_4S_")] uint opcodes,
                                      [Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_2S_F_")] ulong z,
                                      [ValueSource("_2S_F_")] ulong a,
                                      [ValueSource("_2S_F_")] ulong b,
                                      [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);
            Vector128<float> v2 = MakeVectorE0E1(b, b * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsS);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Mla_Mls_V_2D([ValueSource("_F_Mla_Mls_V_2D_")] uint opcodes,
                                   [Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [Values(2u, 0u)] uint rm,
                                   [ValueSource("_1D_F_")] ulong z,
                                   [ValueSource("_1D_F_")] ulong a,
                                   [ValueSource("_1D_F_")] ulong b)
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsD);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Recps_Rsqrts_S_S([ValueSource("_F_Recps_Rsqrts_S_S_")] uint opcodes,
                                       [ValueSource("_1S_F_")] ulong a,
                                       [ValueSource("_1S_F_")] ulong b)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsS);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Recps_Rsqrts_S_D([ValueSource("_F_Recps_Rsqrts_S_D_")] uint opcodes,
                                       [ValueSource("_1D_F_")] ulong a,
                                       [ValueSource("_1D_F_")] ulong b)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsD);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Recps_Rsqrts_V_2S_4S([ValueSource("_F_Recps_Rsqrts_V_2S_4S_")] uint opcodes,
                                           [Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_2S_F_")] ulong z,
                                           [ValueSource("_2S_F_")] ulong a,
                                           [ValueSource("_2S_F_")] ulong b,
                                           [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);
            Vector128<float> v2 = MakeVectorE0E1(b, b * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsS);
        }

        [Test, Pairwise] [Explicit] // Fused.
        public void F_Recps_Rsqrts_V_2D([ValueSource("_F_Recps_Rsqrts_V_2D_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_1D_F_")] ulong z,
                                        [ValueSource("_1D_F_")] ulong a,
                                        [ValueSource("_1D_F_")] ulong b)
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2, fpcr: fpcr);

            CompareAgainstUnicorn(Fpsr.Ioc | Fpsr.Idc, FpSkips.IfUnderflow, FpTolerances.UpToOneUlpsD);
        }

        [Test, Pairwise, Description("ORN <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orn_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x0EE01C00; // ORN V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORN <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orn_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x4EE01C00; // ORN V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orr_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [Values(2u, 0u)] uint rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x0EA01C00; // ORR V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orr_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [Values(2u, 0u)] uint rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x4EA01C00; // ORR V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Raddhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x2E204000; // RADDHN V0.8B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Raddhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [Values(2u, 0u)] uint rm,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x6E204000; // RADDHN2 V0.16B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RSUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Rsubhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x2E206000; // RSUBHN V0.8B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RSUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Rsubhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [Values(2u, 0u)] uint rm,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x6E206000; // RSUBHN2 V0.16B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Saba_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E207C00; // SABA V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Saba_V_16B_8H_4S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E207C00; // SABA V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabal_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x0E205000; // SABAL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabal_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E205000; // SABAL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sabd_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E207400; // SABD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sabd_V_16B_8H_4S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E207400; // SABD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabdl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x0E207000; // SABDL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabdl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E207000; // SABDL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Saddl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x0E200000; // SADDL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Saddl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E200000; // SADDL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Saddw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint rd,
                                                 [Values(1u, 0u)] uint rn,
                                                 [Values(2u, 0u)] uint rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint opcode = 0x0E201000; // SADDW V0.8H, V0.8H, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Saddw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint rd,
                                                  [Values(1u, 0u)] uint rn,
                                                  [Values(2u, 0u)] uint rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint opcode = 0x4E201000; // SADDW2 V0.8H, V0.8H, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Sha1c_Sha1m_Sha1p_Sha1su0_V([ValueSource("_Sha1c_Sha1m_Sha1p_Sha1su0_V_")] uint opcodes,
                                                [Values(0u)]     uint rd,
                                                [Values(1u, 0u)] uint rn,
                                                [Values(2u, 0u)] uint rm,
                                                [Random(RndCnt / 2)] ulong z0, [Random(RndCnt / 2)] ulong z1,
                                                [Random(RndCnt / 2)] ulong a0, [Random(RndCnt / 2)] ulong a1,
                                                [Random(RndCnt / 2)] ulong b0, [Random(RndCnt / 2)] ulong b1)
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z0, z1);
            Vector128<float> v1 = MakeVectorE0E1(a0, a1);
            Vector128<float> v2 = MakeVectorE0E1(b0, b1);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Sha256h_Sha256h2_Sha256su1_V([ValueSource("_Sha256h_Sha256h2_Sha256su1_V_")] uint opcodes,
                                                 [Values(0u)]     uint rd,
                                                 [Values(1u, 0u)] uint rn,
                                                 [Values(2u, 0u)] uint rm,
                                                 [Random(RndCnt / 2)] ulong z0, [Random(RndCnt / 2)] ulong z1,
                                                 [Random(RndCnt / 2)] ulong a0, [Random(RndCnt / 2)] ulong a1,
                                                 [Random(RndCnt / 2)] ulong b0, [Random(RndCnt / 2)] ulong b1)
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z0, z1);
            Vector128<float> v1 = MakeVectorE0E1(a0, a1);
            Vector128<float> v2 = MakeVectorE0E1(b0, b1);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E200400; // SHADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shadd_V_16B_8H_4S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E200400; // SHADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shsub_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E202400; // SHSUB V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shsub_V_16B_8H_4S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E202400; // SHSUB V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void S_Max_Min_P_V([ValueSource("_S_Max_Min_P_V_")] uint opcodes,
                                  [Values(0u)]     uint rd,
                                  [Values(1u, 0u)] uint rn,
                                  [Values(2u, 0u)] uint rm,
                                  [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                  [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                  [Values(0b00u, 0b01u, 0b10u)] uint size, // Q0: <8B,  4H, 2S>
                                  [Values(0b0u, 0b1u)] uint q)             // Q1: <16B, 8H, 4S>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((size & 3) << 22);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);
            Vector128<float> v2 = MakeVectorE0E1(b, b * q);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMLAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Smlal_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x0E208000; // SMLAL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMLAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Smlal_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E208000; // SMLAL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMLSL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Smlsl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x0E20A000; // SMLSL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMLSL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Smlsl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E20A000; // SMLSL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SQADD <V><d>, <V><n>, <V><m>")]
        public void Sqadd_S_B_H_S_D([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <b, H, S, D>
        {
            uint opcode = 0x5E200C00; // SQADD B0, B0, B0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E200C00; // SQADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqadd_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [Values(2u, 0u)] uint rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E200C00; // SQADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQDMULH <V><d>, <V><n>, <V><m>")]
        public void Sqdmulh_S_H_S([Values(0u)]     uint rd,
                                  [Values(1u, 0u)] uint rn,
                                  [Values(2u, 0u)] uint rm,
                                  [ValueSource("_1H1S_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_1H1S_")] [Random(RndCnt)] ulong a,
                                  [ValueSource("_1H1S_")] [Random(RndCnt)] ulong b,
                                  [Values(0b01u, 0b10u)] uint size) // <H, S>
        {
            uint opcode = 0x5E20B400; // SQDMULH B0, B0, B0 (RESERVED)
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqdmulh_V_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b01u, 0b10u)] uint size) // <4H, 2S>
        {
            uint opcode = 0x0E20B400; // SQDMULH V0.8B, V0.8B, V0.8B (RESERVED)
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqdmulh_V_8H_4S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b01u, 0b10u)] uint size) // <8H, 4S>
        {
            uint opcode = 0x4E20B400; // SQDMULH V0.16B, V0.16B, V0.16B (RESERVED)
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQRDMULH <V><d>, <V><n>, <V><m>")]
        public void Sqrdmulh_S_H_S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [Values(2u, 0u)] uint rm,
                                   [ValueSource("_1H1S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_1H1S_")] [Random(RndCnt)] ulong a,
                                   [ValueSource("_1H1S_")] [Random(RndCnt)] ulong b,
                                   [Values(0b01u, 0b10u)] uint size) // <H, S>
        {
            uint opcode = 0x7E20B400; // SQRDMULH B0, B0, B0 (RESERVED)
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQRDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqrdmulh_V_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b01u, 0b10u)] uint size) // <4H, 2S>
        {
            uint opcode = 0x2E20B400; // SQRDMULH V0.8B, V0.8B, V0.8B (RESERVED)
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQRDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqrdmulh_V_8H_4S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b01u, 0b10u)] uint size) // <8H, 4S>
        {
            uint opcode = 0x6E20B400; // SQRDMULH V0.16B, V0.16B, V0.16B (RESERVED)
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQSUB <V><d>, <V><n>, <V><m>")]
        public void Sqsub_S_B_H_S_D([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <b, H, S, D>
        {
            uint opcode = 0x5E202C00; // SQSUB B0, B0, B0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqsub_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E202C00; // SQSUB V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqsub_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [Values(2u, 0u)] uint rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E202C00; // SQSUB V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SRHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Srhadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E201400; // SRHADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SRHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Srhadd_V_16B_8H_4S([Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [Values(2u, 0u)] uint rm,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                       [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E201400; // SRHADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SSUBL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Ssubl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x0E202000; // SSUBL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SSUBL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Ssubl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E202000; // SSUBL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SSUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Ssubw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint rd,
                                                 [Values(1u, 0u)] uint rn,
                                                 [Values(2u, 0u)] uint rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint opcode = 0x0E203000; // SSUBW V0.8H, V0.8H, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SSUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Ssubw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint rd,
                                                  [Values(1u, 0u)] uint rn,
                                                  [Values(2u, 0u)] uint rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint opcode = 0x4E203000; // SSUBW2 V0.8H, V0.8H, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <V><d>, <V><n>, <V><m>")]
        public void Sub_S_D([Values(0u)]     uint rd,
                            [Values(1u, 0u)] uint rn,
                            [Values(2u, 0u)] uint rm,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong b)
        {
            uint opcode = 0x7EE08400; // SUB D0, D0, D0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sub_V_8B_4H_2S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [Values(2u, 0u)] uint rm,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E208400; // SUB V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sub_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [Values(2u, 0u)] uint rm,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E208400; // SUB V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Subhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x0E206000; // SUBHN V0.8B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Subhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x4E206000; // SUBHN2 V0.16B, V0.8H, V0.8H
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TRN1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn1_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E002800; // TRN1 V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TRN1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn1_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E002800; // TRN1 V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TRN2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn2_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E006800; // TRN2 V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("TRN2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn2_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E006800; // TRN2 V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uaba_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E207C00; // UABA V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uaba_V_16B_8H_4S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x6E207C00; // UABA V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabal_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x2E205000; // UABAL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabal_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E205000; // UABAL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uabd_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E207400; // UABD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uabd_V_16B_8H_4S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x6E207400; // UABD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabdl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x2E207000; // UABDL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabdl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E207000; // UABDL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uaddl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x2E200000; // UADDL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uaddl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E200000; // UADDL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Uaddw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint rd,
                                                 [Values(1u, 0u)] uint rn,
                                                 [Values(2u, 0u)] uint rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint opcode = 0x2E201000; // UADDW V0.8H, V0.8H, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Uaddw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint rd,
                                                  [Values(1u, 0u)] uint rn,
                                                  [Values(2u, 0u)] uint rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint opcode = 0x6E201000; // UADDW2 V0.8H, V0.8H, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E200400; // UHADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhadd_V_16B_8H_4S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x6E200400; // UHADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhsub_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E202400; // UHSUB V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhsub_V_16B_8H_4S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x6E202400; // UHSUB V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void U_Max_Min_P_V([ValueSource("_U_Max_Min_P_V_")] uint opcodes,
                                  [Values(0u)]     uint rd,
                                  [Values(1u, 0u)] uint rn,
                                  [Values(2u, 0u)] uint rm,
                                  [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                  [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                  [Values(0b00u, 0b01u, 0b10u)] uint size, // Q0: <8B,  4H, 2S>
                                  [Values(0b0u, 0b1u)] uint q)             // Q1: <16B, 8H, 4S>
        {
            opcodes |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((size & 3) << 22);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);
            Vector128<float> v2 = MakeVectorE0E1(b, b * q);

            SingleOpcode(opcodes, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMLAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Umlal_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x2E208000; // UMLAL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMLAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Umlal_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E208000; // UMLAL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMLSL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Umlsl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x2E20A000; // UMLSL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMLSL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Umlsl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E20A000; // UMLSL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UQADD <V><d>, <V><n>, <V><m>")]
        public void Uqadd_S_B_H_S_D([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <b, H, S, D>
        {
            uint opcode = 0x7E200C00; // UQADD B0, B0, B0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E200C00; // UQADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqadd_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [Values(2u, 0u)] uint rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E200C00; // UQADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQSUB <V><d>, <V><n>, <V><m>")]
        public void Uqsub_S_B_H_S_D([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <b, H, S, D>
        {
            uint opcode = 0x7E202C00; // UQSUB B0, B0, B0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqsub_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [Values(2u, 0u)] uint rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E202C00; // UQSUB V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqsub_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [Values(2u, 0u)] uint rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E202C00; // UQSUB V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("URHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Urhadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [Values(2u, 0u)] uint rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E201400; // URHADD V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("URHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Urhadd_V_16B_8H_4S([Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [Values(2u, 0u)] uint rm,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                       [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x6E201400; // URHADD V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("USUBL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Usubl_V_8B8H_4H4S_2S2D([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [Values(2u, 0u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint opcode = 0x2E202000; // USUBL V0.8H, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("USUBL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Usubl_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [Values(2u, 0u)] uint rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E202000; // USUBL2 V0.8H, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE1(a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("USUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Usubw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint rd,
                                                 [Values(1u, 0u)] uint rn,
                                                 [Values(2u, 0u)] uint rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint opcode = 0x2E203000; // USUBW V0.8H, V0.8H, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("USUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Usubw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint rd,
                                                  [Values(1u, 0u)] uint rn,
                                                  [Values(2u, 0u)] uint rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong a,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong b,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint opcode = 0x6E203000; // USUBW2 V0.8H, V0.8H, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE1(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UZP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp1_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E001800; // UZP1 V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UZP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp1_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E001800; // UZP1 V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UZP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp2_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E005800; // UZP2 V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UZP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp2_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E005800; // UZP2 V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ZIP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip1_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E003800; // ZIP1 V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ZIP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip1_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E003800; // ZIP1 V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ZIP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip2_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Values(2u, 0u)] uint rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E007800; // ZIP2 V0.8B, V0.8B, V0.8B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);
            Vector128<float> v2 = MakeVectorE0(b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ZIP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip2_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [Values(2u, 0u)] uint rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong b,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E007800; // ZIP2 V0.16B, V0.16B, V0.16B
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);
            Vector128<float> v2 = MakeVectorE0E1(b, b);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }
#endif
    }
}
