#define SimdImm

using NUnit.Framework;

using System.Collections.Generic;
using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdImm")]
    public sealed class CpuTestSimdImm : CpuTest
    {
#if SimdImm

#region "Helper methods"
        // abcdefgh -> aaaaaaaabbbbbbbbccccccccddddddddeeeeeeeeffffffffgggggggghhhhhhhh
        private static ulong ExpandImm8(byte imm8)
        {
            ulong imm64 = 0ul;

            for (int i = 0, j = 0; i < 8; i++, j += 8)
            {
                if (((imm8 >> i) & 0b1) != 0)
                {
                    imm64 |= 0b11111111ul << j;
                }
            }

            return imm64;
        }

        // aaaaaaaabbbbbbbbccccccccddddddddeeeeeeeeffffffffgggggggghhhhhhhh -> abcdefgh
        private static byte ShrinkImm64(ulong imm64)
        {
            byte imm8 = 0;

            for (int i = 0, j = 0; i < 8; i++, j += 8)
            {
                if (((imm64 >> j) & 0b11111111ul) != 0ul) // Note: no format check.
                {
                    imm8 |= (byte)(0b1 << i);
                }
            }

            return imm8;
        }
#endregion

#region "ValueSource (Types)"
        private static IEnumerable<byte> _8BIT_IMM_()
        {
            yield return 0x00;
            yield return 0x7F;
            yield return 0x80;
            yield return 0xFF;

            for (int cnt = 1; cnt <= RndCntImm8; cnt++)
            {
                byte imm8 = TestContext.CurrentContext.Random.NextByte();

                yield return imm8;
            }
        }

        private static IEnumerable<ulong> _64BIT_IMM_()
        {
            yield return ExpandImm8(0x00);
            yield return ExpandImm8(0x7F);
            yield return ExpandImm8(0x80);
            yield return ExpandImm8(0xFF);

            for (int cnt = 1; cnt <= RndCntImm64; cnt++)
            {
                byte imm8 = TestContext.CurrentContext.Random.NextByte();

                yield return ExpandImm8(imm8);
            }
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _Movi_V_8bit_()
        {
            return new uint[]
            {
                0x0F00E400u // MOVI V0.8B, #0
            };
        }

        private static uint[] _Movi_Mvni_V_16bit_shifted_imm_()
        {
            return new uint[]
            {
                0x0F008400u, // MOVI V0.4H, #0
                0x2F008400u  // MVNI V0.4H, #0
            };
        }

        private static uint[] _Movi_Mvni_V_32bit_shifted_imm_()
        {
            return new uint[]
            {
                0x0F000400u, // MOVI V0.2S, #0
                0x2F000400u  // MVNI V0.2S, #0
            };
        }

        private static uint[] _Movi_Mvni_V_32bit_shifting_ones_()
        {
            return new uint[]
            {
                0x0F00C400u, // MOVI V0.2S, #0, MSL #8
                0x2F00C400u  // MVNI V0.2S, #0, MSL #8
            };
        }

        private static uint[] _Movi_V_64bit_scalar_()
        {
            return new uint[]
            {
                0x2F00E400u // MOVI D0, #0
            };
        }

        private static uint[] _Movi_V_64bit_vector_()
        {
            return new uint[]
            {
                0x6F00E400u // MOVI V0.2D, #0
            };
        }
#endregion

        private const int RndCntImm8  = 2;
        private const int RndCntImm64 = 2;

        [Test, Pairwise]
        public void Movi_V_8bit([ValueSource("_Movi_V_8bit_")] uint opcodes,
                                [Values(0u)] uint rd,
                                [ValueSource("_8BIT_IMM_")] byte imm8,
                                [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            uint abc   = (imm8 & 0xE0u) >> 5;
            uint defgh = (imm8 & 0x1Fu);

            opcodes |= ((rd & 31) << 0);
            opcodes |= (abc << 16) | (defgh << 5);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(q == 0u ? z : 0ul);

            SingleOpcode(opcodes, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Movi_Mvni_V_16bit_shifted_imm([ValueSource("_Movi_Mvni_V_16bit_shifted_imm_")] uint opcodes,
                                                  [Values(0u)] uint rd,
                                                  [ValueSource("_8BIT_IMM_")] byte imm8,
                                                  [Values(0b0u, 0b1u)] uint amount, // <0, 8>
                                                  [Values(0b0u, 0b1u)] uint q)      // <4H, 8H>
        {
            uint abc   = (imm8 & 0xE0u) >> 5;
            uint defgh = (imm8 & 0x1Fu);

            opcodes |= ((rd & 31) << 0);
            opcodes |= (abc << 16) | (defgh << 5);
            opcodes |= ((amount & 1) << 13);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(q == 0u ? z : 0ul);

            SingleOpcode(opcodes, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Movi_Mvni_V_32bit_shifted_imm([ValueSource("_Movi_Mvni_V_32bit_shifted_imm_")] uint opcodes,
                                                  [Values(0u)] uint rd,
                                                  [ValueSource("_8BIT_IMM_")] byte imm8,
                                                  [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint amount, // <0, 8, 16, 24>
                                                  [Values(0b0u, 0b1u)] uint q)                      // <2S, 4S>
        {
            uint abc   = (imm8 & 0xE0u) >> 5;
            uint defgh = (imm8 & 0x1Fu);

            opcodes |= ((rd & 31) << 0);
            opcodes |= (abc << 16) | (defgh << 5);
            opcodes |= ((amount & 3) << 13);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(q == 0u ? z : 0ul);

            SingleOpcode(opcodes, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Movi_Mvni_V_32bit_shifting_ones([ValueSource("_Movi_Mvni_V_32bit_shifting_ones_")] uint opcodes,
                                                    [Values(0u)] uint rd,
                                                    [ValueSource("_8BIT_IMM_")] byte imm8,
                                                    [Values(0b0u, 0b1u)] uint amount, // <8, 16>
                                                    [Values(0b0u, 0b1u)] uint q)      // <2S, 4S>
        {
            uint abc   = (imm8 & 0xE0u) >> 5;
            uint defgh = (imm8 & 0x1Fu);

            opcodes |= ((rd & 31) << 0);
            opcodes |= (abc << 16) | (defgh << 5);
            opcodes |= ((amount & 1) << 12);
            opcodes |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(q == 0u ? z : 0ul);

            SingleOpcode(opcodes, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Movi_V_64bit_scalar([ValueSource("_Movi_V_64bit_scalar_")] uint opcodes,
                                        [Values(0u)] uint rd,
                                        [ValueSource("_64BIT_IMM_")] ulong imm)
        {
            byte imm8 = ShrinkImm64(imm);

            uint abc   = (imm8 & 0xE0u) >> 5;
            uint defgh = (imm8 & 0x1Fu);

            opcodes |= ((rd & 31) << 0);
            opcodes |= (abc << 16) | (defgh << 5);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE1(z);

            SingleOpcode(opcodes, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Movi_V_64bit_vector([ValueSource("_Movi_V_64bit_vector_")] uint opcodes,
                                        [Values(0u)] uint rd,
                                        [ValueSource("_64BIT_IMM_")] ulong imm)
        {
            byte imm8 = ShrinkImm64(imm);

            uint abc   = (imm8 & 0xE0u) >> 5;
            uint defgh = (imm8 & 0x1Fu);

            opcodes |= ((rd & 31) << 0);
            opcodes |= (abc << 16) | (defgh << 5);

            SingleOpcode(opcodes);

            CompareAgainstUnicorn();
        }
#endif
    }
}
