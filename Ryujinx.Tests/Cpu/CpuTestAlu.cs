#define Alu

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Alu")]
    public sealed class CpuTestAlu : CpuTest
    {
#if Alu
        private const int RndCnt = 2;

        [Test, Pairwise, Description("CLS <Xd>, <Xn>")]
        public void Cls_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0xDAC01400; // CLS X0, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLS <Wd>, <Wn>")]
        public void Cls_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            uint opcode = 0x5AC01400; // CLS W0, W0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Xd>, <Xn>")]
        public void Clz_64bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                      0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0xDAC01000; // CLZ X0, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("CLZ <Wd>, <Wn>")]
        public void Clz_32bit([Values(0u, 31u)] uint rd,
                              [Values(1u, 31u)] uint rn,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            uint opcode = 0x5AC01000; // CLZ W0, W0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Xd>, <Xn>")]
        public void Rbit_64bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                       0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0xDAC00000; // RBIT X0, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("RBIT <Wd>, <Wn>")]
        public void Rbit_32bit([Values(0u, 31u)] uint rd,
                               [Values(1u, 31u)] uint rn,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            uint opcode = 0x5AC00000; // RBIT W0, W0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Xd>, <Xn>")]
        public void Rev16_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0xDAC00400; // REV16 X0, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV16 <Wd>, <Wn>")]
        public void Rev16_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            uint opcode = 0x5AC00400; // REV16 W0, W0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV32 <Xd>, <Xn>")]
        public void Rev32_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0xDAC00800; // REV32 X0, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV <Wd>, <Wn>")]
        public void Rev32_32bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(0x00000000u, 0x7FFFFFFFu,
                                        0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            uint opcode = 0x5AC00800; // REV W0, W0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x1: wn, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("REV64 <Xd>, <Xn>")]
        public void Rev64_64bit([Values(0u, 31u)] uint rd,
                                [Values(1u, 31u)] uint rn,
                                [Values(0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                        0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul)] [Random(RndCnt)] ulong xn)
        {
            uint opcode = 0xDAC00C00; // REV64 X0, X0
            opcode |= ((rn & 31) << 5) | ((rd & 31) << 0);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x1: xn, x31: x31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
