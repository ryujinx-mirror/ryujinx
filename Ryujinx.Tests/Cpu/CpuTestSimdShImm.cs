#define SimdShImm

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdShImm")] // Tested: second half of 2018.
    public sealed class CpuTestSimdShImm : CpuTest
    {
#if SimdShImm

#region "ValueSource (Types)"
        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
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
#endregion

        private const int RndCnt = 2;

        [Test, Pairwise, Description("SHL <V><d>, <V><n>, #<shift>")]
        public void Shl_S_D([Values(0u)]     uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                            [Range(0u, 63u)] uint Shift)
        {
            uint ImmHB = (64 + Shift) & 0x7F;

            uint Opcode = 0x5F405400; // SHL D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (ImmHB << 16);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_8B_16B([Values(0u)]     uint Rd,
                                 [Values(1u, 0u)] uint Rn,
                                 [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                                 [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                                 [Range(0u, 7u)] uint Shift,
                                 [Values(0b0u, 0b1u)] uint Q) // <8B, 16B>
        {
            uint ImmHB = (8 + Shift) & 0x7F;

            uint Opcode = 0x0F085400; // SHL V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (ImmHB << 16);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_4H_8H([Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [ValueSource("_4H_")] [Random(RndCnt)] ulong Z,
                                [ValueSource("_4H_")] [Random(RndCnt)] ulong A,
                                [Range(0u, 15u)] uint Shift,
                                [Values(0b0u, 0b1u)] uint Q) // <4H, 8H>
        {
            uint ImmHB = (16 + Shift) & 0x7F;

            uint Opcode = 0x0F105400; // SHL V0.4H, V0.4H, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (ImmHB << 16);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_2S_4S([Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [ValueSource("_2S_")] [Random(RndCnt)] ulong Z,
                                [ValueSource("_2S_")] [Random(RndCnt)] ulong A,
                                [Range(0u, 31u)] uint Shift,
                                [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            uint ImmHB = (32 + Shift) & 0x7F;

            uint Opcode = 0x0F205400; // SHL V0.2S, V0.2S, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (ImmHB << 16);
            Opcode |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SHL <Vd>.<T>, <Vn>.<T>, #<shift>")]
        public void Shl_V_2D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [Range(0u, 63u)] uint Shift)
        {
            uint ImmHB = (64 + Shift) & 0x7F;

            uint Opcode = 0x4F405400; // SHL V0.2D, V0.2D, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (ImmHB << 16);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_S_D([ValueSource("_ShrImm_S_D_")] uint Opcodes,
                               [Values(0u)]     uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                               [Range(1u, 64u)] uint Shift)
        {
            uint ImmHB = (128 - Shift) & 0x7F;

            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (ImmHB << 16);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_8B_16B([ValueSource("_ShrImm_V_8B_16B_")] uint Opcodes,
                                    [Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                                    [Range(1u, 8u)] uint Shift,
                                    [Values(0b0u, 0b1u)] uint Q) // <8B, 16B>
        {
            uint ImmHB = (16 - Shift) & 0x7F;

            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (ImmHB << 16);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_4H_8H([ValueSource("_ShrImm_V_4H_8H_")] uint Opcodes,
                                   [Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_4H_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_4H_")] [Random(RndCnt)] ulong A,
                                   [Range(1u, 16u)] uint Shift,
                                   [Values(0b0u, 0b1u)] uint Q) // <4H, 8H>
        {
            uint ImmHB = (32 - Shift) & 0x7F;

            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (ImmHB << 16);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_2S_4S([ValueSource("_ShrImm_V_2S_4S_")] uint Opcodes,
                                   [Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_2S_")] [Random(RndCnt)] ulong A,
                                   [Range(1u, 32u)] uint Shift,
                                   [Values(0b0u, 0b1u)] uint Q) // <2S, 4S>
        {
            uint ImmHB = (64 - Shift) & 0x7F;

            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (ImmHB << 16);
            Opcodes |= ((Q & 1) << 30);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A * Q);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void ShrImm_V_2D([ValueSource("_ShrImm_V_2D_")] uint Opcodes,
                                [Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                                [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                                [Range(1u, 64u)] uint Shift)
        {
            uint ImmHB = (128 - Shift) & 0x7F;

            Opcodes |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcodes |= (ImmHB << 16);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);

            AThreadState ThreadState = SingleOpcode(Opcodes, V0: V0, V1: V1);

            CompareAgainstUnicorn();
        }
#endif
    }
}
