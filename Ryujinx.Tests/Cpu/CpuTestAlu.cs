using ChocolArm64.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestAlu : CpuTest
    {
        [Test]
        public void Add()
        {
            // ADD X0, X1, X2
            AThreadState ThreadState = SingleOpcode(0x8B020020, X1: 1, X2: 2);
            Assert.AreEqual(3, ThreadState.X0);
        }

        [TestCase(0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFul, true, false)]
        [TestCase(0xFFFFFFFFu, 0x00000000u, 0x00000000ul, false, true)]
        [TestCase(0x12345678u, 0x7324A993u, 0x12240010ul, false, false)]
        public void Ands(uint A, uint B, ulong Result, bool Negative, bool Zero)
        {
            // ANDS W0, W1, W2
            uint Opcode = 0x6A020020;
            AThreadState ThreadState = SingleOpcode(Opcode, X1: A, X2: B);
            Assert.AreEqual(Result,   ThreadState.X0);
            Assert.AreEqual(Negative, ThreadState.Negative);
            Assert.AreEqual(Zero,     ThreadState.Zero);
        }

        [Test]
        public void OrrBitmasks()
        {
            // ORR W0, WZR, #0x01010101
            Assert.AreEqual(0x01010101, SingleOpcode(0x3200C3E0).X0);

            Reset();

            // ORR W1, WZR, #0x00F000F0
            Assert.AreEqual(0x00F000F0, SingleOpcode(0x320C8FE1).X1);

            Reset();

            // ORR W2, WZR, #1
            Assert.AreEqual(0x00000001, SingleOpcode(0x320003E2).X2);
        }

        [Test]
        public void RevX0X0()
        {
            // REV X0, X0
            AThreadState ThreadState = SingleOpcode(0xDAC00C00, X0: 0xAABBCCDDEEFF1100);
            Assert.AreEqual(0x0011FFEEDDCCBBAA, ThreadState.X0);
        }

        [Test]
        public void RevW1W1()
        {
            // REV W1, W1
            AThreadState ThreadState = SingleOpcode(0x5AC00821, X1: 0x12345678);
            Assert.AreEqual(0x78563412, ThreadState.X1);
        }
    }
}
