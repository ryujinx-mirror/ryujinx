#define Csel

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Csel")]
    public sealed class CpuTestCsel : CpuTest
    {
#if Csel

        [Test, Pairwise, Description("CSEL <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csel_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0x9A800000; // CSEL X0, X0, X0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSEL <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csel_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0x1A800000; // CSEL W0, W0, W0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINC <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csinc_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0x9A800400; // CSINC X0, X0, X0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINC <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csinc_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0x1A800400; // CSINC W0, W0, W0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINV <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csinv_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0xDA800000; // CSINV X0, X0, X0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSINV <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csinv_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0x5A800000; // CSINV W0, W0, W0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSNEG <Xd>, <Xn>, <Xm>, <cond>")]
        public void Csneg_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0xDA800400; // CSNEG X0, X0, X0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CSNEG <Wd>, <Wn>, <Wm>, <cond>")]
        public void Csneg_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] uint wm,
                                [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                        0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                        0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                        0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint opcode = 0x5A800400; // CSNEG W0, W0, W0, EQ
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((cond & 15) << 12);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
