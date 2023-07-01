#define Mul

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Mul")]
    public sealed class CpuTestMul : CpuTest
    {
#if Mul
        [Test, Pairwise, Description("MADD <Xd>, <Xn>, <Xm>, <Xa>")]
        public void Madd_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(3u, 31u)] uint ra,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xa)
        {
            uint opcode = 0x9B000000; // MADD X0, X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x3: xa, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MADD <Wd>, <Wn>, <Wm>, <Wa>")]
        public void Madd_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(3u, 31u)] uint ra,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wa)
        {
            uint opcode = 0x1B000000; // MADD W0, W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x3: wa, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MSUB <Xd>, <Xn>, <Xm>, <Xa>")]
        public void Msub_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(3u, 31u)] uint ra,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xa)
        {
            uint opcode = 0x9B008000; // MSUB X0, X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x3: xa, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MSUB <Wd>, <Wn>, <Wm>, <Wa>")]
        public void Msub_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(3u, 31u)] uint ra,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] uint wa)
        {
            uint opcode = 0x1B008000; // MSUB W0, W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x3: wa, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMADDL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Smaddl_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(3u, 31u)] uint ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xa)
        {
            uint opcode = 0x9B200000; // SMADDL X0, W0, W0, X0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: wn, x2: wm, x3: xa, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMADDL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Umaddl_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(3u, 31u)] uint ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xa)
        {
            uint opcode = 0x9BA00000; // UMADDL X0, W0, W0, X0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: wn, x2: wm, x3: xa, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMSUBL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Smsubl_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(3u, 31u)] uint ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xa)
        {
            uint opcode = 0x9B208000; // SMSUBL X0, W0, W0, X0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: wn, x2: wm, x3: xa, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMSUBL <Xd>, <Wn>, <Wm>, <Xa>")]
        public void Umsubl_64bit([Values(0u, 31u)] uint rd,
                                 [Values(1u, 31u)] uint rn,
                                 [Values(2u, 31u)] uint rm,
                                 [Values(3u, 31u)] uint ra,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wn,
                                 [Values(0x00000000u, 0x7FFFFFFFu,
                                         0x80000000u, 0xFFFFFFFFu)] uint wm,
                                 [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                         0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xa)
        {
            uint opcode = 0x9BA08000; // UMSUBL X0, W0, W0, X0
            opcode |= ((rm & 31) << 16) | ((ra & 31) << 10) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: wn, x2: wm, x3: xa, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SMULH <Xd>, <Xn>, <Xm>")]
        public void Smulh_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm)
        {
            uint opcode = 0x9B407C00; // SMULH X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UMULH <Xd>, <Xn>, <Xm>")]
        public void Umulh_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(2u, 31u)] uint rm,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] ulong xm)
        {
            uint opcode = 0x9BC07C00; // UMULH X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
