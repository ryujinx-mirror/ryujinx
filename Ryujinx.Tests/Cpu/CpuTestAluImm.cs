//#define AluImm

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("AluImm"), Ignore("Tested: second half of 2018.")]
    public sealed class CpuTestAluImm : CpuTest
    {
#if AluImm
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("ADD <Xd|SP>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Add_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn_SP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x91000000; // ADD X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);

                AArch64.SP(new Bits(Xn_SP));
            }

            Base.Add_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("ADD <Wd|WSP>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Add_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn_WSP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x11000000; // ADD W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);

                AArch64.SP(new Bits(Wn_WSP));
            }

            Base.Add_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("ADDS <Xd>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Adds_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn_SP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0xB1000000; // ADDS X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);

                AArch64.SP(new Bits(Xn_SP));
            }

            Base.Adds_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
            CompareAgainstUnicorn();
        }

        [Test, Description("ADDS <Wd>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Adds_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn_WSP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x31000000; // ADDS W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);

                AArch64.SP(new Bits(Wn_WSP));
            }

            Base.Adds_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
            CompareAgainstUnicorn();
        }

        [Test, Description("AND <Xd|SP>, <Xn>, #<imm>")]
        public void And_N1_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                 [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, 2)] uint imms, // <imm>
                                 [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0x92400000; // AND X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.And_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("AND <Xd|SP>, <Xn>, #<imm>")]
        public void And_N0_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                 [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                                 [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0x92000000; // AND X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.And_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("AND <Wd|WSP>, <Wn>, #<imm>")]
        public void And_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                              [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0x12000000; // AND W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            Base.And_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("ANDS <Xd>, <Xn>, #<imm>")]
        public void Ands_N1_64bit([Values(0u, 31u)] uint Rd,
                                  [Values(1u, 31u)] uint Rn,
                                  [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                          0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                  [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, 2)] uint imms, // <imm>
                                  [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0xF2400000; // ANDS X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Ands_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
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
            CompareAgainstUnicorn();
        }

        [Test, Description("ANDS <Xd>, <Xn>, #<imm>")]
        public void Ands_N0_64bit([Values(0u, 31u)] uint Rd,
                                  [Values(1u, 31u)] uint Rn,
                                  [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                          0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                  [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                                  [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0xF2000000; // ANDS X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Ands_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
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
            CompareAgainstUnicorn();
        }

        [Test, Description("ANDS <Wd>, <Wn>, #<imm>")]
        public void Ands_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                               [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0x72000000; // ANDS W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            Base.Ands_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);
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
            CompareAgainstUnicorn();
        }

        [Test, Description("EOR <Xd|SP>, <Xn>, #<imm>")]
        public void Eor_N1_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                 [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, 2)] uint imms, // <imm>
                                 [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0xD2400000; // EOR X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Eor_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("EOR <Xd|SP>, <Xn>, #<imm>")]
        public void Eor_N0_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                 [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                                 [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0xD2000000; // EOR X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Eor_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("EOR <Wd>, <Wn>, #<imm>")]
        public void Eor_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                              [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0x52000000; // EOR W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            Base.Eor_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("ORR <Xd|SP>, <Xn>, #<imm>")]
        public void Orr_N1_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                 [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, 2)] uint imms, // <imm>
                                 [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0xB2400000; // ORR X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Orr_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("ORR <Xd|SP>, <Xn>, #<imm>")]
        public void Orr_N0_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(2)] ulong Xn,
                                 [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                                 [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0xB2000000; // ORR X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            AArch64.X((int)Rn, new Bits(Xn));
            Base.Orr_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("ORR <Wd|WSP>, <Wn>, #<imm>")]
        public void Orr_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(2)] uint Wn,
                              [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, 2)] uint imms, // <imm>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, 2)] uint immr) // <imm>
        {
            uint Opcode = 0x32000000; // ORR W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);
            Bits Op = new Bits(Opcode);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            AArch64.X((int)Rn, new Bits(Wn));
            Base.Orr_Imm(Op[31], Op[22], Op[21, 16], Op[15, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("SUB <Xd|SP>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Sub_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn_SP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0xD1000000; // SUB X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);

                AArch64.SP(new Bits(Xn_SP));
            }

            Base.Sub_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong SP = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(SP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("SUB <Wd|WSP>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Sub_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn_WSP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x51000000; // SUB W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);

                AArch64.SP(new Bits(Wn_WSP));
            }

            Base.Sub_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint WSP = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(WSP));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("SUBS <Xd>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Subs_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(8)] ulong Xn_SP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0xF1000000; // SUBS X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);

                AArch64.X((int)Rn, new Bits(Xn_SP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);

                AArch64.SP(new Bits(Xn_SP));
            }

            Base.Subs_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                ulong _X31 = AArch64.SP(64).ToUInt64();

                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
            CompareAgainstUnicorn();
        }

        [Test, Description("SUBS <Wd>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Subs_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(8)] uint Wn_WSP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, 10)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x71000000; // SUBS W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);
            Bits Op = new Bits(Opcode);

            AThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);

                AArch64.X((int)Rn, new Bits(Wn_WSP));
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);

                AArch64.SP(new Bits(Wn_WSP));
            }

            Base.Subs_Imm(Op[31], Op[23, 22], Op[21, 10], Op[9, 5], Op[4, 0]);

            if (Rd != 31)
            {
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                uint _W31 = AArch64.SP(32).ToUInt32();

                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.Negative, Is.EqualTo(Shared.PSTATE.N));
                Assert.That(ThreadState.Zero,     Is.EqualTo(Shared.PSTATE.Z));
                Assert.That(ThreadState.Carry,    Is.EqualTo(Shared.PSTATE.C));
                Assert.That(ThreadState.Overflow, Is.EqualTo(Shared.PSTATE.V));
            });
            CompareAgainstUnicorn();
        }
#endif
    }
}
