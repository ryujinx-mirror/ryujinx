using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdMove : CpuTest
    {
        [Test, Description("trn1 v0.4s, v1.4s, v2.4s")]
        public void Trn1_V_4S([Random(2)] uint A0, [Random(2)] uint A1, [Random(2)] uint A2, [Random(2)] uint A3,
                              [Random(2)] uint B0, [Random(2)] uint B1, [Random(2)] uint B2, [Random(2)] uint B3)
        {
            uint Opcode = 0x4E822820;
            Vector128<float> V1 = Sse.StaticCast<uint, float>(Sse2.SetVector128(A3, A2, A1, A0));
            Vector128<float> V2 = Sse.StaticCast<uint, float>(Sse2.SetVector128(B3, B2, B1, B0));

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)0);

            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)0), Is.EqualTo(A0));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)1), Is.EqualTo(B0));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)2), Is.EqualTo(A2));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)3), Is.EqualTo(B2));
        }

        [Test, Description("trn1 v0.8b, v1.8b, v2.8b")]
        public void Trn1_V_8B([Random(2)] byte A0, [Random(1)] byte A1, [Random(2)] byte A2, [Random(1)] byte A3,
                              [Random(2)] byte A4, [Random(1)] byte A5, [Random(2)] byte A6, [Random(1)] byte A7,
                              [Random(2)] byte B0, [Random(1)] byte B1, [Random(2)] byte B2, [Random(1)] byte B3,
                              [Random(2)] byte B4, [Random(1)] byte B5, [Random(2)] byte B6, [Random(1)] byte B7)
        {
            uint Opcode = 0x0E022820;
            Vector128<float> V1 = Sse.StaticCast<byte, float>(Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, A7, A6, A5, A4, A3, A2, A1, A0));
            Vector128<float> V2 = Sse.StaticCast<byte, float>(Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, B7, B6, B5, B4, B3, B2, B1, B0));

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)0), Is.EqualTo(A0));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)1), Is.EqualTo(B0));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)2), Is.EqualTo(A2));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)3), Is.EqualTo(B2));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)4), Is.EqualTo(A4));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)5), Is.EqualTo(B4));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)6), Is.EqualTo(A6));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)7), Is.EqualTo(B6));
        }

        [Test, Description("trn2 v0.4s, v1.4s, v2.4s")]
        public void Trn2_V_4S([Random(2)] uint A0, [Random(2)] uint A1, [Random(2)] uint A2, [Random(2)] uint A3,
                              [Random(2)] uint B0, [Random(2)] uint B1, [Random(2)] uint B2, [Random(2)] uint B3)
        {
            uint Opcode = 0x4E826820;
            Vector128<float> V1 = Sse.StaticCast<uint, float>(Sse2.SetVector128(A3, A2, A1, A0));
            Vector128<float> V2 = Sse.StaticCast<uint, float>(Sse2.SetVector128(B3, B2, B1, B0));

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)0), Is.EqualTo(A1));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)1), Is.EqualTo(B1));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)2), Is.EqualTo(A3));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V0), (byte)3), Is.EqualTo(B3));
        }

        [Test, Description("trn2 v0.8b, v1.8b, v2.8b")]
        public void Trn2_V_8B([Random(1)] byte A0, [Random(2)] byte A1, [Random(1)] byte A2, [Random(2)] byte A3,
                              [Random(1)] byte A4, [Random(2)] byte A5, [Random(1)] byte A6, [Random(2)] byte A7,
                              [Random(1)] byte B0, [Random(2)] byte B1, [Random(1)] byte B2, [Random(2)] byte B3,
                              [Random(1)] byte B4, [Random(2)] byte B5, [Random(1)] byte B6, [Random(2)] byte B7)
        {
            uint Opcode = 0x0E026820;
            Vector128<float> V1 = Sse.StaticCast<byte, float>(Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, A7, A6, A5, A4, A3, A2, A1, A0));
            Vector128<float> V2 = Sse.StaticCast<byte, float>(Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, B7, B6, B5, B4, B3, B2, B1, B0));

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)0), Is.EqualTo(A1));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)1), Is.EqualTo(B1));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)2), Is.EqualTo(A3));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)3), Is.EqualTo(B3));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)4), Is.EqualTo(A5));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)5), Is.EqualTo(B5));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)6), Is.EqualTo(A7));
            Assert.That(Sse41.Extract(Sse.StaticCast<float, byte>(ThreadState.V0), (byte)7), Is.EqualTo(B7));
        }

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
            Vector128<float> V1 = MakeVectorE0E1(0x1716151413121110, 0x1F1E1D1C1B1A1918);
            Vector128<float> V2 = MakeVectorE0E1(0x2726252423222120, 0x2F2E2D2C2B2A2928);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);
            Assert.AreEqual(Result_0, GetVectorE0(ThreadState.V0));
            Assert.AreEqual(Result_1, GetVectorE1(ThreadState.V0));
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
            Vector128<float> V1 = MakeVectorE0E1(0x1716151413121110, 0x1F1E1D1C1B1A1918);
            Vector128<float> V2 = MakeVectorE0E1(0x2726252423222120, 0x2F2E2D2C2B2A2928);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);
            Assert.AreEqual(Result_0, GetVectorE0(ThreadState.V0));
            Assert.AreEqual(Result_1, GetVectorE1(ThreadState.V0));
        }
    }
}
