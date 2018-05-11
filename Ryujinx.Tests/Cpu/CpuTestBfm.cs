//#define Bfm

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Bfm"), Ignore("Tested: first half of 2018.")]
    public sealed class CpuTestBfm : CpuTest
    {
#if Bfm
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("BFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Bfm_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Random(2)] ulong _Xd,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr,
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint imms)
        {
            uint Opcode = 0xB3400000; // BFM X0, X0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X0: _Xd, X1: Xn, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rd, new Bits(_Xd));
                AArch64.X((int)Rn, new Bits(Xn));
                Base.Bfm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("BFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Bfm_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Random(2)] uint _Wd,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr,
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint imms)
        {
            uint Opcode = 0x33000000; // BFM W0, W0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X0: _Wd, X1: Wn, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rd, new Bits(_Wd));
                AArch64.X((int)Rn, new Bits(Wn));
                Base.Bfm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("SBFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Sbfm_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint imms)
        {
            uint Opcode = 0x93400000; // SBFM X0, X0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                Base.Sbfm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("SBFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Sbfm_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint imms)
        {
            uint Opcode = 0x13000000; // SBFM W0, W0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                Base.Sbfm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("UBFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Ubfm_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint imms)
        {
            uint Opcode = 0xD3400000; // UBFM X0, X0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                Base.Ubfm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("UBFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Ubfm_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint imms)
        {
            uint Opcode = 0x53000000; // UBFM W0, W0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                Base.Ubfm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }
#endif
    }
}
