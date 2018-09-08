#define Bfm

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Bfm")] // Tested: second half of 2018.
    public sealed class CpuTestBfm : CpuTest
    {
#if Bfm
        private const int RndCnt     = 2;
        private const int RndCntImmr = 2;
        private const int RndCntImms = 2;

        [Test, Pairwise, Description("BFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Bfm_64bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Random(RndCnt)] ulong _Xd,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr,
                              [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImms)] uint imms)
        {
            uint Opcode = 0xB3400000; // BFM X0, X0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X0: _Xd, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("BFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Bfm_32bit([Values(0u, 31u)] uint Rd,
                              [Values(1u, 31u)] uint Rn,
                              [Random(RndCnt)] uint _Wd,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr,
                              [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImms)] uint imms)
        {
            uint Opcode = 0x33000000; // BFM W0, W0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X0: _Wd, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Sbfm_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImms)] uint imms)
        {
            uint Opcode = 0x93400000; // SBFM X0, X0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SBFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Sbfm_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImms)] uint imms)
        {
            uint Opcode = 0x13000000; // SBFM W0, W0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UBFM <Xd>, <Xn>, #<immr>, #<imms>")]
        public void Ubfm_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImmr)] uint immr,
                               [Values(0u, 31u, 32u, 63u)] [Random(0u, 63u, RndCntImms)] uint imms)
        {
            uint Opcode = 0xD3400000; // UBFM X0, X0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UBFM <Wd>, <Wn>, #<immr>, #<imms>")]
        public void Ubfm_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImmr)] uint immr,
                               [Values(0u, 15u, 16u, 31u)] [Random(0u, 31u, RndCntImms)] uint imms)
        {
            uint Opcode = 0x53000000; // UBFM W0, W0, #0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((immr & 63) << 16) | ((imms & 63) << 10);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X31: _W31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
