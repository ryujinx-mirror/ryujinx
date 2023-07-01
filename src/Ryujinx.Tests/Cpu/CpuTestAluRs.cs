#define AluRs

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("AluRs")]
    public sealed class CpuTestAluRs : CpuTest
    {
#if AluRs

        [Test, Pairwise, Description("ADC <Xd>, <Xn>, <Xm>")]
        public void Adc_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values] bool carryIn)
        {
            uint opcode = 0x9A000000; // ADC X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADC <Wd>, <Wn>, <Wm>")]
        public void Adc_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values] bool carryIn)
        {
            uint opcode = 0x1A000000; // ADC W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADCS <Xd>, <Xn>, <Xm>")]
        public void Adcs_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values] bool carryIn)
        {
            uint opcode = 0xBA000000; // ADCS X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADCS <Wd>, <Wn>, <Wm>")]
        public void Adcs_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values] bool carryIn)
        {
            uint opcode = 0x3A000000; // ADCS W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Add_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0x8B000000; // ADD X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADD <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Add_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x0B000000; // ADD W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Adds_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xAB000000; // ADDS X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ADDS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Adds_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x2B000000; // ADDS W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void And_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0x8A000000; // AND X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("AND <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void And_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x0A000000; // AND W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ANDS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Ands_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xEA000000; // ANDS X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ANDS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Ands_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x6A000000; // ANDS W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ASRV <Xd>, <Xn>, <Xm>")]
        public void Asrv_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm)
        {
            uint opcode = 0x9AC02800; // ASRV X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ASRV <Wd>, <Wn>, <Wm>")]
        public void Asrv_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm)
        {
            uint opcode = 0x1AC02800; // ASRV W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIC <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Bic_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0x8A200000; // BIC X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BIC <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Bic_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x0A200000; // BIC W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BICS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Bics_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xEA200000; // BICS X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BICS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Bics_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                               [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x6A200000; // BICS W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EON <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Eon_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xCA200000; // EON X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EON <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Eon_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x4A200000; // EON W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Eor_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xCA000000; // EOR X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EOR <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Eor_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x4A000000; // EOR W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EXTR <Xd>, <Xn>, <Xm>, #<lsb>")]
        public void Extr_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0u, 31u, 32u, 63u)] uint lsb)
        {
            uint opcode = 0x93C00000; // EXTR X0, X0, X0, #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((lsb & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("EXTR <Wd>, <Wn>, <Wm>, #<lsb>")]
        public void Extr_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0u, 15u, 16u, 31u)] uint lsb)
        {
            uint opcode = 0x13800000; // EXTR W0, W0, W0, #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((lsb & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("LSLV <Xd>, <Xn>, <Xm>")]
        public void Lslv_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm)
        {
            uint opcode = 0x9AC02000; // LSLV X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("LSLV <Wd>, <Wn>, <Wm>")]
        public void Lslv_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm)
        {
            uint opcode = 0x1AC02000; // LSLV W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("LSRV <Xd>, <Xn>, <Xm>")]
        public void Lsrv_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm)
        {
            uint opcode = 0x9AC02400; // LSRV X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("LSRV <Wd>, <Wn>, <Wm>")]
        public void Lsrv_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm)
        {
            uint opcode = 0x1AC02400; // LSRV W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORN <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Orn_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xAA200000; // ORN X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORN <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Orn_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x2A200000; // ORN W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Orr_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xAA000000; // ORR X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("ORR <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Orr_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint shift, // <LSL, LSR, ASR, ROR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x2A000000; // ORR W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RORV <Xd>, <Xn>, <Xm>")]
        public void Rorv_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0ul, 31ul, 32ul, 63ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm)
        {
            uint opcode = 0x9AC02C00; // RORV X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RORV <Wd>, <Wn>, <Wm>")]
        public void Rorv_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0u, 15u, 16u, 31u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm)
        {
            uint opcode = 0x1AC02C00; // RORV W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBC <Xd>, <Xn>, <Xm>")]
        public void Sbc_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values] bool carryIn)
        {
            uint opcode = 0xDA000000; // SBC X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBC <Wd>, <Wn>, <Wm>")]
        public void Sbc_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values] bool carryIn)
        {
            uint opcode = 0x5A000000; // SBC W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBCS <Xd>, <Xn>, <Xm>")]
        public void Sbcs_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values] bool carryIn)
        {
            uint opcode = 0xFA000000; // SBCS X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBCS <Wd>, <Wn>, <Wm>")]
        public void Sbcs_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values] bool carryIn)
        {
            uint opcode = 0x7A000000; // SBCS W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31, carry: carryIn);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Sub_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xCB000000; // SUB X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUB <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Sub_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(2u, 31u)] uint rm,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wm,
                              [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                              [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x4B000000; // SUB W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Xd>, <Xn>, <Xm>{, <shift> #<amount>}")]
        public void Subs_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 31u, 32u, 63u)] uint amount)
        {
            uint opcode = 0xEB000000; // SUBS X0, X0, X0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SUBS <Wd>, <Wn>, <Wm>{, <shift> #<amount>}")]
        public void Subs_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0b00u, 0b01u, 0b10u)] uint shift, // <LSL, LSR, ASR>
                               [Values(0u, 15u, 16u, 31u)] uint amount)
        {
            uint opcode = 0x6B000000; // SUBS W0, W0, W0, LSL #0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((shift & 3) << 22) | ((amount & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
