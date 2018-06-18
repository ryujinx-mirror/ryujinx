//#define CcmpImm

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("CcmpImm"), Ignore("Tested: first half of 2018.")]
    public sealed class CpuTestCcmpImm : CpuTest
    {
#if CcmpImm
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("CCMN <Xn>, #<imm>, #<nzcv>, <cond>")]
        public void Ccmn_64bit([Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0u, 31u)] [Random(0u, 31u, 3)] uint imm,
                               [Random(0u, 15u, 1)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xBA400800; // CCMN X0, #0, #0, EQ
            Opcode |= ((Rn & 31) << 5);
            Opcode |= ((imm & 31) << 16) | ((cond & 15) << 12) | ((nzcv & 15) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            Bits Op = new Bits(Opcode);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Ccmn_Imm(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[3, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("CCMN <Wn>, #<imm>, #<nzcv>, <cond>")]
        public void Ccmn_32bit([Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0u, 31u)] [Random(0u, 31u, 3)] uint imm,
                               [Random(0u, 15u, 1)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x3A400800; // CCMN W0, #0, #0, EQ
            Opcode |= ((Rn & 31) << 5);
            Opcode |= ((imm & 31) << 16) | ((cond & 15) << 12) | ((nzcv & 15) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            Bits Op = new Bits(Opcode);

            AArch64.X((int)Rn, new Bits(Wn));
            Base.Ccmn_Imm(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[3, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("CCMP <Xn>, #<imm>, #<nzcv>, <cond>")]
        public void Ccmp_64bit([Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0u, 31u)] [Random(0u, 31u, 3)] uint imm,
                               [Random(0u, 15u, 1)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0xFA400800; // CCMP X0, #0, #0, EQ
            Opcode |= ((Rn & 31) << 5);
            Opcode |= ((imm & 31) << 16) | ((cond & 15) << 12) | ((nzcv & 15) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            Bits Op = new Bits(Opcode);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Ccmp_Imm(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[3, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("CCMP <Wn>, #<imm>, #<nzcv>, <cond>")]
        public void Ccmp_32bit([Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0u, 31u)] [Random(0u, 31u, 3)] uint imm,
                               [Random(0u, 15u, 1)] uint nzcv,
                               [Values(0b0000u, 0b0001u, 0b0010u, 0b0011u,             // <EQ, NE, CS/HS, CC/LO,
                                       0b0100u, 0b0101u, 0b0110u, 0b0111u,             //  MI, PL, VS, VC,
                                       0b1000u, 0b1001u, 0b1010u, 0b1011u,             //  HI, LS, GE, LT,
                                       0b1100u, 0b1101u, 0b1110u, 0b1111u)] uint cond) //  GT, LE, AL, NV>
        {
            uint Opcode = 0x7A400800; // CCMP W0, #0, #0, EQ
            Opcode |= ((Rn & 31) << 5);
            Opcode |= ((imm & 31) << 16) | ((cond & 15) << 12) | ((nzcv & 15) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            Bits Op = new Bits(Opcode);

            AArch64.X((int)Rn, new Bits(Wn));
            Base.Ccmp_Imm(Op[31], Op[20, 16], Op[15, 12], Op[9, 5], Op[3, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }
#endif
    }
}
