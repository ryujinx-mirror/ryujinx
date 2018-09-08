#define Mul

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Mul")] // Tested: second half of 2018.
    public sealed class CpuTestMul : CpuTest
    {
#if Mul
        private const int RndCnt = 2;

        [Test, Pairwise, Description("MADD <Xd>, <Xn>, <Xm>, <Xa>")]
        public void Madd_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xa)
        {
            uint Opcode = 0x9B000000; // MADD X0, X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X3: Xa, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MADD <Wd>, <Wn>, <Wm>, <Wa>")]
        public void Madd_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wa)
        {
            uint Opcode = 0x1B000000; // MADD W0, W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Wa, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MSUB <Xd>, <Xn>, <Xm>, <Xa>")]
        public void Msub_64bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xa)
        {
            uint Opcode = 0x9B008000; // MSUB X0, X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X3: Xa, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MSUB <Wd>, <Wn>, <Wm>, <Wa>")]
        public void Msub_32bit([Values(0u, 31u)] uint Rd,
                               [Values(1u, 31u)] uint Rn,
                               [Values(2u, 31u)] uint Rm,
                               [Values(3u, 31u)] uint Ra,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wa)
        {
            uint Opcode = 0x1B008000; // MSUB W0, W0, W0, W0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Wa, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMADDL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Smaddl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xa)
        {
            uint Opcode = 0x9B200000; // SMADDL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMADDL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Umaddl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xa)
        {
            uint Opcode = 0x9BA00000; // UMADDL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMSUBL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Smsubl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xa)
        {
            uint Opcode = 0x9B208000; // SMSUBL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMSUBL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Umsubl_64bit([Values(0u, 31u)] uint Rd,
                                 [Values(1u, 31u)] uint Rn,
                                 [Values(2u, 31u)] uint Rm,
                                 [Values(3u, 31u)] uint Ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint Wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xa)
        {
            uint Opcode = 0x9BA08000; // UMSUBL X0, W0, W0, X0
            Opcode |= ((Rm & 31) << 16) | ((Ra & 31) << 10) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Wn, X2: Wm, X3: Xa, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMULH <Xd>, <Xn>, <Xm>")]
        public void Smulh_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm)
        {
            uint Opcode = 0x9B407C00; // SMULH X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMULH <Xd>, <Xn>, <Xm>")]
        public void Umulh_64bit([Values(0u, 31u)] uint Rd,
                                [Values(1u, 31u)] uint Rn,
                                [Values(2u, 31u)] uint Rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong Xm)
        {
            uint Opcode = 0x9BC07C00; // UMULH X0, X0, X0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            AThreadState ThreadState = SingleOpcode(Opcode, X1: Xn, X2: Xm, X31: _X31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
