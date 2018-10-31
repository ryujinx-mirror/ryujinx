#define Csel

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Csel")] // Tested: second half of 2018.
    public sealed class CpuTestCsel : CpuTest
    {
#if Csel
        private const int RndCnt = 2;

        [Test, Pairwise, Description("CSEL <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csel_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x9A800000; // CSEL X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSEL <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csel_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x1A800000; // CSEL W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINC <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csinc_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x9A800400; // CSINC X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINC <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csinc_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x1A800400; // CSINC W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINV <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csinv_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xDA800000; // CSINV X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINV <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csinv_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x5A800000; // CSINV W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSNEG <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csneg_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xDA800400; // CSNEG X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSNEG <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csneg_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x5A800400; // CSNEG W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
