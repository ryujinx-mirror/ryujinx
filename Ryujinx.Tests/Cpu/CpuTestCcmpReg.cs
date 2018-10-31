#define CcmpReg

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("CcmpReg")] // Tested: second half of 2018.
    public sealed class CpuTestCcmpReg : CpuTest
    {
#if CcmpReg
        private const int RndCnt     = 2;
        private const int RndCntNzcv = 2;

        [Test, Pairwise, Description("CCMN <Xn>, <Xm>, #<nzcv>, <cond>")]
        public void Ccmn_64bit([Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                               [Random(0u, 15u, RndCntNzcv)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xBA400000; // CCMN X0, X0, #0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5);
            Opcode |= ((cond & 15) << 12) | ((nzcv & 15) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CCMN <Wn>, <Wm>, #<nzcv>, <cond>")]
        public void Ccmn_32bit([Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                               [Random(0u, 15u, RndCntNzcv)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x3A400000; // CCMN W0, W0, #0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5);
            Opcode |= ((cond & 15) << 12) | ((nzcv & 15) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CCMP <Xn>, <Xm>, #<nzcv>, <cond>")]
        public void Ccmp_64bit([Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                               [Random(0u, 15u, RndCntNzcv)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xFA400000; // CCMP X0, X0, #0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5);
            Opcode |= ((cond & 15) << 12) | ((nzcv & 15) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CCMP <Wn>, <Wm>, #<nzcv>, <cond>")]
        public void Ccmp_32bit([Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                               [Random(0u, 15u, RndCntNzcv)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x7A400000; // CCMP W0, W0, #0, EQ
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5);
            Opcode |= ((cond & 15) << 12) | ((nzcv & 15) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
