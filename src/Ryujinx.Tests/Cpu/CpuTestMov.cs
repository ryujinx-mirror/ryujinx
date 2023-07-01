#define Mov

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Mov")]
    public sealed class CpuTestMov : CpuTest
    {
#if Mov
        private const int RndCnt = 2;

        [Test, Pairwise, Description("MOVK <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movk_64bit([Values(0u, 31u)] uint rd,
                               [Random(RndCnt)] ulong xd,
                               [Values(0u, 65535u)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint opcode = 0xF2800000; // MOVK X0, #0, LSL #0
            opcode |= ((rd & 31) << 0);
            opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x0: xd, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVK <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movk_32bit([Values(0u, 31u)] uint rd,
                               [Random(RndCnt)] uint wd,
                               [Values(0u, 65535u)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint opcode = 0x72800000; // MOVK W0, #0, LSL #0
            opcode |= ((rd & 31) << 0);
            opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x0: wd, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVN <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movn_64bit([Values(0u, 31u)] uint rd,
                               [Values(0u, 65535u)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint opcode = 0x92800000; // MOVN X0, #0, LSL #0
            opcode |= ((rd & 31) << 0);
            opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVN <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movn_32bit([Values(0u, 31u)] uint rd,
                               [Values(0u, 65535u)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint opcode = 0x12800000; // MOVN W0, #0, LSL #0
            opcode |= ((rd & 31) << 0);
            opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x31: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVZ <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movz_64bit([Values(0u, 31u)] uint rd,
                               [Values(0u, 65535u)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint opcode = 0xD2800000; // MOVZ X0, #0, LSL #0
            opcode |= ((rd & 31) << 0);
            opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong x31 = TestContext.CurrentContext.Random.NextULong();

            SingleOpcode(opcode, x31: x31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVZ <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movz_32bit([Values(0u, 31u)] uint rd,
                               [Values(0u, 65535u)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint opcode = 0x52800000; // MOVZ W0, #0, LSL #0
            opcode |= ((rd & 31) << 0);
            opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, x31: w31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
