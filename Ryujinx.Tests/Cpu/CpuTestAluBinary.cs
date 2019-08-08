#define AluBinary

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("AluBinary")]
    public sealed class CpuTestAluBinary : CpuTest
    {
#if AluBinary
        private const int RndCnt = 2;

        [Test, Pairwise, Description("CRC32X <Wd>, <Wn>, <Xm>"), Ignore("Unicorn fails.")]
        public void Crc32x([Values(0u, 31u)] uint rd,
                           [Values(1u, 31u)] uint rn,
                           [Values(2u, 31u)] uint rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                           [Values((ulong)0x00_00_00_00_00_00_00_00,
                                   (ulong)0x7F_FF_FF_FF_FF_FF_FF_FF,
                                   (ulong)0x80_00_00_00_00_00_00_00,
                                   (ulong)0xFF_FF_FF_FF_FF_FF_FF_FF)] [Random(RndCnt)] ulong xm)
        {
            uint opcode = 0x9AC04C00; // CRC32X W0, W0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: xm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32W <Wd>, <Wn>, <Wm>"), Ignore("Unicorn fails.")]
        public void Crc32w([Values(0u, 31u)] uint rd,
                           [Values(1u, 31u)] uint rn,
                           [Values(2u, 31u)] uint rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                           [Values((uint)0x00_00_00_00, (uint)0x7F_FF_FF_FF,
                                   (uint)0x80_00_00_00, (uint)0xFF_FF_FF_FF)] [Random(RndCnt)] uint wm)
        {
            uint opcode = 0x1AC04800; // CRC32W W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32H <Wd>, <Wn>, <Wm>"), Ignore("Unicorn fails.")]
        public void Crc32h([Values(0u, 31u)] uint rd,
                           [Values(1u, 31u)] uint rn,
                           [Values(2u, 31u)] uint rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                           [Values((ushort)0x00_00, (ushort)0x7F_FF,
                                   (ushort)0x80_00, (ushort)0xFF_FF)] [Random(RndCnt)] ushort wm)
        {
            uint opcode = 0x1AC04400; // CRC32H W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32B <Wd>, <Wn>, <Wm>"), Ignore("Unicorn fails.")]
        public void Crc32b([Values(0u, 31u)] uint rd,
                           [Values(1u, 31u)] uint rn,
                           [Values(2u, 31u)] uint rm,
                           [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                           [Values((byte)0x00, (byte)0x7F,
                                   (byte)0x80, (byte)0xFF)] [Random(RndCnt)] byte wm)
        {
            uint opcode = 0x1AC04000; // CRC32B W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32CX <Wd>, <Wn>, <Xm>")]
        public void Crc32cx([Values(0u, 31u)] uint rd,
                            [Values(1u, 31u)] uint rn,
                            [Values(2u, 31u)] uint rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                            [Values((ulong)0x00_00_00_00_00_00_00_00,
                                    (ulong)0x7F_FF_FF_FF_FF_FF_FF_FF,
                                    (ulong)0x80_00_00_00_00_00_00_00,
                                    (ulong)0xFF_FF_FF_FF_FF_FF_FF_FF)] [Random(RndCnt)] ulong xm)
        {
            uint opcode = 0x9AC05C00; // CRC32CX W0, W0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: xm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32CW <Wd>, <Wn>, <Wm>")]
        public void Crc32cw([Values(0u, 31u)] uint rd,
                            [Values(1u, 31u)] uint rn,
                            [Values(2u, 31u)] uint rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                            [Values((uint)0x00_00_00_00, (uint)0x7F_FF_FF_FF,
                                    (uint)0x80_00_00_00, (uint)0xFF_FF_FF_FF)] [Random(RndCnt)] uint wm)
        {
            uint opcode = 0x1AC05800; // CRC32CW W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32CH <Wd>, <Wn>, <Wm>")]
        public void Crc32ch([Values(0u, 31u)] uint rd,
                            [Values(1u, 31u)] uint rn,
                            [Values(2u, 31u)] uint rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                            [Values((ushort)0x00_00, (ushort)0x7F_FF,
                                    (ushort)0x80_00, (ushort)0xFF_FF)] [Random(RndCnt)] ushort wm)
        {
            uint opcode = 0x1AC05400; // CRC32CH W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CRC32CB <Wd>, <Wn>, <Wm>")]
        public void Crc32cb([Values(0u, 31u)] uint rd,
                            [Values(1u, 31u)] uint rn,
                            [Values(2u, 31u)] uint rm,
                            [Values(0x00000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                            [Values((byte)0x00, (byte)0x7F,
                                    (byte)0x80, (byte)0xFF)] [Random(RndCnt)] byte wm)
        {
            uint opcode = 0x1AC05000; // CRC32CB W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SDIV <Xd>, <Xn>, <Xm>")]
        public void Sdiv_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xm)
        {
            uint opcode = 0x9AC00C00; // SDIV X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("SDIV <Wd>, <Wn>, <Wm>")]
        public void Sdiv_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wm)
        {
            uint opcode = 0x1AC00C00; // SDIV W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UDIV <Xd>, <Xn>, <Xm>")]
        public void Udiv_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xm)
        {
            uint opcode = 0x9AC00800; // UDIV X0, X0, X0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x2: xm, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("UDIV <Wd>, <Wn>, <Wm>")]
        public void Udiv_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(2u, 31u)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wm)
        {
            uint opcode = 0x1AC00800; // UDIV W0, W0, W0
            opcode |= ((rm & 31) << 16) | ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x2: wm, x31: w31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
