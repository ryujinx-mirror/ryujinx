using ChocolArm64.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [TestFixture]
    public partial class CpuTest
    {
        [TestCase(0x00000000u, 0x80000000u, 0x00000000u)]
        [TestCase(0x80000000u, 0x00000000u, 0x00000000u)]
        [TestCase(0x80000000u, 0x80000000u, 0x80000000u)]
        [TestCase(0x3DCCCCCDu, 0x3C9623B1u, 0x3DCCCCCDu)]
        [TestCase(0x8BA98D27u, 0x00000076u, 0x00000076u)]
        [TestCase(0x807FFFFFu, 0x7F7FFFFFu, 0x7F7FFFFFu)]
        [TestCase(0x7F7FFFFFu, 0x807FFFFFu, 0x7F7FFFFFu)]
        [TestCase(0x7FC00000u, 0x3F800000u, 0x7FC00000u)]
        [TestCase(0x3F800000u, 0x7FC00000u, 0x7FC00000u)]
        // NaN tests
        //[TestCase(0x7F800001u, 0x7FC00042u, 0x7FC00001u)]
        //[TestCase(0x7FC00042u, 0x7F800001u, 0x7FC00001u)]
        //[TestCase(0x7FC0000Au, 0x7FC0000Bu, 0x7FC0000Au)]
        public void Fmax_S(uint A, uint B, uint Result)
        {
            // FMAX S0, S1, S2
            uint Opcode = 0x1E224820;
            AThreadState ThreadState = SingleOpcode(Opcode, V1: new AVec { W0 = A }, V2: new AVec { W0 = B });
            Assert.AreEqual(Result, ThreadState.V0.W0);
        }
    }
}
