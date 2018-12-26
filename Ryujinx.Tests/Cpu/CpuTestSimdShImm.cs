#define SimdShImm

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdShImm")]
    public sealed class CpuTestSimdShImm : CpuTest
    {
#if SimdShImm

#region "ValueSource (Types)"
        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _1H_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x0000000000007FFFul,
                                 0x0000000000008000ul, 0x000000000000FFFFul };
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

        private static ulong[] _4H_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _SU_Shll_V_8B8H_16B8H_()
        {
            return new uint[]
            {
                0x0F08A400u, // SSHLL V0.8H, V0.8B, #0
                0x2F08A400u  // USHLL V0.8H, V0.8B, #0
            };
        }

        private static uint[] _SU_Shll_V_4H4S_8H4S_()
        {
            return new uint[]
            {
                0x0F10A400u, // SSHLL V0.4S, V0.4H, #0
                0x2F10A400u  // USHLL V0.4S, V0.4H, #0
            };
        }

        private static uint[] _SU_Shll_V_2S2D_4S2D_()
        {
            return new uint[]
            {
                0x0F20A400u, // SSHLL V0.2D, V0.2S, #0
                0x2F20A400u  // USHLL V0.2D, V0.2S, #0
            };
        }

        private static uint[] _ShrImm_S_D_()
        {
            return new uint[]
            {
                0x5F402400u, // SRSHR D0, D0, #64
                0x5F403400u, // SRSRA D0, D0, #64
                0x5F400400u, // SSHR  D0, D0, #64
                0x5F401400u, // SSRA  D0, D0, #64
                0x7F402400u, // URSHR D0, D0, #64
                0x7F403400u, // URSRA D0, D0, #64
                0x7F400400u, // USHR  D0, D0, #64
                0x7F401400u  // USRA  D0, D0, #64
            };
        }

        private static uint[] _ShrImm_V_8B_16B_()
        {
            return new uint[]
            {
                0x0F082400u, // SRSHR V0.8B, V0.8B, #8
                0x0F083400u, // SRSRA V0.8B, V0.8B, #8
                0x0F080400u, // SSHR  V0.8B, V0.8B, #8
                0x0F081400u, // SSRA  V0.8B, V0.8B, #8
                0x2F082400u, // URSHR V0.8B, V0.8B, #8
                0x2F083400u, // URSRA V0.8B, V0.8B, #8
                0x2F080400u, // USHR  V0.8B, V0.8B, #8
                0x2F081400u  // USRA  V0.8B, V0.8B, #8
            };
        }

        private static uint[] _ShrImm_V_4H_8H_()
        {
            return new uint[]
            {
                0x0F102400u, // SRSHR V0.4H, V0.4H, #16
                0x0F103400u, // SRSRA V0.4H, V0.4H, #16
                0x0F100400u, // SSHR  V0.4H, V0.4H, #16
                0x0F101400u, // SSRA  V0.4H, V0.4H, #16
                0x2F102400u, // URSHR V0.4H, V0.4H, #16
                0x2F103400u, // URSRA V0.4H, V0.4H, #16
                0x2F100400u, // USHR  V0.4H, V0.4H, #16
                0x2F101400u  // USRA  V0.4H, V0.4H, #16
            };
        }

        private static uint[] _ShrImm_V_2S_4S_()
        {
            return new uint[]
            {
                0x0F202400u, // SRSHR V0.2S, V0.2S, #32
                0x0F203400u, // SRSRA V0.2S, V0.2S, #32
                0x0F200400u, // SSHR  V0.2S, V0.2S, #32
                0x0F201400u, // SSRA  V0.2S, V0.2S, #32
                0x2F202400u, // URSHR V0.2S, V0.2S, #32
                0x2F203400u, // URSRA V0.2S, V0.2S, #32
                0x2F200400u, // USHR  V0.2S, V0.2S, #32
                0x2F201400u  // USRA  V0.2S, V0.2S, #32
            };
        }

        private static uint[] _ShrImm_V_2D_()
        {
            return new uint[]
            {
                0x4F402400u, // SRSHR V0.2D, V0.2D, #64
                0x4F403400u, // SRSRA V0.2D, V0.2D, #64
                0x4F400400u, // SSHR  V0.2D, V0.2D, #64
                0x4F401400u, // SSRA  V0.2D, V0.2D, #64
                0x6F402400u, // URSHR V0.2D, V0.2D, #64
                0x6F403400u, // URSRA V0.2D, V0.2D, #64
                0x6F400400u, // USHR  V0.2D, V0.2D, #64
                0x6F401400u  // USRA  V0.2D, V0.2D, #64
            };
        }

        private static uint[] _ShrImmNarrow_V_8H8B_8H16B_()
        {
            return new uint[]
            {
                0x0F088C00u, // RSHRN V0.8B, V0.8H, #8
                0x0F088400u  // SHRN  V0.8B, V0.8H, #8
            };
        }

        private static uint[] _ShrImmNarrow_V_4S4H_4S8H_()
        {
            return new uint[]
            {
                0x0F108C00u, // RSHRN V0.4H, V0.4S, #16
                0x0F108400u  // SHRN  V0.4H, V0.4S, #16
            };
        }

        private static uint[] _ShrImmNarrow_V_2D2S_2D4S_()
        {
            return new uint[]
            {
                0x0F208C00u, // RSHRN V0.2S, V0.2D, #32
                0x0F208400u  // SHRN  V0.2S, V0.2D, #32
            };
        }

        private static uint[] _ShrImmSaturatingNarrow_S_HB_()
        {
            return new uint[]
            {
                0x5F089C00u, // SQRSHRN  B0, H0, #8
                0x7F089C00u, // UQRSHRN  B0, H0, #8
                0x7F088C00u, // SQRSHRUN B0, H0, #8
                0x5F089400u, // SQSHRN   B0, H0, #8
                0x7F089400u, // UQSHRN   B0, H0, #8
                0x7F088400u  // SQSHRUN  B0, H0, #8
            };
        }

        private static uint[] _ShrImmSaturatingNarrow_S_SH_()
        {
            return new uint[]
            {
                0x5F109C00u, // SQRSHRN  H0, S0, #16
                0x7F109C00u, // UQRSHRN  H0, S0, #16
                0x7F108C00u, // SQRSHRUN H0, S0, #16
                0x5F109400u, // SQSHRN   H0, S0, #16
                0x7F109400u, // UQSHRN   H0, S0, #16
                0x7F108400u  // SQSHRUN  H0, S0, #16
            };
        }

        private static uint[] _ShrImmSaturatingNarrow_S_DS_()
        {
            return new uint[]
            {
                0x5F209C00u, // SQRSHRN  S0, D0, #32
                0x7F209C00u, // UQRSHRN  S0, D0, #32
                0x7F208C00u, // SQRSHRUN S0, D0, #32
                0x5F209400u, // SQSHRN   S0, D0, #32
                0x7F209400u, // UQSHRN   S0, D0, #32
                0x7F208400u  // SQSHRUN  S0, D0, #32
            };
        }

        private static uint[] _ShrImmSaturatingNarrow_V_8H8B_8H16B_()
        {
            return new uint[]
            {
                0x0F089C00u, // SQRSHRN  V0.8B, V0.8H, #8
                0x2F089C00u, // UQRSHRN  V0.8B, V0.8H, #8
                0x2F088C00u, // SQRSHRUN V0.8B, V0.8H, #8
                0x0F089400u, // SQSHRN   V0.8B, V0.8H, #8
                0x2F089400u, // UQSHRN   V0.8B, V0.8H, #8
                0x2F088400u  // SQSHRUN  V0.8B, V0.8H, #8
            };
        }

        private static uint[] _ShrImmSaturatingNarrow_V_4S4H_4S8H_()
        {
            return new uint[]
            {
                0x0F109C00u, // SQRSHRN  V0.4H, V0.4S, #16
                0x2F109C00u, // UQRSHRN  V0.4H, V0.4S, #16
                0x2F108C00u, // SQRSHRUN V0.4H, V0.4S, #16
                0x0F109400u, // SQSHRN   V0.4H, V0.4S, #16
                0x2F109400u, // UQSHRN   V0.4H, V0.4S, #16
                0x2F108400u  // SQSHRUN  V0.4H, V0.4S, #16
            };
        }

        private static uint[] _ShrImmSaturatingNarrow_V_2D2S_2D4S_()
        {
            return new uint[]
            {
                0x0F209C00u, // SQRSHRN  V0.2S, V0.2D, #32
                0x2F209C00u, // UQRSHRN  V0.2S, V0.2D, #32
                0x2F208C00u, // SQRSHRUN V0.2S, V0.2D, #32
                0x0F209400u, // SQSHRN   V0.2S, V0.2D, #32
                0x2F209400u, // UQSHRN   V0.2S, V0.2D, #32
                0x2F208400u  // SQSHRUN  V0.2S, V0.2D, #32
            };
        }
