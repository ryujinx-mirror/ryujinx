#define SimdIns

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdIns")]
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
        public void Dup_Gp_W([Values(0u)]      uint rd,
                             [Values(1u, 31u)] uint rn,
                             [ValueSource("_W_")] [Random(RndCnt)] uint wn,
                             [Values(0, 1, 2)] int size,  // Q0: <8B,  4H, 2S>
                             [Values(0b0u, 0b1u)] uint q) // Q1: <16B, 8H, 4S>
        {
            uint imm5 = (1u << size) & 0x1Fu;

            uint opcode = 0x0E000C00; // RESERVED
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (imm5 << 16);
            opcode |= ((q & 1) << 30);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);

            SingleOpcode(opcode, x1: wn, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("DUP <Vd>.<T>, <R><n>")]
        public void Dup_Gp_X([Values(0u)]      uint rd,
                             [Values(1u, 31u)] uint rn,
                             [ValueSource("_X_")] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0x4E080C00; // DUP V0.2D, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong z = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v0 = MakeVectorE0E1(z, z);

            SingleOpcode(opcode, x1: xn, v0: v0);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMOV <Wd>, <Vn>.<Ts>[<index>]")]
        public void Smov_S_W([Values(0u, 31u)] uint rd,
                             [Values(1u)]      uint rn,
                             [ValueSource("_8B4H_")] [Random(RndCnt)] ulong a,
                             [Values(0, 1)] int size, // <B, H>
                             [Values(0u, 1u, 2u, 3u)] uint index)
        {
            uint imm5 = (index << (size + 1) | 1u << size) & 0x1Fu;

            uint opcode = 0x0E002C00; // RESERVED
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (imm5 << 16);

            ulong x0 = (ulong)TestContext.CurrentContext.Random.NextUInt() << 32;
            uint w31 = TestContext.CurrentContext.Random.NextUInt();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, x0: x0, x31: w31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMOV <Xd>, <Vn>.<Ts>[<index>]")]
        public void Smov_S_X([Values(0u, 31u)] uint rd,
                             [Values(1u)]      uint rn,
                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                             [Values(0, 1, 2)] int size, // <B, H, S>
                             [Values(0u, 1u)] uint index)
        {
            uint imm5 = (index << (size + 1) | 1u << size) & 0x1Fu;

            uint opcode = 0x4E002C00; // RESERVED
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (imm5 << 16);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, x31: x31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMOV <Wd>, <Vn>.<Ts>[<index>]")]
        public void Umov_S_W([Values(0u, 31u)] uint rd,
                             [Values(1u)]      uint rn,
                             [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong a,
                             [Values(0, 1, 2)] int size, // <B, H, S>
                             [Values(0u, 1u)] uint index)
        {
            uint imm5 = (index << (size + 1) | 1u << size) & 0x1Fu;

            uint opcode = 0x0E003C00; // RESERVED
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (imm5 << 16);

            ulong x0 = (ulong)TestContext.CurrentContext.Random.NextUInt() << 32;
            uint w31 = TestContext.CurrentContext.Random.NextUInt();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, x0: x0, x31: w31, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMOV <Xd>, <Vn>.<Ts>[<index>]")]
        public void Umov_S_X([Values(0u, 31u)] uint rd,
                             [Values(1u)]      uint rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong a,
                             [Values(3)] int size, // <D>
                             [Values(0u)] uint index)
        {
            uint imm5 = (index << (size + 1) | 1u << size) & 0x1Fu;

            uint opcode = 0x4E003C00; // RESERVED
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= (imm5 << 16);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> v1 = MakeVectorE0(a);

            SingleOpcode(opcode, x31: x31, v1: v1);

            CompareAgainstUnicorn();
        }
#endif
    }
}
