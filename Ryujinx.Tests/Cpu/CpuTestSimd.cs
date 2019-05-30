#define Simd

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("Simd")]
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

        private static ulong[] _1S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x000000007FFFFFFFul,
                                 0x0000000080000000ul, 0x00000000FFFFFFFFul };
        }

        private static ulong[] _2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFF7FFFFFFFul,
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

            for (int cnt = 1; cnt <= RndCnt; cnt++)
            {
                uint rnd1 = (uint)GenNormalH();
                uint rnd2 = (uint)GenSubnormalH();

                yield return (rnd1 << 48) | (rnd1 << 32) | (rnd1 << 16) | rnd1;
                yield return (rnd2 << 48) | (rnd2 << 32) | (rnd2 << 16) | rnd2;
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

            for (int cnt = 1; cnt <= RndCnt; cnt++)
            {
                ulong grbg = TestContext.CurrentContext.Random.NextUInt();
                ulong rnd1 = GenNormalS();
                ulong rnd2 = GenSubnormalS();

                yield return (grbg << 32) | rnd1;
                yield return (grbg << 32) | rnd2;
            }
        }

        private static IEnumerable<ulong> _1S_F_W_()
        {
            // int
            yield return 0x00000000CF000001ul; // -2.1474839E9f  (-2147483904)
            yield return 0x00000000CF000000ul; // -2.14748365E9f (-2147483648)
            yield return 0x00000000CEFFFFFFul; // -2.14748352E9f (-2147483520)
            yield return 0x000000004F000001ul; //  2.1474839E9f  (2147483904)
            yield return 0x000000004F000000ul; //  2.14748365E9f (2147483648)
            yield return 0x000000004EFFFFFFul; //  2.14748352E9f (2147483520)

            // uint
            yield return 0x000000004F800001ul; // 4.2949678E9f  (4294967808)
            yield return 0x000000004F800000ul; // 4.2949673E9f  (4294967296)
            yield return 0x000000004F7FFFFFul; // 4.29496704E9f (4294967040)

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

                ulong rnd1 = (uint)BitConverter.SingleToInt32Bits(
                    (float)((int)TestContext.CurrentContext.Random.NextUInt()));
                ulong rnd2 = (uint)BitConverter.SingleToInt32Bits(
                    (float)((uint)TestContext.CurrentContext.Random.NextUInt()));

                ulong rnd3 = GenNormalS();
                ulong rnd4 = GenSubnormalS();

                yield return (grbg << 32) | rnd1;
                yield return (grbg << 32) | rnd2;

                yield return (grbg << 32) | rnd3;
                yield return (grbg << 32) | rnd4;
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

        private static IEnumerable<ulong> _2S_F_W_()
        {
            // int
            yield return 0xCF000001CF000001ul; // -2.1474839E9f  (-2147483904)
            yield return 0xCF000000CF000000ul; // -2.14748365E9f (-2147483648)
            yield return 0xCEFFFFFFCEFFFFFFul; // -2.14748352E9f (-2147483520)
            yield return 0x4F0000014F000001ul; //  2.1474839E9f  (2147483904)
            yield return 0x4F0000004F000000ul; //  2.14748365E9f (2147483648)
            yield return 0x4EFFFFFF4EFFFFFFul; //  2.14748352E9f (2147483520)

            // uint
            yield return 0x4F8000014F800001ul; // 4.2949678E9f  (4294967808)
            yield return 0x4F8000004F800000ul; // 4.2949673E9f  (4294967296)
            yield return 0x4F7FFFFF4F7FFFFFul; // 4.29496704E9f (4294967040)

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
                ulong rnd1 = (uint)BitConverter.SingleToInt32Bits(
                    (float)((int)TestContext.CurrentContext.Random.NextUInt()));
                ulong rnd2 = (uint)BitConverter.SingleToInt32Bits(
                    (float)((uint)TestContext.CurrentContext.Random.NextUInt()));

                ulong rnd3 = GenNormalS();
                ulong rnd4 = GenSubnormalS();

                yield return (rnd1 << 32) | rnd1;
                yield return (rnd2 << 32) | rnd2;

                yield return (rnd3 << 32) | rnd3;
                yield return (rnd4 << 32) | rnd4;
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

        private static IEnumerable<ulong> _1D_F_X_()
        {
            // long
            yield return 0xC3E0000000000001ul; // -9.2233720368547780E18d (-9223372036854778000)
            yield return 0xC3E0000000000000ul; // -9.2233720368547760E18d (-9223372036854776000)
            yield return 0xC3DFFFFFFFFFFFFFul; // -9.2233720368547750E18d (-9223372036854775000)
            yield return 0x43E0000000000001ul; //  9.2233720368547780E18d (9223372036854778000)
            yield return 0x43E0000000000000ul; //  9.2233720368547760E18d (9223372036854776000)
            yield return 0x43DFFFFFFFFFFFFFul; //  9.2233720368547750E18d (9223372036854775000)

            // ulong
            yield return 0x43F0000000000001ul; // 1.8446744073709556e19d (18446744073709556000)
            yield return 0x43F0000000000000ul; // 1.8446744073709552E19d (18446744073709552000)
            yield return 0x43EFFFFFFFFFFFFFul; // 1.8446744073709550e19d (18446744073709550000)

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
                ulong rnd1 = (ulong)BitConverter.DoubleToInt64Bits(
                    (double)((long)TestContext.CurrentContext.Random.NextULong()));
                ulong rnd2 = (ulong)BitConverter.DoubleToInt64Bits(
                    (double)((ulong)TestContext.CurrentContext.Random.NextULong()));

                ulong rnd3 = GenNormalD();
                ulong rnd4 = GenSubnormalD();

                yield return rnd1;
                yield return rnd2;

                yield return rnd3;
                yield return rnd4;
            }
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _SU_Add_Max_Min_V_V_8BB_4HH_()
        {
            return new uint[]
            {
                0x0E31B800u, // ADDV  B0, V0.8B
                0x0E30A800u, // SMAXV B0, V0.8B
                0x0E31A800u, // SMINV B0, V0.8B
                0x2E30A800u, // UMAXV B0, V0.8B
                0x2E31A800u  // UMINV B0, V0.8B
            };
        }

        private static uint[] _SU_Add_Max_Min_V_V_16BB_8HH_4SS_()
        {
            return new uint[]
            {
                0x4E31B800u, // ADDV  B0, V0.16B
                0x4E30A800u, // SMAXV B0, V0.16B
                0x4E31A800u, // SMINV B0, V0.16B
                0x6E30A800u, // UMAXV B0, V0.16B
                0x6E31A800u  // UMINV B0, V0.16B
            };
        }

        private static uint[] _F_Abs_Neg_Recpx_Sqrt_S_S_()
        {
            return new uint[]
            {
                0x1E20C020u, // FABS   S0, S1
                0x1E214020u, // FNEG   S0, S1
                0x5EA1F820u, // FRECPX S0, S1
                0x1E21C020u  // FSQRT  S0, S1
            };
        }

        private static uint[] _F_Abs_Neg_Recpx_Sqrt_S_D_()
        {
            return new uint[]
            {
                0x1E60C020u, // FABS   D0, D1
                0x1E614020u, // FNEG   D0, D1
                0x5EE1F820u, // FRECPX D0, D1
                0x1E61C020u  // FSQRT  D0, D1
            };
        }

        private static uint[] _F_Abs_Neg_Sqrt_V_2S_4S_()
        {
            return new uint[]
            {
                0x0EA0F800u, // FABS  V0.2S, V0.2S
                0x2EA0F800u, // FNEG  V0.2S, V0.2S
                0x2EA1F800u  // FSQRT V0.2S, V0.2S
            };
        }

        private static uint[] _F_Abs_Neg_Sqrt_V_2D_()
        {
            return new uint[]
            {
                0x4EE0F800u, // FABS  V0.2D, V0.2D
                0x6EE0F800u, // FNEG  V0.2D, V0.2D
                0x6EE1F800u  // FSQRT V0.2D, V0.2D
            };
        }

        private static uint[] _F_Add_P_S_2SS_()
        {
            return new uint[]
            {
                0x7E30D820u // FADDP S0, V1.2S
            };
        }

        private static uint[] _F_Add_P_S_2DD_()
        {
            return new uint[]
            {
                0x7E70D820u // FADDP D0, V1.2D
            };
        }

        private static uint[] _F_Cm_EqGeGtLeLt_S_S_()
        {
            return new uint[]
            {
                0x5EA0D820u, // FCMEQ S0, S1, #0.0
                0x7EA0C820u, // FCMGE S0, S1, #0.0
                0x5EA0C820u, // FCMGT S0, S1, #0.0
                0x7EA0D820u, // FCMLE S0, S1, #0.0
                0x5EA0E820u  // FCMLT S0, S1, #0.0
            };
        }

        private static uint[] _F_Cm_EqGeGtLeLt_S_D_()
        {
            return new uint[]
            {
                0x5EE0D820u, // FCMEQ D0, D1, #0.0
                0x7EE0C820u, // FCMGE D0, D1, #0.0
                0x5EE0C820u, // FCMGT D0, D1, #0.0
                0x7EE0D820u, // FCMLE D0, D1, #0.0
                0x5EE0E820u  // FCMLT D0, D1, #0.0
            };
        }

        private static uint[] _F_Cm_EqGeGtLeLt_V_2S_4S_()
        {
            return new uint[]
            {
                0x0EA0D800u, // FCMEQ V0.2S, V0.2S, #0.0
                0x2EA0C800u, // FCMGE V0.2S, V0.2S, #0.0
                0x0EA0C800u, // FCMGT V0.2S, V0.2S, #0.0
                0x2EA0D800u, // FCMLE V0.2S, V0.2S, #0.0
                0x0EA0E800u  // FCMLT V0.2S, V0.2S, #0.0
            };
        }

        private static uint[] _F_Cm_EqGeGtLeLt_V_2D_()
        {
            return new uint[]
            {
                0x4EE0D800u, // FCMEQ V0.2D, V0.2D, #0.0
                0x6EE0C800u, // FCMGE V0.2D, V0.2D, #0.0
                0x4EE0C800u, // FCMGT V0.2D, V0.2D, #0.0
                0x6EE0D800u, // FCMLE V0.2D, V0.2D, #0.0
                0x4EE0E800u  // FCMLT V0.2D, V0.2D, #0.0
            };
        }

        private static uint[] _F_Cmp_Cmpe_S_S_()
        {
            return new uint[]
            {
                0x1E202028u, // FCMP  S1, #0.0
                0x1E202038u  // FCMPE S1, #0.0
            };
        }

        private static uint[] _F_Cmp_Cmpe_S_D_()
        {
            return new uint[]
            {
                0x1E602028u, // FCMP  D1, #0.0
                0x1E602038u  // FCMPE D1, #0.0
            };
        }

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

        private static uint[] _F_Recpe_Rsqrte_S_S_()
        {
            return new uint[]
            {
                0x5EA1D820u, // FRECPE  S0, S1
                0x7EA1D820u  // FRSQRTE S0, S1
            };
        }

        private static uint[] _F_Recpe_Rsqrte_S_D_()
        {
            return new uint[]
            {
                0x5EE1D820u, // FRECPE  D0, D1
                0x7EE1D820u  // FRSQRTE D0, D1
            };
        }

        private static uint[] _F_Recpe_Rsqrte_V_2S_4S_()
        {
            return new uint[]
            {
                0x0EA1D800u, // FRECPE  V0.2S, V0.2S
                0x2EA1D800u  // FRSQRTE V0.2S, V0.2S
            };
        }

        private static uint[] _F_Recpe_Rsqrte_V_2D_()
        {
            return new uint[]
            {
                0x4EE1D800u, // FRECPE  V0.2D, V0.2D
                0x6EE1D800u  // FRSQRTE V0.2D, V0.2D
            };
        }

        private static uint[] _F_Rint_AMNPZ_S_S_()
        {
            return new uint[]
            {
                0x1E264020u, // FRINTA S0, S1
                0x1E254020u, // FRINTM S0, S1
                0x1E244020u, // FRINTN S0, S1
                0x1E24C020u, // FRINTP S0, S1
                0x1E25C020u  // FRINTZ S0, S1
            };
        }

        private static uint[] _F_Rint_AMNPZ_S_D_()
        {
            return new uint[]
            {
                0x1E664020u, // FRINTA D0, D1
                0x1E654020u, // FRINTM D0, D1
                0x1E644020u, // FRINTN D0, D1
                0x1E64C020u, // FRINTP D0, D1
                0x1E65C020u  // FRINTZ D0, D1
            };
        }

        private static uint[] _F_Rint_AMNPZ_V_2S_4S_()
        {
            return new uint[]
            {
                0x2E218800u, // FRINTA V0.2S, V0.2S
                0x0E219800u, // FRINTM V0.2S, V0.2S
                0x0E218800u, // FRINTN V0.2S, V0.2S
                0x0EA18800u, // FRINTP V0.2S, V0.2S
                0x0EA19800u  // FRINTZ V0.2S, V0.2S
            };
        }

        private static uint[] _F_Rint_AMNPZ_V_2D_()
        {
            return new uint[]
            {
                0x6E618800u, // FRINTA V0.2D, V0.2D
                0x4E619800u, // FRINTM V0.2D, V0.2D
                0x4E618800u, // FRINTN V0.2D, V0.2D
                0x4EE18800u, // FRINTP V0.2D, V0.2D
                0x4EE19800u  // FRINTZ V0.2D, V0.2D
            };
        }

        private static uint[] _F_Rint_IX_S_S_()
        {
            return new uint[]
            {
                0x1E27C020u, // FRINTI S0, S1
                0x1E274020u  // FRINTX S0, S1
            };
        }

        private static uint[] _F_Rint_IX_S_D_()
        {
            return new uint[]
            {
                0x1E67C020u, // FRINTI D0, D1
                0x1E674020u  // FRINTX D0, D1
            };
        }

        private static uint[] _F_Rint_IX_V_2S_4S_()
        {
            return new uint[]
            {
                0x2EA19800u, // FRINTI V0.2S, V0.2S
                0x2E219800u  // FRINTX V0.2S, V0.2S
            };
        }

        private static uint[] _F_Rint_IX_V_2D_()
        {
            return new uint[]
            {
                0x6EE19800u, // FRINTI V0.2D, V0.2D
                0x6E619800u  // FRINTX V0.2D, V0.2D
            };
        }

        private static uint[] _SU_Cvt_F_S_S_()
        {
            return new uint[]
            {
                0x5E21D820u, // SCVTF S0, S1
                0x7E21D820u  // UCVTF S0, S1
            };
        }

        private static uint[] _SU_Cvt_F_S_D_()
        {
            return new uint[]
            {
                0x5E61D820u, // SCVTF D0, D1
                0x7E61D820u  // UCVTF D0, D1
            };
        }

        private static uint[] _SU_Cvt_F_V_2S_4S_()
        {
            return new uint[]
            {
                0x0E21D800u, // SCVTF V0.2S, V0.2S
                0x2E21D800u  // UCVTF V0.2S, V0.2S
            };
        }

        private static uint[] _SU_Cvt_F_V_2D_()
        {
            return new uint[]
            {
                0x4E61D800u, // SCVTF V0.2D, V0.2D
                0x6E61D800u  // UCVTF V0.2D, V0.2D
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
        public void Abs_S_D([Values(0u)]     uint rd,
                            [Values(1u, 0u)] uint rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x5EE0B800; // ABS D0, D0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_8B_4H_2S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E20B800; // ABS V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E20B800; // ABS V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDP <V><d>, <Vn>.<T>")]
        public void Addp_S_2DD([Values(0u)]     uint rd,
                               [Values(1u, 0u)] uint rn,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x5EF1B800; // ADDP D0, V0.2D
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void SU_Add_Max_Min_V_V_8BB_4HH([ValueSource("_SU_Add_Max_Min_V_V_8BB_4HH_")] uint opcodes,
                                               [Values(0u)]     uint rd,
                                               [Values(1u, 0u)] uint rn,
                                               [ValueSource("_8B4H_")] [Random(RndCnt)] ulong z,
                                               [ValueSource("_8B4H_")] [Random(RndCnt)] ulong a,
                                               [Values(0b00u, 0b01u)] uint size) // <8BB, 4HH>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void SU_Add_Max_Min_V_V_16BB_8HH_4SS([ValueSource("_SU_Add_Max_Min_V_V_16BB_8HH_4SS_")] uint opcodes,
                                                    [Values(0u)]     uint rd,
                                                    [Values(1u, 0u)] uint rn,
                                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16BB, 8HH, 4SS>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLS <Vd>.<T>, <Vn>.<T>")]
        public void Cls_V_8B_4H_2S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E204800; // CLS V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLS <Vd>.<T>, <Vn>.<T>")]
        public void Cls_V_16B_8H_4S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E204800; // CLS V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Vd>.<T>, <Vn>.<T>")]
        public void Clz_V_8B_4H_2S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E204800; // CLZ V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Vd>.<T>, <Vn>.<T>")]
        public void Clz_V_16B_8H_4S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x6E204800; // CLZ V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <V><d>, <V><n>, #0")]
        public void Cmeq_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x5EE09800; // CMEQ D0, D0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmeq_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E209800; // CMEQ V0.8B, V0.8B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmeq_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E209800; // CMEQ V0.16B, V0.16B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <V><d>, <V><n>, #0")]
        public void Cmge_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x7EE08800; // CMGE D0, D0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmge_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E208800; // CMGE V0.8B, V0.8B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmge_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E208800; // CMGE V0.16B, V0.16B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <V><d>, <V><n>, #0")]
        public void Cmgt_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x5EE08800; // CMGT D0, D0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmgt_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E208800; // CMGT V0.8B, V0.8B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmgt_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E208800; // CMGT V0.16B, V0.16B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLE <V><d>, <V><n>, #0")]
        public void Cmle_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x7EE09800; // CMLE D0, D0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmle_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E209800; // CMLE V0.8B, V0.8B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmle_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E209800; // CMLE V0.16B, V0.16B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLT <V><d>, <V><n>, #0")]
        public void Cmlt_S_D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x5EE0A800; // CMLT D0, D0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmlt_V_8B_4H_2S([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E20A800; // CMLT V0.8B, V0.8B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CMLT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmlt_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E20A800; // CMLT V0.16B, V0.16B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CNT <Vd>.<T>, <Vn>.<T>")]
        public void Cnt_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x0E205800; // CNT V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CNT <Vd>.<T>, <Vn>.<T>")]
        public void Cnt_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x4E205800; // CNT V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Abs_Neg_Recpx_Sqrt_S_S([ValueSource("_F_Abs_Neg_Recpx_Sqrt_S_S_")] uint opcodes,
                                             [ValueSource("_1S_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Abs_Neg_Recpx_Sqrt_S_D([ValueSource("_F_Abs_Neg_Recpx_Sqrt_S_D_")] uint opcodes,
                                             [ValueSource("_1D_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Abs_Neg_Sqrt_V_2S_4S([ValueSource("_F_Abs_Neg_Sqrt_V_2S_4S_")] uint opcodes,
                                           [Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [ValueSource("_2S_F_")] ulong z,
                                           [ValueSource("_2S_F_")] ulong a,
                                           [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Abs_Neg_Sqrt_V_2D([ValueSource("_F_Abs_Neg_Sqrt_V_2D_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_1D_F_")] ulong z,
                                        [ValueSource("_1D_F_")] ulong a)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Add_P_S_2SS([ValueSource("_F_Add_P_S_2SS_")] uint opcodes,
                                  [ValueSource("_2S_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Add_P_S_2DD([ValueSource("_F_Add_P_S_2DD_")] uint opcodes,
                                  [ValueSource("_1D_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cm_EqGeGtLeLt_S_S([ValueSource("_F_Cm_EqGeGtLeLt_S_S_")] uint opcodes,
                                        [ValueSource("_1S_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cm_EqGeGtLeLt_S_D([ValueSource("_F_Cm_EqGeGtLeLt_S_D_")] uint opcodes,
                                        [ValueSource("_1D_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cm_EqGeGtLeLt_V_2S_4S([ValueSource("_F_Cm_EqGeGtLeLt_V_2S_4S_")] uint opcodes,
                                            [Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_2S_F_")] ulong z,
                                            [ValueSource("_2S_F_")] ulong a,
                                            [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cm_EqGeGtLeLt_V_2D([ValueSource("_F_Cm_EqGeGtLeLt_V_2D_")] uint opcodes,
                                         [Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [ValueSource("_1D_F_")] ulong z,
                                         [ValueSource("_1D_F_")] ulong a)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = rnd & (1 << (int)Fpcr.Fz);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cmp_Cmpe_S_S([ValueSource("_F_Cmp_Cmpe_S_S_")] uint opcodes,
                                   [ValueSource("_1S_F_")] ulong a)
        {
            Vector128<float> v1 = MakeVectorE0(a);

            bool v = TestContext.CurrentContext.Random.NextBool();
            bool c = TestContext.CurrentContext.Random.NextBool();
            bool z = TestContext.CurrentContext.Random.NextBool();
            bool n = TestContext.CurrentContext.Random.NextBool();

            SingleOpcode(opcodes, v1: v1, overflow: v, carry: c, zero: z, negative: n);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cmp_Cmpe_S_D([ValueSource("_F_Cmp_Cmpe_S_D_")] uint opcodes,
                                   [ValueSource("_1D_F_")] ulong a)
        {
            Vector128<float> v1 = MakeVectorE0(a);

            bool v = TestContext.CurrentContext.Random.NextBool();
            bool c = TestContext.CurrentContext.Random.NextBool();
            bool z = TestContext.CurrentContext.Random.NextBool();
            bool n = TestContext.CurrentContext.Random.NextBool();

            SingleOpcode(opcodes, v1: v1, overflow: v, carry: c, zero: z, negative: n);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_S_SD([ValueSource("_F_Cvt_S_SD_")] uint opcodes,
                               [ValueSource("_1S_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_S_DS([ValueSource("_F_Cvt_S_DS_")] uint opcodes,
                               [ValueSource("_1D_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_S_S([ValueSource("_F_Cvt_NZ_SU_S_S_")] uint opcodes,
                                    [ValueSource("_1S_F_W_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_S_D([ValueSource("_F_Cvt_NZ_SU_S_D_")] uint opcodes,
                                    [ValueSource("_1D_F_X_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_V_2S_4S([ValueSource("_F_Cvt_NZ_SU_V_2S_4S_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_2S_F_W_")] ulong z,
                                        [ValueSource("_2S_F_W_")] ulong a,
                                        [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvt_NZ_SU_V_2D([ValueSource("_F_Cvt_NZ_SU_V_2D_")] uint opcodes,
                                     [Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_1D_F_X_")] ulong z,
                                     [ValueSource("_1D_F_X_")] ulong a)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvtl_V_4H4S_8H4S([ValueSource("_F_Cvtl_V_4H4S_8H4S_")] uint opcodes,
                                       [Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [ValueSource("_4H_F_")] ulong z,
                                       [ValueSource("_4H_F_")] ulong a,
                                       [Values(0b0u, 0b1u)] uint q, // <4H4S, 8H4S>
                                       [Values(RMode.Rn)] RMode rMode)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(q == 0u ? a : 0ul, q == 1u ? a : 0ul);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = (int)rMode << (int)Fpcr.RMode;
            fpcr |= rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);
            fpcr |= rnd & (1 << (int)Fpcr.Ahp);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Ofc | Fpsr.Ufc | Fpsr.Ixc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Cvtl_V_2S2D_4S2D([ValueSource("_F_Cvtl_V_2S2D_4S2D_")] uint opcodes,
                                       [Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [ValueSource("_2S_F_")] ulong z,
                                       [ValueSource("_2S_F_")] ulong a,
                                       [Values(0b0u, 0b1u)] uint q) // <2S2D, 4S2D>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(q == 0u ? a : 0ul, q == 1u ? a : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit] // Unicorn seems to default all rounding modes to RMode.Rn.
        public void F_Cvtn_V_4S4H_4S8H([ValueSource("_F_Cvtn_V_4S4H_4S8H_")] uint opcodes,
                                       [Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [ValueSource("_2S_F_")] ulong z,
                                       [ValueSource("_2S_F_")] ulong a,
                                       [Values(0b0u, 0b1u)] uint q, // <4S4H, 4S8H>
                                       [Values(RMode.Rn)] RMode rMode)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = (int)rMode << (int)Fpcr.RMode;
            fpcr |= rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);
            fpcr |= rnd & (1 << (int)Fpcr.Ahp);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Ofc | Fpsr.Ufc | Fpsr.Ixc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit] // Unicorn seems to default all rounding modes to RMode.Rn.
        public void F_Cvtn_V_2D2S_2D4S([ValueSource("_F_Cvtn_V_2D2S_2D4S_")] uint opcodes,
                                       [Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [ValueSource("_1D_F_")] ulong z,
                                       [ValueSource("_1D_F_")] ulong a,
                                       [Values(0b0u, 0b1u)] uint q) // <2D2S, 2D4S>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Recpe_Rsqrte_S_S([ValueSource("_F_Recpe_Rsqrte_S_S_")] uint opcodes,
                                       [ValueSource("_1S_F_")] ulong a,
                                       [Values(RMode.Rn)] RMode rMode)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = (int)rMode << (int)Fpcr.RMode;
            fpcr |= rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Ofc | Fpsr.Ufc | Fpsr.Ixc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Recpe_Rsqrte_S_D([ValueSource("_F_Recpe_Rsqrte_S_D_")] uint opcodes,
                                       [ValueSource("_1D_F_")] ulong a,
                                       [Values(RMode.Rn)] RMode rMode)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = (int)rMode << (int)Fpcr.RMode;
            fpcr |= rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Ofc | Fpsr.Ufc | Fpsr.Ixc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Recpe_Rsqrte_V_2S_4S([ValueSource("_F_Recpe_Rsqrte_V_2S_4S_")] uint opcodes,
                                           [Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [ValueSource("_2S_F_")] ulong z,
                                           [ValueSource("_2S_F_")] ulong a,
                                           [Values(0b0u, 0b1u)] uint q, // <2S, 4S>
                                           [Values(RMode.Rn)] RMode rMode)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = (int)rMode << (int)Fpcr.RMode;
            fpcr |= rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Ofc | Fpsr.Ufc | Fpsr.Ixc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Recpe_Rsqrte_V_2D([ValueSource("_F_Recpe_Rsqrte_V_2D_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_1D_F_")] ulong z,
                                        [ValueSource("_1D_F_")] ulong a,
                                        [Values(RMode.Rn)] RMode rMode)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            int rnd = (int)TestContext.CurrentContext.Random.NextUInt();

            int fpcr = (int)rMode << (int)Fpcr.RMode;
            fpcr |= rnd & (1 << (int)Fpcr.Fz);
            fpcr |= rnd & (1 << (int)Fpcr.Dn);

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Ioc | Fpsr.Dzc | Fpsr.Ofc | Fpsr.Ufc | Fpsr.Ixc | Fpsr.Idc);
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_AMNPZ_S_S([ValueSource("_F_Rint_AMNPZ_S_S_")] uint opcodes,
                                     [ValueSource("_1S_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_AMNPZ_S_D([ValueSource("_F_Rint_AMNPZ_S_D_")] uint opcodes,
                                     [ValueSource("_1D_F_")] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_AMNPZ_V_2S_4S([ValueSource("_F_Rint_AMNPZ_V_2S_4S_")] uint opcodes,
                                         [Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [ValueSource("_2S_F_")] ulong z,
                                         [ValueSource("_2S_F_")] ulong a,
                                         [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_AMNPZ_V_2D([ValueSource("_F_Rint_AMNPZ_V_2D_")] uint opcodes,
                                      [Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [ValueSource("_1D_F_")] ulong z,
                                      [ValueSource("_1D_F_")] ulong a)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_IX_S_S([ValueSource("_F_Rint_IX_S_S_")] uint opcodes,
                                  [ValueSource("_1S_F_")] ulong a,
                                  [Values] RMode rMode)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            int fpcr = (int)rMode << (int)Fpcr.RMode;

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_IX_S_D([ValueSource("_F_Rint_IX_S_D_")] uint opcodes,
                                  [ValueSource("_1D_F_")] ulong a,
                                  [Values] RMode rMode)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            int fpcr = (int)rMode << (int)Fpcr.RMode;

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_IX_V_2S_4S([ValueSource("_F_Rint_IX_V_2S_4S_")] uint opcodes,
                                      [Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [ValueSource("_2S_F_")] ulong z,
                                      [ValueSource("_2S_F_")] ulong a,
                                      [Values(0b0u, 0b1u)] uint q, // <2S, 4S>
                                      [Values] RMode rMode)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            int fpcr = (int)rMode << (int)Fpcr.RMode;

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void F_Rint_IX_V_2D([ValueSource("_F_Rint_IX_V_2D_")] uint opcodes,
                                   [Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_1D_F_")] ulong z,
                                   [ValueSource("_1D_F_")] ulong a,
                                   [Values] RMode rMode)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            int fpcr = (int)rMode << (int)Fpcr.RMode;

            SingleOpcode(opcodes, v0: v0, v1: v1, fpcr: fpcr);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NEG <V><d>, <V><n>")]
        public void Neg_S_D([Values(0u)]     uint rd,
                            [Values(1u, 0u)] uint rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x7EE0B800; // NEG D0, D0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_8B_4H_2S([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E20B800; // NEG V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                       [Values(1u, 0u)] uint rn,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E20B800; // NEG V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NOT <Vd>.<T>, <Vn>.<T>")]
        public void Not_V_8B([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x2E205800; // NOT V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("NOT <Vd>.<T>, <Vn>.<T>")]
        public void Not_V_16B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x6E205800; // NOT V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Vd>.<T>, <Vn>.<T>")]
        public void Rbit_V_8B([Values(0u)]     uint rd,
                              [Values(1u, 0u)] uint rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x2E605800; // RBIT V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Vd>.<T>, <Vn>.<T>")]
        public void Rbit_V_16B([Values(0u)]     uint rd,
                               [Values(1u, 0u)] uint rn,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x6E605800; // RBIT V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Vd>.<T>, <Vn>.<T>")]
        public void Rev16_V_8B([Values(0u)]     uint rd,
                               [Values(1u, 0u)] uint rn,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x0E201800; // REV16 V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Vd>.<T>, <Vn>.<T>")]
        public void Rev16_V_16B([Values(0u)]     uint rd,
                                [Values(1u, 0u)] uint rn,
                                [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                                [ValueSource("_8B_")] [Random(RndCnt)] ulong a)
        {
            uint opcode = 0x4E201800; // REV16 V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV32 <Vd>.<T>, <Vn>.<T>")]
        public void Rev32_V_8B_4H([Values(0u)]     uint rd,
                                  [Values(1u, 0u)] uint rn,
                                  [ValueSource("_8B4H_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_8B4H_")] [Random(RndCnt)] ulong a,
                                  [Values(0b00u, 0b01u)] uint size) // <8B, 4H>
        {
            uint opcode = 0x2E200800; // REV32 V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV32 <Vd>.<T>, <Vn>.<T>")]
        public void Rev32_V_16B_8H([Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong a,
                                   [Values(0b00u, 0b01u)] uint size) // <16B, 8H>
        {
            uint opcode = 0x6E200800; // REV32 V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV64 <Vd>.<T>, <Vn>.<T>")]
        public void Rev64_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E200800; // REV64 V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV64 <Vd>.<T>, <Vn>.<T>")]
        public void Rev64_V_16B_8H_4S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint opcode = 0x4E200800; // REV64 V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Sadalp_V_8B4H_4H2S_2S1D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint opcode = 0x0E206800; // SADALP V0.4H, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Sadalp_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E206800; // SADALP V0.8H, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Saddlp_V_8B4H_4H2S_2S1D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint opcode = 0x0E202800; // SADDLP V0.4H, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Saddlp_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x4E202800; // SADDLP V0.8H, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_S_S([ValueSource("_SU_Cvt_F_S_S_")] uint opcodes,
                                 [ValueSource("_1S_")] [Random(RndCnt)] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_S_D([ValueSource("_SU_Cvt_F_S_D_")] uint opcodes,
                                 [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpTolerances: FpTolerances.UpToOneUlpsD); // unsigned
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_V_2S_4S([ValueSource("_SU_Cvt_F_V_2S_4S_")] uint opcodes,
                                     [Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_2S_")] [Random(RndCnt)] ulong a,
                                     [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise] [Explicit]
        public void SU_Cvt_F_V_2D([ValueSource("_SU_Cvt_F_V_2D_")] uint opcodes,
                                  [Values(0u)]     uint rd,
                                  [Values(1u, 0u)] uint rn,
                                  [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_1D_")] [Random(RndCnt)] ulong a)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpTolerances: FpTolerances.UpToOneUlpsD); // unsigned
        }

        [Test, Pairwise]
        public void Sha1h_Sha1su1_V([ValueSource("_Sha1h_Sha1su1_V_")] uint opcodes,
                                    [Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [Random(RndCnt / 2)] ulong z0, [Random(RndCnt / 2)] ulong z1,
                                    [Random(RndCnt / 2)] ulong a0, [Random(RndCnt / 2)] ulong a1)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z0, z1);
            Vector128<float> v1 = MakeVectorE0E1(a0, a1);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Sha256su0_V([ValueSource("_Sha256su0_V_")] uint opcodes,
                                [Values(0u)]     uint rd,
                                [Values(1u, 0u)] uint rn,
                                [Random(RndCnt / 2)] ulong z0, [Random(RndCnt / 2)] ulong z1,
                                [Random(RndCnt / 2)] ulong a0, [Random(RndCnt / 2)] ulong a1)
        {
            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);

            Vector128<float> v0 = MakeVectorE0E1(z0, z1);
            Vector128<float> v1 = MakeVectorE0E1(a0, a1);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHLL{2} <Vd>.<Ta>, <Vn>.<Tb>, #<shift>")]
        public void Shll_V([Values(0u)]     uint rd,
                           [Values(1u, 0u)] uint rn,
                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                           [Values(0b00u, 0b01u, 0b10u)] uint size, // <shift: 8, 16, 32>
                           [Values(0b0u, 0b1u)] uint q)
        {
            uint opcode = 0x2E213800; // SHLL V0.8H, V0.8B, #8
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);
            opcode |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(q == 0u ? a : 0ul, q == 1u ? a : 0ul);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SQABS <V><d>, <V><n>")]
        public void Sqabs_S_B_H_S_D([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint opcode = 0x5E207800; // SQABS B0, B0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQABS <Vd>.<T>, <Vn>.<T>")]
        public void Sqabs_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E207800; // SQABS V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQABS <Vd>.<T>, <Vn>.<T>")]
        public void Sqabs_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E207800; // SQABS V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQNEG <V><d>, <V><n>")]
        public void Sqneg_S_B_H_S_D([Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint opcode = 0x7E207800; // SQNEG B0, B0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQNEG <Vd>.<T>, <Vn>.<T>")]
        public void Sqneg_V_8B_4H_2S([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E207800; // SQNEG V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQNEG <Vd>.<T>, <Vn>.<T>")]
        public void Sqneg_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E207800; // SQNEG V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQXTN <Vb><d>, <Va><n>")]
        public void Sqxtn_S_HB_SH_DS([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint opcode = 0x5E214800; // SQXTN B0, H0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x0E214800; // SQXTN V0.8B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x4E214800; // SQXTN2 V0.16B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQXTUN <Vb><d>, <Va><n>")]
        public void Sqxtun_S_HB_SH_DS([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong a,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint opcode = 0x7E212800; // SQXTUN B0, H0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQXTUN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtun_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x2E212800; // SQXTUN V0.8B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SQXTUN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtun_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x6E212800; // SQXTUN2 V0.16B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SUQADD <V><d>, <V><n>")]
        public void Suqadd_S_B_H_S_D([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint opcode = 0x5E203800; // SUQADD B0, B0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SUQADD <Vd>.<T>, <Vn>.<T>")]
        public void Suqadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x0E203800; // SUQADD V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("SUQADD <Vd>.<T>, <Vn>.<T>")]
        public void Suqadd_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                          [Values(1u, 0u)] uint rn,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                          [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x4E203800; // SUQADD V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uadalp_V_8B4H_4H2S_2S1D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint opcode = 0x2E206800; // UADALP V0.4H, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADALP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uadalp_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E206800; // UADALP V0.8H, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uaddlp_V_8B4H_4H2S_2S1D([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B4H, 4H2S, 2S1D>
        {
            uint opcode = 0x2E202800; // UADDLP V0.4H, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UADDLP <Vd>.<Ta>, <Vn>.<Tb>")]
        public void Uaddlp_V_16B8H_8H4S_4S2D([Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint opcode = 0x6E202800; // UADDLP V0.8H, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UQXTN <Vb><d>, <Va><n>")]
        public void Uqxtn_S_HB_SH_DS([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint opcode = 0x7E214800; // UQXTN B0, H0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Uqxtn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                           [Values(1u, 0u)] uint rn,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x2E214800; // UQXTN V0.8B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("UQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Uqxtn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                            [Values(1u, 0u)] uint rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x6E214800; // UQXTN2 V0.16B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("USQADD <V><d>, <V><n>")]
        public void Usqadd_S_B_H_S_D([Values(0u)]     uint rd,
                                     [Values(1u, 0u)] uint rn,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong z,
                                     [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong a,
                                     [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint opcode = 0x7E203800; // USQADD B0, B0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("USQADD <Vd>.<T>, <Vn>.<T>")]
        public void Usqadd_V_8B_4H_2S([Values(0u)]     uint rd,
                                      [Values(1u, 0u)] uint rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint opcode = 0x2E203800; // USQADD V0.8B, V0.8B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("USQADD <Vd>.<T>, <Vn>.<T>")]
        public void Usqadd_V_16B_8H_4S_2D([Values(0u)]     uint rd,
                                          [Values(1u, 0u)] uint rn,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong z,
                                          [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong a,
                                          [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint opcode = 0x6E203800; // USQADD V0.16B, V0.16B
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("XTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Xtn_V_8H8B_4S4H_2D2S([Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                         [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint opcode = 0x0E212800; // XTN V0.8B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("XTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Xtn_V_8H16B_4S8H_2D4S([Values(0u)]     uint rd,
                                          [Values(1u, 0u)] uint rn,
                                          [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong z,
                                          [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong a,
                                          [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint opcode = 0x4E212800; // XTN2 V0.16B, V0.8H
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((size & 3) << 22);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }
#endif
    }
}
