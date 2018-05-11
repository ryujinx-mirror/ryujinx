//#define Mul

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Mul"), Ignore("Tested: first half of 2018.")]
    public sealed class CpuTestMul : CpuTest
    {
#if Mul
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("MADD <Xd>, <Xn>, <Xm>, <Xa>")]
        public void Madd_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xa)
        {
            uint Opcode = 0x9B000000; // MADD X0, X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X3: Xa, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                AArch64.X((int)Ra, new Bits(Xa));
                Base.Madd(Op[31], Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("MADD <Wd>, <Wn>, <Wm>, <Wa>")]
        public void Madd_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wa)
        {
            uint Opcode = 0x1B000000; // MADD W0, W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Wa, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                AArch64.X((int)Ra, new Bits(Wa));
                Base.Madd(Op[31], Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("MSUB <Xd>, <Xn>, <Xm>, <Xa>")]
        public void Msub_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xa)
        {
            uint Opcode = 0x9B008000; // MSUB X0, X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X3: Xa, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                AArch64.X((int)Ra, new Bits(Xa));
                Base.Msub(Op[31], Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("MSUB <Wd>, <Wn>, <Wm>, <Wa>")]
        public void Msub_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wa)
        {
            uint Opcode = 0x1B008000; // MSUB W0, W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Wa, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                AArch64.X((int)Ra, new Bits(Wa));
                Base.Msub(Op[31], Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("SMADDL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Smaddl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xa)
        {
            uint Opcode = 0x9B200000; // SMADDL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                AArch64.X((int)Ra, new Bits(Xa));
                Base.Smaddl(Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("UMADDL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Umaddl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xa)
        {
            uint Opcode = 0x9BA00000; // UMADDL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                AArch64.X((int)Ra, new Bits(Xa));
                Base.Umaddl(Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("SMSUBL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Smsubl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xa)
        {
            uint Opcode = 0x9B208000; // SMSUBL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                AArch64.X((int)Ra, new Bits(Xa));
                Base.Smsubl(Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("UMSUBL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Umsubl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xa)
        {
            uint Opcode = 0x9BA08000; // UMSUBL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                AArch64.X((int)Ra, new Bits(Xa));
                Base.Umsubl(Op[20, 16], Op[14, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("SMULH <Xd>, <Xn>, <Xm>")]
        public void Smulh_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(16)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(16)] ulong Xm)
        {
            uint Opcode = 0x9B407C00; // SMULH X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Smulh(Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("UMULH <Xd>, <Xn>, <Xm>")]
        public void Umulh_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(16)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(16)] ulong Xm)
        {
            uint Opcode = 0x9BC07C00; // UMULH X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Umulh(Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }
#endif
    }
}
