#define Mov

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("Mov")] // Tested: second half of 2018.
    public sealed class CpuTestMov : CpuTest
    {
#if Mov
        private const int RndCntImm = 2;

        [Test, Pairwise, Description("MOVK <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movk_64bit([Values(0u, 31u)] uint Rd,
                               [Random(RndCntImm)] ulong _Xd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, RndCntImm)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint Opcode = 0xF2800000; // MOVK X0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X0: _Xd, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVK <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movk_32bit([Values(0u, 31u)] uint Rd,
                               [Random(RndCntImm)] uint _Wd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, RndCntImm)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint Opcode = 0x72800000; // MOVK W0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X0: _Wd, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVN <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movn_64bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, RndCntImm)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint Opcode = 0x92800000; // MOVN X0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVN <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movn_32bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, RndCntImm)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint Opcode = 0x12800000; // MOVN W0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X31: _W31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVZ <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movz_64bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, RndCntImm)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint Opcode = 0xD2800000; // MOVZ X0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X31: _X31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("MOVZ <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movz_32bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, RndCntImm)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint Opcode = 0x52800000; // MOVZ W0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();

            CpuThreadState ThreadState = SingleOpcode(Opcode, X31: _W31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
