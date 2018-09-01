//#define Mov

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("Mov"), Ignore("Tested: second half of 2018.")]
    public sealed class CpuTestMov : CpuTest
    {
#if Mov
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

        [Test, Description("MOVK <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movk_64bit([Values(0u, 31u)] uint Rd,
                               [Random(12)] ulong _Xd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, 10)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint Opcode = 0xF2800000; // MOVK X0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X0: _Xd, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rd, new Bits(_Xd));
                Base.Movk(Op[31], Op[22, 21], Op[20, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("MOVK <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movk_32bit([Values(0u, 31u)] uint Rd,
                               [Random(12)] uint _Wd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, 10)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint Opcode = 0x72800000; // MOVK W0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X0: _Wd, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                AArch64.X((int)Rd, new Bits(_Wd));
                Base.Movk(Op[31], Op[22, 21], Op[20, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("MOVN <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movn_64bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, 128)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint Opcode = 0x92800000; // MOVN X0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                Base.Movn(Op[31], Op[22, 21], Op[20, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("MOVN <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movn_32bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, 128)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint Opcode = 0x12800000; // MOVN W0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                Base.Movn(Op[31], Op[22, 21], Op[20, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("MOVZ <Xd>, #<imm>{, LSL #<shift>}")]
        public void Movz_64bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, 128)] uint imm,
                               [Values(0u, 16u, 32u, 48u)] uint shift)
        {
            uint Opcode = 0xD2800000; // MOVZ X0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            ulong _X31 = TestContext.CurrentContext.Random.NextULong();
            AThreadState ThreadState = SingleOpcode(Opcode, X31: _X31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                Base.Movz(Op[31], Op[22, 21], Op[20, 5], Op[4, 0]);
                ulong Xd = AArch64.X(64, (int)Rd).ToUInt64();

                Assert.That((ulong)ThreadState.X0, Is.EqualTo(Xd));
            }
            else
            {
                Assert.That((ulong)ThreadState.X31, Is.EqualTo(_X31));
            }
            CompareAgainstUnicorn();
        }

        [Test, Description("MOVZ <Wd>, #<imm>{, LSL #<shift>}")]
        public void Movz_32bit([Values(0u, 31u)] uint Rd,
                               [Values(0u, 65535u)] [Random(0u, 65535u, 128)] uint imm,
                               [Values(0u, 16u)] uint shift)
        {
            uint Opcode = 0x52800000; // MOVZ W0, #0, LSL #0
            Opcode |= ((Rd & 31) << 0);
            Opcode |= (((shift / 16) & 3) << 21) | ((imm & 65535) << 5);

            uint _W31 = TestContext.CurrentContext.Random.NextUInt();
            AThreadState ThreadState = SingleOpcode(Opcode, X31: _W31);

            if (Rd != 31)
            {
                Bits Op = new Bits(Opcode);

                Base.Movz(Op[31], Op[22, 21], Op[20, 5], Op[4, 0]);
                uint Wd = AArch64.X(32, (int)Rd).ToUInt32();

                Assert.That((uint)ThreadState.X0, Is.EqualTo(Wd));
            }
            else
            {
                Assert.That((uint)ThreadState.X31, Is.EqualTo(_W31));
            }
            CompareAgainstUnicorn();
        }
#endif
    }
}
