#define AluImm

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("AluImm")] // Tested: second half of 2018.
    public sealed class CpuTestAluImm : CpuTest
    {
#if AluImm
        private const int RndCnt     = 2;
        private const int RndCntImm  = 2;
        private const int RndCntImms = 2;
        private const int RndCntImmr = 2;

        [Test, Pairwise, Description("ADD <Xd|SP>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Add_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn_SP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x91000000; // ADD X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Wd|WSP>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Add_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn_WSP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x11000000; // ADD W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Xd>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Adds_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn_SP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0xB1000000; // ADDS X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Wd>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Adds_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn_WSP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x31000000; // ADDS W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Xd|SP>, <Xn>, #<imm>")]
        public void And_N1_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                 [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, RndCntImms)] uint imms, // <imm>
                                 [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0x92400000; // AND X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Xd|SP>, <Xn>, #<imm>")]
        public void And_N0_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                 [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                                 [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0x92000000; // AND X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Wd|WSP>, <Wn>, #<imm>")]
        public void And_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                              [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0x12000000; // AND W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ANDS <Xd>, <Xn>, #<imm>")]
        public void Ands_N1_64bit([Values(0u, 31u)] uint Rd,
                                  [Values(1u, 31u)] uint Rn,
                                  [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                          0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                  [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, RndCntImms)] uint imms, // <imm>
                                  [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0xF2400000; // ANDS X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ANDS <Xd>, <Xn>, #<imm>")]
        public void Ands_N0_64bit([Values(0u, 31u)] uint Rd,
                                  [Values(1u, 31u)] uint Rn,
                                  [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                          0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                  [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                                  [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0xF2000000; // ANDS X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ANDS <Wd>, <Wn>, #<imm>")]
        public void Ands_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0x72000000; // ANDS W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Xd|SP>, <Xn>, #<imm>")]
        public void Eor_N1_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                 [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, RndCntImms)] uint imms, // <imm>
                                 [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0xD2400000; // EOR X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Xd|SP>, <Xn>, #<imm>")]
        public void Eor_N0_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                 [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                                 [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0xD2000000; // EOR X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Wd>, <Wn>, #<imm>")]
        public void Eor_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                              [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0x52000000; // EOR W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Xd|SP>, <Xn>, #<imm>")]
        public void Orr_N1_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                 [Values(0u, 31u, 32u, 62u)] [Random(0u, 62u, RndCntImms)] uint imms, // <imm>
                                 [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0xB2400000; // ORR X0, X0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Xd|SP>, <Xn>, #<imm>")]
        public void Orr_N0_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                 [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                                 [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0xB2000000; // ORR X0, X0, #0x100000001
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Wd|WSP>, <Wn>, #<imm>")]
        public void Orr_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                              [Values(0u, 15u, 16u, 30u)] [Random(0u, 30u, RndCntImms)] uint imms, // <imm>
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr) // <imm>
        {
            uint Opcode = 0x32000000; // ORR W0, W0, #0x1
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Xd|SP>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Sub_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn_SP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0xD1000000; // SUB X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Wd|WSP>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Sub_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn_WSP,
                              [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                              [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x51000000; // SUB W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Xd>, <Xn|SP>, #<imm>{, <shift>}")]
        public void Subs_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn_SP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0xF1000000; // SUBS X0, X0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Xn_SP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Xn_SP);
            }

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Wd>, <Wn|WSP>, #<imm>{, <shift>}")]
        public void Subs_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn_WSP,
                               [Values(0u, 4095u)] [Random(0u, 4095u, RndCntImm)] uint imm,
                               [Values(0b00u, 0b01u)] uint shift) // <LSL #0, LSL #12>
        {
            uint Opcode = 0x71000000; // SUBS W0, W0, #0, LSL #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((shift & 3) << 22) | ((imm & 4095) << 10);

            CpuThreadState ThreadState;

            if (Rn != 31)
            {
                ThreadState = SingleOpcode(Opcode, X1: Wn_WSP);
            }
            else
            {
                ThreadState = SingleOpcode(Opcode, X31: Wn_WSP);
            }

            CompareAgainstUnicorn();
        }
#endif
    }
}
