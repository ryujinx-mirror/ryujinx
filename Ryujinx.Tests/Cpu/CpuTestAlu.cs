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

        [TestCase(0x3A020020u, 2u,          3u,          false, false, false, false, 5u)]
        [TestCase(0x3A020020u, 2u,          3u,          true,  false, false, false, 6u)]
        [TestCase(0xBA020020u, 2u,          3u,          false, false, false, false, 5u)]
        [TestCase(0xBA020020u, 2u,          3u,          true,  false, false, false, 6u)]
        [TestCase(0x3A020020u, 0xFFFFFFFEu, 0x1u,        true,  false, true,  true,  0x0u)]
        [TestCase(0x3A020020u, 0xFFFFFFFFu, 0xFFFFFFFFu, true,  true,  false, true,  0xFFFFFFFFu)]
        public void Adcs(uint Opcode, uint A, uint B, bool CarryState, bool Negative, bool Zero, bool Carry, uint Result)
        {
            //ADCS (X0/W0), (X1, W1), (X2/W2)
            AThreadState ThreadState = SingleOpcode(Opcode, X1: A, X2: B, Carry: CarryState);
            Assert.Multiple(() =>
            {
                Assert.IsFalse(ThreadState.Overflow);
                Assert.AreEqual(Negative, ThreadState.Negative);
                Assert.AreEqual(Zero,     ThreadState.Zero);
                Assert.AreEqual(Carry,    ThreadState.Carry);
                Assert.AreEqual(Result,   ThreadState.X0);
            });
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
            Assert.Multiple(() =>
            {
                Assert.IsFalse(ThreadState.Negative);
                Assert.IsFalse(ThreadState.Overflow);
                Assert.AreEqual(Zero,  ThreadState.Zero);
                Assert.AreEqual(Carry, ThreadState.Carry);
                Assert.AreEqual(A,     ThreadState.X31);
            });
        }

        [TestCase(0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFul, true,  false)]
        [TestCase(0xFFFFFFFFu, 0x00000000u, 0x00000000ul, false, true)]
        [TestCase(0x12345678u, 0x7324A993u, 0x12240010ul, false, false)]
        public void Ands(uint A, uint B, ulong Result, bool Negative, bool Zero)
        {
            // ANDS W0, W1, W2
            uint Opcode = 0x6A020020;
            AThreadState ThreadState = SingleOpcode(Opcode, X1: A, X2: B);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(Result,   ThreadState.X0);
                Assert.AreEqual(Negative, ThreadState.Negative);
                Assert.AreEqual(Zero,     ThreadState.Zero);
            });
        }

        [TestCase(0x0000FF44u, 0x00000004u, 0x00000FF4u)]
        [TestCase(0x00000000u, 0x00000004u, 0x00000000u)]
        [TestCase(0x0000FF44u, 0x00000008u, 0x000000FFu)]
        [TestCase(0xFFFFFFFFu, 0x00000004u, 0xFFFFFFFFu)]
        [TestCase(0xFFFFFFFFu, 0x00000008u, 0xFFFFFFFFu)]
        [TestCase(0xFFFFFFFFu, 0x00000020u, 0xFFFFFFFFu)]
        [TestCase(0x0FFFFFFFu, 0x0000001Cu, 0x00000000u)]
        [TestCase(0x80000000u, 0x0000001Fu, 0xFFFFFFFFu)]
        [TestCase(0xCAFE0000u, 0x00000020u, 0xCAFE0000u)]
        public void Asrv32(uint A, uint ShiftValue, uint Result)
        {
            // ASRV W0, W1, W2
            AThreadState ThreadState = SingleOpcode(0x1AC22820, X1: A, X2: ShiftValue);
            Assert.AreEqual(Result, ThreadState.X0);
        }

        [TestCase(0x000000000000FF44ul, 0x00000004u, 0x0000000000000FF4ul)]
        [TestCase(0x0000000000000000ul, 0x00000004u, 0x0000000000000000ul)]
        [TestCase(0x000000000000FF44ul, 0x00000008u, 0x00000000000000FFul)]
        [TestCase(0x00000000FFFFFFFFul, 0x00000004u, 0x000000000FFFFFFFul)]
        [TestCase(0x00000000FFFFFFFFul, 0x00000008u, 0x0000000000FFFFFFul)]
        [TestCase(0x00000000FFFFFFFFul, 0x00000020u, 0x0000000000000000ul)]
        [TestCase(0x000000000FFFFFFFul, 0x0000001Cu, 0x0000000000000000ul)]
        [TestCase(0x000CC4488FFFFFFFul, 0x0000001Cu, 0x0000000000CC4488ul)]
        [TestCase(0xFFFFFFFFFFFFFFFFul, 0x0000001Cu, 0xFFFFFFFFFFFFFFFFul)]
        [TestCase(0x8000000000000000ul, 0x0000003Fu, 0xFFFFFFFFFFFFFFFFul)]
        [TestCase(0xCAFE000000000000ul, 0x00000040u, 0xCAFE000000000000ul)]
        public void Asrv64(ulong A, uint ShiftValue, ulong Result)
        {
            // ASRV X0, X1, X2
            AThreadState ThreadState = SingleOpcode(0x9AC22820, X1: A, X2: ShiftValue);
            Assert.AreEqual(Result, ThreadState.X0);
        }

        [TestCase(0x01010101u, 0x3200C3E2u)]
        [TestCase(0x00F000F0u, 0x320C8FE2u)]
        [TestCase(0x00000001u, 0x320003E2u)]
        public void OrrBitmasks(uint Bitmask, uint Opcode)
        {
            // ORR W2, WZR, #Bitmask
            Assert.AreEqual(Bitmask, SingleOpcode(Opcode).X2);
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
            Assert.Multiple(() =>
            {
                Assert.IsFalse(ThreadState.Overflow);
                Assert.AreEqual(Negative, ThreadState.Negative);
                Assert.AreEqual(Zero,     ThreadState.Zero);
                Assert.AreEqual(Carry,    ThreadState.Carry);
                Assert.AreEqual(Result,   ThreadState.X0);
            });
        }
    }
}
