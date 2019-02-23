#define SimdCvt

using NUnit.Framework;

using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdCvt")]
    public sealed class CpuTestSimdCvt : CpuTest
    {
#if SimdCvt

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

            for (int cnt = 1; cnt <= RndCnt; cnt++)
            {
                ulong grbg = TestContext.CurrentContext.Random.NextUInt();
                ulong rnd1 = GenNormalS();
                ulong rnd2 = GenSubnormalS();

                yield return (grbg << 32) | rnd1;
                yield return (grbg << 32) | rnd2;
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

        private static uint[] _W_()
        {
            return new uint[] { 0x00000000u, 0x7FFFFFFFu,
                                0x80000000u, 0xFFFFFFFFu };
        }

        private static ulong[] _X_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _F_Cvt_Z_SU_S_SW_()
        {
            return new uint[]
            {
                0x1E188000u, // FCVTZS W0, S0, #32
                0x1E198000u  // FCVTZU W0, S0, #32
            };
        }

        private static uint[] _F_Cvt_Z_SU_S_SX_()
        {
            return new uint[]
            {
                0x9E180000u, // FCVTZS X0, S0, #64
                0x9E190000u  // FCVTZU X0, S0, #64
            };
        }

        private static uint[] _F_Cvt_Z_SU_S_DW_()
        {
            return new uint[]
            {
                0x1E588000u, // FCVTZS W0, D0, #32
                0x1E598000u  // FCVTZU W0, D0, #32
            };
        }

        private static uint[] _F_Cvt_Z_SU_S_DX_()
        {
            return new uint[]
            {
                0x9E580000u, // FCVTZS X0, D0, #64
                0x9E590000u  // FCVTZU X0, D0, #64
            };
        }

        private static uint[] _SU_Cvt_F_S_WS_()
        {
            return new uint[]
            {
                0x1E028000u, // SCVTF S0, W0, #32
                0x1E038000u  // UCVTF S0, W0, #32
            };
        }

        private static uint[] _SU_Cvt_F_S_WD_()
        {
            return new uint[]
            {
                0x1E428000u, // SCVTF D0, W0, #32
                0x1E438000u  // UCVTF D0, W0, #32
            };
        }

        private static uint[] _SU_Cvt_F_S_XS_()
        {
            return new uint[]
            {
                0x9E020000u, // SCVTF S0, X0, #64
                0x9E030000u  // UCVTF S0, X0, #64
            };
        }

        private static uint[] _SU_Cvt_F_S_XD_()
        {
            return new uint[]
            {
                0x9E420000u, // SCVTF D0, X0, #64
                0x9E430000u  // UCVTF D0, X0, #64
            };
        }
#endregion

        private const int RndCnt      = 2;
        private const int RndCntFbits = 2;

        private static readonly bool NoZeros = false;
        private static readonly bool NoInfs  = false;
        private static readonly bool NoNaNs  = false;

        [Test, Pairwise] [Explicit]
        public void F_Cvt_Z_SU_S_SW([ValueSource("_F_Cvt_Z_SU_S_SW_")] uint opcodes,
                                    [Values(0u, 31u)] uint rd,
                                    [Values(1u)]      uint rn,
                                    [ValueSource("_1S_F_")] ulong a,
                                    [Values(1u, 32u)] [Random(2u, 31u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            ulong x0 = (ulong)TestContext.CurrentContext.Random.NextUInt() << 32;
            uint w31 = TestContext.CurrentContext.Random.NextUInt();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, x0: x0, x31: w31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_Z_SU_S_SX([ValueSource("_F_Cvt_Z_SU_S_SX_")] uint opcodes,
                                    [Values(0u, 31u)] uint rd,
                                    [Values(1u)]      uint rn,
                                    [ValueSource("_1S_F_")] ulong a,
                                    [Values(1u, 64u)] [Random(2u, 63u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, x31: x31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_Z_SU_S_DW([ValueSource("_F_Cvt_Z_SU_S_DW_")] uint opcodes,
                                    [Values(0u, 31u)] uint rd,
                                    [Values(1u)]      uint rn,
                                    [ValueSource("_1D_F_")] ulong a,
                                    [Values(1u, 32u)] [Random(2u, 31u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            ulong x0 = (ulong)TestContext.CurrentContext.Random.NextUInt() << 32;
            uint w31 = TestContext.CurrentContext.Random.NextUInt();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, x0: x0, x31: w31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_Z_SU_S_DX([ValueSource("_F_Cvt_Z_SU_S_DX_")] uint opcodes,
                                    [Values(0u, 31u)] uint rd,
                                    [Values(1u)]      uint rn,
                                    [ValueSource("_1D_F_")] ulong a,
                                    [Values(1u, 64u)] [Random(2u, 63u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, x31: x31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_S_WS([ValueSource("_SU_Cvt_F_S_WS_")] uint opcodes,
                                  [Values(0u)]      uint rd,
                                  [Values(1u, 31u)] uint rn,
                                  [ValueSource("_W_")] [Random(RndCnt)] uint wn,
                                  [Values(1u, 32u)] [Random(2u, 31u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            uint  w31 = TestContext.CurrentContext.Random.NextUInt();
            ulong z   = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);

            SingleOpcode(opcodes, x1: wn, x31: w31, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_S_WD([ValueSource("_SU_Cvt_F_S_WD_")] uint opcodes,
                                  [Values(0u)]      uint rd,
                                  [Values(1u, 31u)] uint rn,
                                  [ValueSource("_W_")] [Random(RndCnt)] uint wn,
                                  [Values(1u, 32u)] [Random(2u, 31u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            uint  w31 = TestContext.CurrentContext.Random.NextUInt();
            ulong z   = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);

            SingleOpcode(opcodes, x1: wn, x31: w31, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_S_XS([ValueSource("_SU_Cvt_F_S_XS_")] uint opcodes,
                                  [Values(0u)]      uint rd,
                                  [Values(1u, 31u)] uint rn,
                                  [ValueSource("_X_")] [Random(RndCnt)] ulong xn,
                                  [Values(1u, 64u)] [Random(2u, 63u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();
            ulong z   = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);

            SingleOpcode(opcodes, x1: xn, x31: x31, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_S_XD([ValueSource("_SU_Cvt_F_S_XD_")] uint opcodes,
                                  [Values(0u)]      uint rd,
                                  [Values(1u, 31u)] uint rn,
                                  [ValueSource("_X_")] [Random(RndCnt)] ulong xn,
                                  [Values(1u, 64u)] [Random(2u, 63u, RndCntFbits)] uint fbits)
        {
            uint scale = (64u - fbits) & 0x3Fu;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (scale << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();
            ulong z   = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);

            SingleOpcode(opcodes, x1: xn, x31: x31, v0: v0);

            CompareAgainstUnicorn();
        }
#endif
    }
}
