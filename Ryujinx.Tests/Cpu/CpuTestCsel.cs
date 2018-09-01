//#define Csel

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("Csel"), Ignore("Tested: second half of 2018.")]
    public sealed class CpuTestCsel : CpuTest
    {
#if Csel
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("CSEL <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csel_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x9A800000; // CSEL X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Csel(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSEL <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csel_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x1A800000; // CSEL W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Csel(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSINC <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csinc_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x9A800400; // CSINC X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Csinc(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSINC <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csinc_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x1A800400; // CSINC W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Csinc(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSINV <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csinv_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xDA800000; // CSINV X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Csinv(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSINV <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csinv_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x5A800000; // CSINV W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Csinv(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSNEG <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csneg_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xDA800400; // CSNEG X0, X0, X0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Csneg(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("CSNEG <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csneg_32bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x5A800400; // CSNEG W0, W0, W0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((cond & 15) << 12);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Csneg(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }
#endif
    }
}
