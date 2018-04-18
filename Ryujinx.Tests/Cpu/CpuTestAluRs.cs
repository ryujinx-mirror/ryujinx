//#define AluRs

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("AluRs"), Ignore("Tested: first half of 2018.")]
    public sealed class CpuTestAluRs : CpuTest
    {
#if AluRs
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("ADC <Xd>, <Xn>, <Xm>")]
        public void Adc_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xm,
                              [Values] bool CarryIn)
        {
            uint Opcode = 0x9A000000; // ADC X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31, Carry: CarryIn);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Shared.PSTATE.C = CarryIn;
                Base.Adc(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("ADC <Wd>, <Wn>, <Wm>")]
        public void Adc_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wm,
                              [Values] bool CarryIn)
        {
            uint Opcode = 0x1A000000; // ADC W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31, Carry: CarryIn);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Shared.PSTATE.C = CarryIn;
                Base.Adc(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("ADCS <Xd>, <Xn>, <Xm>")]
        public void Adcs_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xm,
                               [Values] bool CarryIn)
        {
            uint Opcode = 0xBA000000; // ADCS X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31, Carry: CarryIn);

            AArch64.X((int)Rn, new Bits(Xn));
            AArch64.X((int)Rm, new Bits(Xm));
            Shared.PSTATE.C = CarryIn;
            Base.Adcs(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
            ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

            if (Rd != 31)
            {
                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADCS <Wd>, <Wn>, <Wm>")]
        public void Adcs_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wm,
                               [Values] bool CarryIn)
        {
            uint Opcode = 0x3A000000; // ADCS W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31, Carry: CarryIn);

            AArch64.X((int)Rn, new Bits(Wn));
            AArch64.X((int)Rm, new Bits(Wm));
            Shared.PSTATE.C = CarryIn;
            Base.Adcs(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
            uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

            if (Rd != 31)
            {
                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADD <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Add_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0x8B000000; // ADD X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Add_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("ADD <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Add_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x0B000000; // ADD W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Add_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("ADDS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Adds_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xAB000000; // ADDS X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            AArch64.X((int)Rm, new Bits(Xm));
            Base.Adds_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

            if (Rd != 31)
            {
                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ADDS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Adds_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x2B000000; // ADDS W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            AArch64.X((int)Rm, new Bits(Wm));
            Base.Adds_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

            if (Rd != 31)
            {
                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("AND <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void And_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0x8A000000; // AND X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.And_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("AND <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void And_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x0A000000; // AND W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.And_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("ANDS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Ands_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xEA000000; // ANDS X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            AArch64.X((int)Rm, new Bits(Xm));
            Base.Ands_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

            if (Rd != 31)
            {
                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ANDS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Ands_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x6A000000; // ANDS W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            AArch64.X((int)Rm, new Bits(Wm));
            Base.Ands_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

            if (Rd != 31)
            {
                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("ASRV <Xd>, <Xn>, <Xm>")]
        public void Asrv_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(5)] ulong Xm)
        {
            uint Opcode = 0x9AC02800; // ASRV X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Asrv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("ASRV <Wd>, <Wn>, <Wm>")]
        public void Asrv_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(5)] uint Wm)
        {
            uint Opcode = 0x1AC02800; // ASRV W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Asrv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("BIC <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Bic_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0x8A200000; // BIC X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Bic(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("BIC <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Bic_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x0A200000; // BIC W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Bic(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("BICS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Bics_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xEA200000; // BICS X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            AArch64.X((int)Rm, new Bits(Xm));
            Base.Bics(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

            if (Rd != 31)
            {
                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("BICS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Bics_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x6A200000; // BICS W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            AArch64.X((int)Rm, new Bits(Wm));
            Base.Bics(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

            if (Rd != 31)
            {
                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("CRC32X <Wd>, <Wn>, <Xm>")]
        public void Crc32x([Values(0u, 31u)] uint Rd,
                           [Values(1u, 31u)] uint Rn,
                           [Values(2u, 31u)] uint Rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                           [Values((ulong)0x00_00_00_00_00_00_00_00,
                                   (ulong)0x7F_FF_FF_FF_FF_FF_FF_FF,
                                   (ulong)0x80_00_00_00_00_00_00_00,
                                   (ulong)0xFF_FF_FF_FF_FF_FF_FF_FF)] [Random(64)] ulong Xm)
        {
            uint Opcode = 0x9AC04C00; // CRC32X W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Xm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Crc32(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32W <Wd>, <Wn>, <Wm>")]
        public void Crc32w([Values(0u, 31u)] uint Rd,
                           [Values(1u, 31u)] uint Rn,
                           [Values(2u, 31u)] uint Rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                           [Values((uint)0x00_00_00_00, (uint)0x7F_FF_FF_FF,
                                   (uint)0x80_00_00_00, (uint)0xFF_FF_FF_FF)] [Random(64)] uint Wm)
        {
            uint Opcode = 0x1AC04800; // CRC32W W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Crc32(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32H <Wd>, <Wn>, <Wm>")]
        public void Crc32h([Values(0u, 31u)] uint Rd,
                           [Values(1u, 31u)] uint Rn,
                           [Values(2u, 31u)] uint Rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                           [Values((ushort)0x00_00, (ushort)0x7F_FF,
                                   (ushort)0x80_00, (ushort)0xFF_FF)] [Random(64)] ushort Wm)
        {
            uint Opcode = 0x1AC04400; // CRC32H W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Crc32(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32B <Wd>, <Wn>, <Wm>")]
        public void Crc32b([Values(0u, 31u)] uint Rd,
                           [Values(1u, 31u)] uint Rn,
                           [Values(2u, 31u)] uint Rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                           [Values((byte)0x00, (byte)0x7F,
                                   (byte)0x80, (byte)0xFF)] [Random(64)] byte Wm)
        {
            uint Opcode = 0x1AC04000; // CRC32B W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Crc32(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32CX <Wd>, <Wn>, <Xm>")]
        public void Crc32cx([Values(0u, 31u)] uint Rd,
                            [Values(1u, 31u)] uint Rn,
                            [Values(2u, 31u)] uint Rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                            [Values((ulong)0x00_00_00_00_00_00_00_00,
                                    (ulong)0x7F_FF_FF_FF_FF_FF_FF_FF,
                                    (ulong)0x80_00_00_00_00_00_00_00,
                                    (ulong)0xFF_FF_FF_FF_FF_FF_FF_FF)] [Random(64)] ulong Xm)
        {
            uint Opcode = 0x9AC05C00; // CRC32CX W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Xm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Crc32c(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32CW <Wd>, <Wn>, <Wm>")]
        public void Crc32cw([Values(0u, 31u)] uint Rd,
                            [Values(1u, 31u)] uint Rn,
                            [Values(2u, 31u)] uint Rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                            [Values((uint)0x00_00_00_00, (uint)0x7F_FF_FF_FF,
                                    (uint)0x80_00_00_00, (uint)0xFF_FF_FF_FF)] [Random(64)] uint Wm)
        {
            uint Opcode = 0x1AC05800; // CRC32CW W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Crc32c(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32CH <Wd>, <Wn>, <Wm>")]
        public void Crc32ch([Values(0u, 31u)] uint Rd,
                            [Values(1u, 31u)] uint Rn,
                            [Values(2u, 31u)] uint Rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                            [Values((ushort)0x00_00, (ushort)0x7F_FF,
                                    (ushort)0x80_00, (ushort)0xFF_FF)] [Random(64)] ushort Wm)
        {
            uint Opcode = 0x1AC05400; // CRC32CH W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Crc32c(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("CRC32CB <Wd>, <Wn>, <Wm>")]
        public void Crc32cb([Values(0u, 31u)] uint Rd,
                            [Values(1u, 31u)] uint Rn,
                            [Values(2u, 31u)] uint Rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                            [Values((byte)0x00, (byte)0x7F,
                                    (byte)0x80, (byte)0xFF)] [Random(64)] byte Wm)
        {
            uint Opcode = 0x1AC05000; // CRC32CB W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Crc32c(Op[31], Op[20, 16], Op[11, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("EON <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Eon_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xCA200000; // EON X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Eon(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("EON <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Eon_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x4A200000; // EON W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Eon(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("EOR <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Eor_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xCA000000; // EOR X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Eor_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("EOR <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Eor_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x4A000000; // EOR W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Eor_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("EXTR <Xd>, <Xn>, <Xm>, #<lsb>")]
        public void Extr_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xm,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint lsb)
        {
            uint Opcode = 0x93C00000; // EXTR X0, X0, X0, #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((lsb & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Extr(Op[31], Op[22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("EXTR <Wd>, <Wn>, <Wm>, #<lsb>")]
        public void Extr_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wm,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint lsb)
        {
            uint Opcode = 0x13800000; // EXTR W0, W0, W0, #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((lsb & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Extr(Op[31], Op[22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("LSLV <Xd>, <Xn>, <Xm>")]
        public void Lslv_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(5)] ulong Xm)
        {
            uint Opcode = 0x9AC02000; // LSLV X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Lslv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("LSLV <Wd>, <Wn>, <Wm>")]
        public void Lslv_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(5)] uint Wm)
        {
            uint Opcode = 0x1AC02000; // LSLV W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Lslv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("LSRV <Xd>, <Xn>, <Xm>")]
        public void Lsrv_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(5)] ulong Xm)
        {
            uint Opcode = 0x9AC02400; // LSRV X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Lsrv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("LSRV <Wd>, <Wn>, <Wm>")]
        public void Lsrv_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(5)] uint Wm)
        {
            uint Opcode = 0x1AC02400; // LSRV W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Lsrv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("ORN <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Orn_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xAA200000; // ORN X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Orn(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("ORN <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Orn_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x2A200000; // ORN W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Orn(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("ORR <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Orr_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xAA000000; // ORR X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Orr_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("ORR <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Orr_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x2A000000; // ORR W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Orr_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("RORV <Xd>, <Xn>, <Xm>")]
        public void Rorv_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(5)] ulong Xm)
        {
            uint Opcode = 0x9AC02C00; // RORV X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Rorv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("RORV <Wd>, <Wn>, <Wm>")]
        public void Rorv_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(5)] uint Wm)
        {
            uint Opcode = 0x1AC02C00; // RORV W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Rorv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("SBC <Xd>, <Xn>, <Xm>")]
        public void Sbc_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xm,
                              [Values] bool CarryIn)
        {
            uint Opcode = 0xDA000000; // SBC X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31, Carry: CarryIn);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Shared.PSTATE.C = CarryIn;
                Base.Sbc(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("SBC <Wd>, <Wn>, <Wm>")]
        public void Sbc_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wm,
                              [Values] bool CarryIn)
        {
            uint Opcode = 0x5A000000; // SBC W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31, Carry: CarryIn);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Shared.PSTATE.C = CarryIn;
                Base.Sbc(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("SBCS <Xd>, <Xn>, <Xm>")]
        public void Sbcs_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(4)] ulong Xm,
                               [Values] bool CarryIn)
        {
            uint Opcode = 0xFA000000; // SBCS X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31, Carry: CarryIn);

            AArch64.X((int)Rn, new Bits(Xn));
            AArch64.X((int)Rm, new Bits(Xm));
            Shared.PSTATE.C = CarryIn;
            Base.Sbcs(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
            ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

            if (Rd != 31)
            {
                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SBCS <Wd>, <Wn>, <Wm>")]
        public void Sbcs_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(4)] uint Wm,
                               [Values] bool CarryIn)
        {
            uint Opcode = 0x7A000000; // SBCS W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31, Carry: CarryIn);

            AArch64.X((int)Rn, new Bits(Wn));
            AArch64.X((int)Rm, new Bits(Wm));
            Shared.PSTATE.C = CarryIn;
            Base.Sbcs(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
            uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

            if (Rd != 31)
            {
                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SDIV <Xd>, <Xn>, <Xm>")]
        public void Sdiv_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xm)
        {
            uint Opcode = 0x9AC00C00; // SDIV X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Sdiv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("SDIV <Wd>, <Wn>, <Wm>")]
        public void Sdiv_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wm)
        {
            uint Opcode = 0x1AC00C00; // SDIV W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Sdiv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("SUB <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Sub_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xCB000000; // SUB X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Sub_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("SUB <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Sub_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(2u, 31u)] uint Rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x4B000000; // SUB W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Sub_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
        }

        [Test, Description("SUBS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Subs_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(1)] ulong Xm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 1)] uint amount)
        {
            uint Opcode = 0xEB000000; // SUBS X0, X0, X0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            AArch64.X((int)Rm, new Bits(Xm));
            Base.Subs_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

            if (Rd != 31)
            {
                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("SUBS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Subs_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(1)] uint Wm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 1)] uint amount)
        {
            uint Opcode = 0x6B000000; // SUBS W0, W0, W0, LSL #0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            AArch64.X((int)Rm, new Bits(Wm));
            Base.Subs_Rs(Op[31], Op[23, 22], Op[20, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
            uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

            if (Rd != 31)
            {
                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
        }

        [Test, Description("UDIV <Xd>, <Xn>, <Xm>")]
        public void Udiv_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xm)
        {
            uint Opcode = 0x9AC00800; // UDIV X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Xn));
                AArch64.X((int)Rm, new Bits(Xm));
                Base.Udiv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
        }

        [Test, Description("UDIV <Wd>, <Wn>, <Wm>")]
        public void Udiv_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wm)
        {
            uint Opcode = 0x1AC00800; // UDIV W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rn, new Bits(Wn));
                AArch64.X((int)Rm, new Bits(Wm));
                Base.Udiv(Op[31], Op[20, 16], Op[9, 5], Op[4, 0]);
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
