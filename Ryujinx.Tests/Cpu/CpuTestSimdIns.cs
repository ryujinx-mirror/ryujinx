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
            uint Imm5 = (1U << Size) & 0x1F;

            uint Opcode = 0x0E000C00; // RESERVED
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= (Imm5 << 16);
            Opcode |= ((Q & 1) << 30);

            ulong Z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0E1(Z, Z);

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, V0: V0);

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

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, V0: V0);

            CompareAgainstUnicorn();
        }
#endif
    }
}