#endregion

        private const int RndCnt = 2;

        [Test, Pairwise, Description("SHL <V><d>, <V><n>, #<shift>")]
        public void Shl_S_D([Values(0u)]     uint rd,
                            [Values(1u, 0u)] uint rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                            [Range(0u, 63u)] uint shift)
        {
            uint immHb = (64 + shift) & 0x7F;

            uint opcode = 0x5F405400; // SHL D0, D0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_8B_16B([Values(0u)]     uint rd,
                                 [Values(1u, 0u)] uint rn,
                                 [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                                 [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                                 [Range(0u, 7u)] uint shift,
                                 [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            uint immHb = (8 + shift) & 0x7F;

            uint opcode = 0x0F085400; // SHL V0.8B, V0.8B, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (immHb << 16);
            opcode |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_4H_8H([Values(0u)]     uint rd,
                                [Values(1u, 0u)] uint rn,
                                [ValueSource("_4H_")] [Random(RndCnt)] ulong z,
                                [ValueSource("_4H_")] [Random(RndCnt)] ulong a,
                                [Range(0u, 15u)] uint shift,
                                [Values(0b0u, 0b1u)] uint q) // <4H, 8H>
        {
            uint immHb = (16 + shift) & 0x7F;

            uint opcode = 0x0F105400; // SHL V0.4H, V0.4H, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (immHb << 16);
            opcode |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_2S_4S([Values(0u)]     uint rd,
                                [Values(1u, 0u)] uint rn,
                                [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                [ValueSource("_2S_")] [Random(RndCnt)] ulong a,
                                [Range(0u, 31u)] uint shift,
                                [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            uint immHb = (32 + shift) & 0x7F;

            uint opcode = 0x0F205400; // SHL V0.2S, V0.2S, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (immHb << 16);
            opcode |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_2D([Values(0u)]     uint rd,
                             [Values(1u, 0u)] uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [Range(0u, 63u)] uint shift)
        {
            uint immHb = (64 + shift) & 0x7F;

            uint opcode = 0x4F405400; // SHL V0.2D, V0.2D, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void SU_Shll_V_8B8H_16B8H([ValueSource("_SU_Shll_V_8B8H_16B8H_")] uint opcodes,
                                         [Values(0u)]     uint rd,
                                         [Values(1u, 0u)] uint rn,
                                         [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                                         [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                                         [Range(0u, 7u)] uint shift,
                                         [Values(0b0u, 0b1u)] uint q) // <8B8H, 16B8H>
        {
            uint immHb = (8 + shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(q == 0u ? a : 0ul, q == 1u ? a : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void SU_Shll_V_4H4S_8H4S([ValueSource("_SU_Shll_V_4H4S_8H4S_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_4H_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_4H_")] [Random(RndCnt)] ulong a,
                                        [Range(0u, 15u)] uint shift,
                                        [Values(0b0u, 0b1u)] uint q) // <4H4S, 8H4S>
        {
            uint immHb = (16 + shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(q == 0u ? a : 0ul, q == 1u ? a : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void SU_Shll_V_2S2D_4S2D([ValueSource("_SU_Shll_V_2S2D_4S2D_")] uint opcodes,
                                        [Values(0u)]     uint rd,
                                        [Values(1u, 0u)] uint rn,
                                        [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                        [ValueSource("_2S_")] [Random(RndCnt)] ulong a,
                                        [Range(0u, 31u)] uint shift,
                                        [Values(0b0u, 0b1u)] uint q) // <2S2D, 4S2D>
        {
            uint immHb = (32 + shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(q == 0u ? a : 0ul, q == 1u ? a : 0ul);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_S_D([ValueSource("_ShrImm_S_D_")] uint opcodes,
                               [Values(0u)]     uint rd,
                               [Values(1u, 0u)] uint rn,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                               [Range(1u, 64u)] uint shift)
        {
            uint immHb = (128 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_8B_16B([ValueSource("_ShrImm_V_8B_16B_")] uint opcodes,
                                    [Values(0u)]     uint rd,
                                    [Values(1u, 0u)] uint rn,
                                    [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                                    [ValueSource("_8B_")] [Random(RndCnt)] ulong a,
                                    [Range(1u, 8u)] uint shift,
                                    [Values(0b0u, 0b1u)] uint q) // <8B, 16B>
        {
            uint immHb = (16 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_4H_8H([ValueSource("_ShrImm_V_4H_8H_")] uint opcodes,
                                   [Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_4H_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_4H_")] [Random(RndCnt)] ulong a,
                                   [Range(1u, 16u)] uint shift,
                                   [Values(0b0u, 0b1u)] uint q) // <4H, 8H>
        {
            uint immHb = (32 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_2S_4S([ValueSource("_ShrImm_V_2S_4S_")] uint opcodes,
                                   [Values(0u)]     uint rd,
                                   [Values(1u, 0u)] uint rn,
                                   [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                   [ValueSource("_2S_")] [Random(RndCnt)] ulong a,
                                   [Range(1u, 32u)] uint shift,
                                   [Values(0b0u, 0b1u)] uint q) // <2S, 4S>
        {
            uint immHb = (64 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a * q);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_2D([ValueSource("_ShrImm_V_2D_")] uint opcodes,
                                [Values(0u)]     uint rd,
                                [Values(1u, 0u)] uint rn,
                                [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                                [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                                [Range(1u, 64u)] uint shift)
        {
            uint immHb = (128 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImmNarrow_V_8H8B_8H16B([ValueSource("_ShrImmNarrow_V_8H8B_8H16B_")] uint opcodes,
                                              [Values(0u)]     uint rd,
                                              [Values(1u, 0u)] uint rn,
                                              [ValueSource("_4H_")] [Random(RndCnt)] ulong z,
                                              [ValueSource("_4H_")] [Random(RndCnt)] ulong a,
                                              [Range(1u, 8u)] uint shift,
                                              [Values(0b0u, 0b1u)] uint q) // <8H8B, 8H16B>
        {
            uint immHb = (16 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImmNarrow_V_4S4H_4S8H([ValueSource("_ShrImmNarrow_V_4S4H_4S8H_")] uint opcodes,
                                             [Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_2S_")] [Random(RndCnt)] ulong a,
                                             [Range(1u, 16u)] uint shift,
                                             [Values(0b0u, 0b1u)] uint q) // <4S4H, 4S8H>
        {
            uint immHb = (32 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImmNarrow_V_2D2S_2D4S([ValueSource("_ShrImmNarrow_V_2D2S_2D4S_")] uint opcodes,
                                             [Values(0u)]     uint rd,
                                             [Values(1u, 0u)] uint rn,
                                             [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                                             [Range(1u, 32u)] uint shift,
                                             [Values(0b0u, 0b1u)] uint q) // <2D2S, 2D4S>
        {
            uint immHb = (64 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0E1(a, a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImmSaturatingNarrow_S_HB([ValueSource("_ShrImmSaturatingNarrow_S_HB_")] uint opcodes,
                                                [Values(0u)]     uint rd,
                                                [Values(1u, 0u)] uint rn,
                                                [ValueSource("_1H_")] [Random(RndCnt)] ulong z,
                                                [ValueSource("_1H_")] [Random(RndCnt)] ulong a,
                                                [Range(1u, 8u)] uint shift)
        {
            uint immHb = (16 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise]
        public void ShrImmSaturatingNarrow_S_SH([ValueSource("_ShrImmSaturatingNarrow_S_SH_")] uint opcodes,
                                                [Values(0u)]     uint rd,
                                                [Values(1u, 0u)] uint rn,
                                                [ValueSource("_1S_")] [Random(RndCnt)] ulong z,
                                                [ValueSource("_1S_")] [Random(RndCnt)] ulong a,
                                                [Range(1u, 16u)] uint shift)
        {
            uint immHb = (32 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise]
        public void ShrImmSaturatingNarrow_S_DS([ValueSource("_ShrImmSaturatingNarrow_S_DS_")] uint opcodes,
                                                [Values(0u)]     uint rd,
                                                [Values(1u, 0u)] uint rn,
                                                [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                                                [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                                                [Range(1u, 32u)] uint shift)
        {
            uint immHb = (64 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise]
        public void ShrImmSaturatingNarrow_V_8H8B_8H16B([ValueSource("_ShrImmSaturatingNarrow_V_8H8B_8H16B_")] uint opcodes,
                                                        [Values(0u)]     uint rd,
                                                        [Values(1u, 0u)] uint rn,
                                                        [ValueSource("_4H_")] [Random(RndCnt)] ulong z,
                                                        [ValueSource("_4H_")] [Random(RndCnt)] ulong a,
                                                        [Range(1u, 8u)] uint shift,
                                                        [Values(0b0u, 0b1u)] uint q) // <8H8B, 8H16B>
        {
            uint immHb = (16 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise]
        public void ShrImmSaturatingNarrow_V_4S4H_4S8H([ValueSource("_ShrImmSaturatingNarrow_V_4S4H_4S8H_")] uint opcodes,
                                                       [Values(0u)]     uint rd,
                                                       [Values(1u, 0u)] uint rn,
                                                       [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                                       [ValueSource("_2S_")] [Random(RndCnt)] ulong a,
                                                       [Range(1u, 16u)] uint shift,
                                                       [Values(0b0u, 0b1u)] uint q) // <4S4H, 4S8H>
        {
            uint immHb = (32 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise]
        public void ShrImmSaturatingNarrow_V_2D2S_2D4S([ValueSource("_ShrImmSaturatingNarrow_V_2D2S_2D4S_")] uint opcodes,
                                                       [Values(0u)]     uint rd,
                                                       [Values(1u, 0u)] uint rn,
                                                       [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                                                       [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                                                       [Range(1u, 32u)] uint shift,
                                                       [Values(0b0u, 0b1u)] uint q) // <2D2S, 2D4S>
        {
            uint immHb = (64 - shift) & 0x7F;

            opcodes |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcodes |= (immHb << 16);
            opcodes |= ((q & 1) << 30);

            Vector128<float> v0 = MakeVectorE0E1(z, z);
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcodes, v0: v0, v1: v1);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }
#endif
    }
}
