#define Alu

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Alu")] // Tested: second half of 2018.
    public sealed class CpuTestAlu : CpuTest
    {
#if Alu
        private const int RndCnt = 2;

        [Test, Pairwise, Description("CLS <Xd>, <Xn>")]
        public void Cls_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0xDAC01400; // CLS X0, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLS <Wd>, <Wn>")]
        public void Cls_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn)
        {
            uint Opcode = 0x5AC01400; // CLS W0, W0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Xd>, <Xn>")]
        public void Clz_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0xDAC01000; // CLZ X0, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Wd>, <Wn>")]
        public void Clz_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn)
        {
            uint Opcode = 0x5AC01000; // CLZ W0, W0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Xd>, <Xn>")]
        public void Rbit_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0xDAC00000; // RBIT X0, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Wd>, <Wn>")]
        public void Rbit_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn)
        {
            uint Opcode = 0x5AC00000; // RBIT W0, W0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Xd>, <Xn>")]
        public void Rev16_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0xDAC00400; // REV16 X0, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Wd>, <Wn>")]
        public void Rev16_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn)
        {
            uint Opcode = 0x5AC00400; // REV16 W0, W0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV32 <Xd>, <Xn>")]
        public void Rev32_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0xDAC00800; // REV32 X0, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV <Wd>, <Wn>")]
        public void Rev32_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn)
        {
            uint Opcode = 0x5AC00800; // REV W0, W0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV64 <Xd>, <Xn>")]
        public void Rev64_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn)
        {
            uint Opcode = 0xDAC00C00; // REV64 X0, X0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
