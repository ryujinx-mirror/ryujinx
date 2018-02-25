using ChocolArm64.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestAlu : CpuTest
    {
        [TestCase(0x9A020020u, 2u,          3u,   true,  6u)]
        [TestCase(0x9A020020u, 2u,          3u,   false, 5u)]
        [TestCase(0x1A020020u, 2u,          3u,   true,  6u)]
        [TestCase(0x1A020020u, 2u,          3u,   false, 5u)]
        [TestCase(0x1A020020u, 0xFFFFFFFFu, 0x2u, false, 0x1u)]
        public void Adc(uint Opcode, uint A, uint B, bool CarryState, uint Result)
        {
            // ADC (X0/W0), (X1/W1), (X2/W2)
            AThreadState ThreadState = SingleOpcode(Opcode, X1: A, X2: B, Carry: CarryState);
            Assert.AreEqual(Result, ThreadState.X0);
        }

        [TestCase(0x3A020020u, 2u,          3u,   false, false, false, 5u)]
        [TestCase(0x3A020020u, 2u,          3u,   true,  false, false, 6u)]
        [TestCase(0xBA020020u, 2u,          3u,   false, false, false, 5u)]
        [TestCase(0xBA020020u, 2u,          3u,   true,  false, false, 6u)]
        [TestCase(0x3A020020u, 0xFFFFFFFEu, 0x1u, true,  true,  true,  0x0u)]
        public void Adcs(uint Opcode, uint A, uint B, bool CarryState, bool Zero, bool Carry, uint Result)
        {
            //ADCS (X0/W0), (X1, W1), (X2/W2)
            AThreadState ThreadState = SingleOpcode(Opcode, X1: A, X2: B, Carry: CarryState);
            Assert.IsFalse(ThreadState.Negative);
            Assert.IsFalse(ThreadState.Overflow);
            Assert.AreEqual(Zero, ThreadState.Zero);
            Assert.AreEqual(Carry, ThreadState.Carry);
            Assert.AreEqual(Result, ThreadState.X0);
        }
    
        [Test]
        public void Add()
        {
            // ADD X0, X1, X2
            AThreadState ThreadState = SingleOpcode(0x8B020020, X1: 1, X2: 2);
            Assert.AreEqual(3, ThreadState.X0);
        }

        [TestCase(2u,          false, false)]
        [TestCase(5u,          false, false)]
        [TestCase(7u,          false, false)]
        [TestCase(0xFFFFFFFFu, false, true )]
        [TestCase(0xFFFFFFFBu, true,  true )]
        public void Adds(uint A, bool Zero, bool Carry)
        {
            //ADDS WZR, WSP, #5
            AThreadState ThreadState = SingleOpcode(0x310017FF, X31: A);
            Assert.IsFalse(ThreadState.Negative);
            Assert.AreEqual(Zero, ThreadState.Zero);
            Assert.AreEqual(Carry, ThreadState.Carry);
            Assert.IsFalse(ThreadState.Overflow);
            Assert.AreEqual(A, ThreadState.X31);
        }

        [TestCase(0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFul, true,  false)]
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

        [TestCase(0x7A020020u, 4u, 2u, false, false, false, true,  1u)]
        [TestCase(0x7A020020u, 4u, 2u, true,  false, false, true,  2u)]
        [TestCase(0xFA020020u, 4u, 2u, false, false, false, true,  1u)]
        [TestCase(0xFA020020u, 4u, 2u, true,  false, false, true,  2u)]
        [TestCase(0x7A020020u, 4u, 4u, false, true,  false, false, 0xFFFFFFFFu)]
        [TestCase(0x7A020020u, 4u, 4u, true,  false, true,  true,  0x0u)]
        public void Sbcs(uint Opcode, uint A, uint B, bool CarryState, bool Negative, bool Zero, bool Carry, uint Result)
        {
            //SBCS (X0/W0), (X1, W1), (X2/W2)
            AThreadState ThreadState = SingleOpcode(Opcode, X1: A, X2: B, Carry: CarryState);
            Assert.AreEqual(Negative, ThreadState.Negative);
            Assert.IsFalse(ThreadState.Overflow);
            Assert.AreEqual(Zero, ThreadState.Zero);
            Assert.AreEqual(Carry, ThreadState.Carry);
            Assert.AreEqual(Result, ThreadState.X0);
        }
    }
}
