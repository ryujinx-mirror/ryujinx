#define Bfm

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Bfm")]
    public sealed class CpuTestBfm : CpuTest
    {
#if Bfm
        private const int RndCnt = 2;

        [Test, Pairwise, Description("BFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Bfm_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Random(RndCnt)] ulong xd,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                              [Values(0u, 31u, 32u, 63u)] uint immr,
                              [Values(0u, 31u, 32u, 63u)] uint imms)
        {
            uint opcode = 0xB3400000; // BFM X0, X0, #0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x0: xd, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Bfm_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Random(RndCnt)] uint wd,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] uint wn,
                              [Values(0u, 15u, 16u, 31u)] uint immr,
                              [Values(0u, 15u, 16u, 31u)] uint imms)
        {
            uint opcode = 0x33000000; // BFM W0, W0, #0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x0: wd, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Sbfm_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0u, 31u, 32u, 63u)] uint immr,
                               [Values(0u, 31u, 32u, 63u)] uint imms)
        {
            uint opcode = 0x93400000; // SBFM X0, X0, #0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Sbfm_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0u, 15u, 16u, 31u)] uint immr,
                               [Values(0u, 15u, 16u, 31u)] uint imms)
        {
            uint opcode = 0x13000000; // SBFM W0, W0, #0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UBFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Ubfm_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0u, 31u, 32u, 63u)] uint immr,
                               [Values(0u, 31u, 32u, 63u)] uint imms)
        {
            uint opcode = 0xD3400000; // UBFM X0, X0, #0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UBFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Ubfm_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0u, 15u, 16u, 31u)] uint immr,
                               [Values(0u, 15u, 16u, 31u)] uint imms)
        {
            uint opcode = 0x53000000; // UBFM W0, W0, #0, #0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);
            opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
