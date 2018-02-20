using ChocolArm64.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [TestFixture]
    public partial class CpuTest
    {
        [TestCase(0u, 0u, 0x2313221221112010ul, 0x0000000000000000ul)]
        [TestCase(1u, 0u, 0x2313221221112010ul, 0x2717261625152414ul)]
        [TestCase(0u, 1u, 0x2322131221201110ul, 0x0000000000000000ul)]
        [TestCase(1u, 1u, 0x2322131221201110ul, 0x2726171625241514ul)]
        [TestCase(0u, 2u, 0x2322212013121110ul, 0x0000000000000000ul)]
        [TestCase(1u, 2u, 0x2322212013121110ul, 0x2726252417161514ul)]
        [TestCase(1u, 3u, 0x1716151413121110ul, 0x2726252423222120ul)]
        public void Zip1_V(uint Q, uint size, ulong Result_0, ulong Result_1)
        {
            // ZIP1 V0.<T>, V1.<T>, V2.<T>
            uint Opcode = 0x0E023820 | (Q << 30) | (size << 22);
            AVec V1 = new AVec { X0 = 0x1716151413121110, X1 = 0x1F1E1D1C1B1A1918 };
            AVec V2 = new AVec { X0 = 0x2726252423222120, X1 = 0x2F2E2D2C2B2A2928 };
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);
            Assert.AreEqual(Result_0, ThreadState.V0.X0);
            Assert.AreEqual(Result_1, ThreadState.V0.X1);
        }

        [TestCase(0u, 0u, 0x2717261625152414ul, 0x0000000000000000ul)]
        [TestCase(1u, 0u, 0x2B1B2A1A29192818ul, 0x2F1F2E1E2D1D2C1Cul)]
        [TestCase(0u, 1u, 0x2726171625241514ul, 0x0000000000000000ul)]
        [TestCase(1u, 1u, 0x2B2A1B1A29281918ul, 0x2F2E1F1E2D2C1D1Cul)]
        [TestCase(0u, 2u, 0x2726252417161514ul, 0x0000000000000000ul)]
        [TestCase(1u, 2u, 0x2B2A29281B1A1918ul, 0x2F2E2D2C1F1E1D1Cul)]
        [TestCase(1u, 3u, 0x1F1E1D1C1B1A1918ul, 0x2F2E2D2C2B2A2928ul)]
        public void Zip2_V(uint Q, uint size, ulong Result_0, ulong Result_1)
        {
            // ZIP2 V0.<T>, V1.<T>, V2.<T>
            uint Opcode = 0x0E027820 | (Q << 30) | (size << 22);
            AVec V1 = new AVec { X0 = 0x1716151413121110, X1 = 0x1F1E1D1C1B1A1918 };
            AVec V2 = new AVec { X0 = 0x2726252423222120, X1 = 0x2F2E2D2C2B2A2928 };
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);
            Assert.AreEqual(Result_0, ThreadState.V0.X0);
            Assert.AreEqual(Result_1, ThreadState.V0.X1);
        }
    }
}
