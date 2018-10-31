#define SimdIns

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdIns")] // Tested: second half of 2018.
    public sealed class CpuTestSimdIns : CpuTest
    {
#if SimdIns

#region "ValueSource"
        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
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

        private static uint[] _W_()
        {
            return new uint[] { 0x00000000u, 0x0000007Fu,
                                0x00000080u, 0x000000FFu,
                                0x00007FFFu, 0x00008000u,
                                0x0000FFFFu, 0x7FFFFFFFu,
                                0x80000000u, 0xFFFFFFFFu };
        }

        private static ulong[] _X_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

        private const int RndCnt = 2;

        [Test, Pairwise, Description("DUP <Vd>.<T>, <R><n>")]
        public void Dup_Gp_W([Values(0u)]      uint Rd,
                             [Values(1u, 31u)] uint Rn,
                             [ValueSource("_W_")] [Random(RndCnt)] uint Wn,
                             [Values(0, 1, 2)] int Size,  // Q0: <8B,  4H, 2S>
                             [Values(0b0u, 0b1u)] uint Q) // Q1: <16B, 8H, 4S>
        {
            uint Imm5 = (1u << Size) & 0x1Fu;

            uint Opcode = 0x0E000C00; // RESERVED
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (Imm5 << 16);
            Opcode |= ((Q & 1) << 30);

            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, V0: V0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("DUP <Vd>.<T>, <R><n>")]
        public void Dup_Gp_X([Values(0u)]      uint Rd,
                             [Values(1u, 31u)] uint Rn,
                             [ValueSource("_X_")] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0x4E080C00; // DUP V0.2D, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, V0: V0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMOV <Wd>, <Vn>.<Ts>[<index>]")]
        public void Smov_S_W([Values(0u, 31u)] uint Rd,
                             [Values(1u)]      uint Rn,
                             [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                             [Values(0, 1)] int Size, // <B, H>
                             [Values(0u, 1u, 2u, 3u)] uint Index)
        {
            uint Imm5 = (Index << (Size + 1) | 1u << Size) & 0x1Fu;

            uint Opcode = 0x0E002C00; // RESERVED
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (Imm5 << 16);

            ulong _X0 = (ulong)TestContext.CurrentContext.Random.NextUInt() << 32;
            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            Vector128<float> V1 = MakeVectorE0(A);

            CpuThreadState ThreadState = SingleOpcode(Opcode, X0: _X0, X31: _W31, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMOV <Xd>, <Vn>.<Ts>[<index>]")]
        public void Smov_S_X([Values(0u, 31u)] uint Rd,
                             [Values(1u)]      uint Rn,
                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                             [Values(0, 1, 2)] int Size, // <B, H, S>
                             [Values(0u, 1u)] uint Index)
        {
            uint Imm5 = (Index << (Size + 1) | 1u << Size) & 0x1Fu;

            uint Opcode = 0x4E002C00; // RESERVED
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (Imm5 << 16);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V1 = MakeVectorE0(A);

            CpuThreadState ThreadState = SingleOpcode(Opcode, X31: _X31, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMOV <Wd>, <Vn>.<Ts>[<index>]")]
        public void Umov_S_W([Values(0u, 31u)] uint Rd,
                             [Values(1u)]      uint Rn,
                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                             [Values(0, 1, 2)] int Size, // <B, H, S>
                             [Values(0u, 1u)] uint Index)
        {
            uint Imm5 = (Index << (Size + 1) | 1u << Size) & 0x1Fu;

            uint Opcode = 0x0E003C00; // RESERVED
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (Imm5 << 16);

            ulong _X0 = (ulong)TestContext.CurrentContext.Random.NextUInt() << 32;
            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            Vector128<float> V1 = MakeVectorE0(A);

            CpuThreadState ThreadState = SingleOpcode(Opcode, X0: _X0, X31: _W31, V1: V1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMOV <Xd>, <Vn>.<Ts>[<index>]")]
        public void Umov_S_X([Values(0u, 31u)] uint Rd,
                             [Values(1u)]      uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [Values(3)] int Size, // <D>
                             [Values(0u)] uint Index)
        {
            uint Imm5 = (Index << (Size + 1) | 1u << Size) & 0x1Fu;

            uint Opcode = 0x4E003C00; // RESERVED
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (Imm5 << 16);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V1 = MakeVectorE0(A);

            CpuThreadState ThreadState = SingleOpcode(Opcode, X31: _X31, V1: V1);

            CompareAgainstUnicorn();
        }
#endif
    }
}
