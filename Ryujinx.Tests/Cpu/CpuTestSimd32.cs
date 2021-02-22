#define Simd32

using ARMeilleure.State;
using NUnit.Framework;
using System.Collections.Generic;

namespace Ryujinx.Tests.Cpu
{
    [Category("Simd32")]
    public sealed class CpuTestSimd32 : CpuTest32
    {
#if Simd32

#region "ValueSource (Opcodes)"
        private static uint[] _Vabs_Vneg_V_()
        {
            return new uint[]
            {
                0xf3b10300u, // VABS.S8 D0, D0
                0xf3b10380u  // VNEG.S8 D0, D0
            };
        }
#endregion

#region "ValueSource (Types)"
        private static ulong[] _8B4H2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
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

        private static IEnumerable<ulong> _GenPopCnt8B_()
        {
            for (ulong cnt = 0ul; cnt <= 255ul; cnt++)
            {
                yield return (cnt << 56) | (cnt << 48) | (cnt << 40) | (cnt << 32) |
                             (cnt << 24) | (cnt << 16) | (cnt << 08) | cnt;
            }
        }
#endregion

        private const int RndCnt = 2;

        private static readonly bool NoZeros = false;
        private static readonly bool NoInfs  = false;
        private static readonly bool NoNaNs  = false;

        [Test, Pairwise]
        public void Vabs_Vneg_V_S8_S16_S32([ValueSource("_Vabs_Vneg_V_")] uint opcode,
                                           [Range(0u, 3u)] uint rd,
                                           [Range(0u, 3u)] uint rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong b,
                                           [Values(0u, 1u, 2u)] uint size, // <S8, S16, S32>
                                           [Values] bool q)
        {
            const bool f = false;

            Vabs_Vneg_V(opcode, rd, rm, z, b, size, f, q);
        }

        [Test, Pairwise]
        public void Vabs_Vneg_V_F32([ValueSource("_Vabs_Vneg_V_")] uint opcode,
                                    [Range(0u, 3u)] uint rd,
                                    [Range(0u, 3u)] uint rm,
                                    [ValueSource("_2S_F_")] ulong z,
                                    [ValueSource("_2S_F_")] ulong b,
                                    [Values] bool q)
        {
            const uint size = 0b10; // <F32>
            const bool f = true;

            Vabs_Vneg_V(opcode, rd, rm, z, b, size, f, q);
        }

        private void Vabs_Vneg_V(uint opcode, uint rd, uint rm, ulong z, ulong b, uint size, bool f, bool q)
        {
            if (f)
            {
                opcode |= 1 << 10;
            }

            if (q)
            {
                opcode |= 1 << 6;

                rd >>= 1; rd <<= 1;
                rm >>= 1; rm <<= 1;
            }

            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((rm & 0xf) << 0)  | ((rm & 0x10) << 1);

            opcode |= (size & 0x3) << 18;

            V128 v0 = MakeVectorE0E1(z, ~z);
            V128 v1 = MakeVectorE0E1(b, ~b);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("VCNT.8 D0, D0 | VCNT.8 Q0, Q0")]
        public void Vcnt([Values(0u, 1u)] uint rd,
                         [Values(0u, 1u)] uint rm,
                         [ValueSource(nameof(_GenPopCnt8B_))] [Random(RndCnt)] ulong d0,
                         [Values] bool q)
        {
            ulong d1 = ~d0; // It's expensive to have a second generator.

            uint opcode = 0xf3b00500u; // VCNT.8 D0, D0

            if (q)
            {
                opcode |= 1u << 6;

                rd &= ~1u;
                rm &= ~1u;
            }

            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((rm & 0xf) << 0)  | ((rm & 0x10) << 1);

            V128 v0 = MakeVectorE0E1(d0, d1);

            SingleOpcode(opcode, v0: v0);

            CompareAgainstUnicorn();
        }
#endif
    }
}
