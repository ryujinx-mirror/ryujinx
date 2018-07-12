using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdCvt : CpuTest
    {
        [TestCase((ushort)0x0000, 0x00000000u)] // Positive Zero
        [TestCase((ushort)0x8000, 0x80000000u)] // Negative Zero
        [TestCase((ushort)0x3E00, 0x3FC00000u)] // +1.5
        [TestCase((ushort)0xBE00, 0xBFC00000u)] // -1.5
        [TestCase((ushort)0xFFFF, 0xFFFFE000u)] // -QNaN
        [TestCase((ushort)0x7C00, 0x7F800000u)] // +Inf
        [TestCase((ushort)0x3C00, 0x3F800000u)] // 1.0
        [TestCase((ushort)0x3C01, 0x3F802000u)] // 1.0009765625
        [TestCase((ushort)0xC000, 0xC0000000u)] // -2.0
        [TestCase((ushort)0x7BFF, 0x477FE000u)] // 65504.0 (Largest Normal)
        [TestCase((ushort)0x03FF, 0x387FC000u)] // 0.00006097555 (Largest Subnormal)
        [TestCase((ushort)0x0001, 0x33800000u)] // 5.96046448e-8 (Smallest Subnormal)
        public void Fcvtl_V_f16(ushort Value, uint Result)
        {
            uint Opcode = 0x0E217801;
            Vector128<float> V0 = Sse.StaticCast<ushort, float>(Sse2.SetAllVector128(Value));

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0);

            Assert.Multiple(() =>
            {
                Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V1), (byte)0), Is.EqualTo(Result));
                Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V1), (byte)1), Is.EqualTo(Result));
                Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V1), (byte)2), Is.EqualTo(Result));
                Assert.That(Sse41.Extract(Sse.StaticCast<float, uint>(ThreadState.V1), (byte)3), Is.EqualTo(Result));
            });
        }
    }
}
