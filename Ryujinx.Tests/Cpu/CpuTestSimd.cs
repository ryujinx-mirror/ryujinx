#define Simd

using ChocolArm64.State;

using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("Simd")]
    public sealed class CpuTestSimd : CpuTest
    {
#if Simd
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

#region "ValueSource"
        private static ulong[] _D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B4H2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _16B8H4S2D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

        [Test, Description("ABS <V><d>, <V><n>")]
        public void Abs_S_D([ValueSource("_D_")] [Random(1)] ulong A)
        {
            uint Opcode = 0x5EE0B820; // ABS D0, D1
            Bits Op = new Bits(Opcode);

            AVec V0 = new AVec { X1 = TestContext.CurrentContext.Random.NextULong() };
            AVec V1 = new AVec { X0 = A };
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.V(1, new Bits(A));
            SimdFp.Abs_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.That(ThreadState.V0.X0, Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
            Assert.That(ThreadState.V0.X1, Is.Zero);
        }

        [Test, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E20B820; // ABS V0.8B, V1.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            AVec V0 = new AVec { X1 = TestContext.CurrentContext.Random.NextULong() };
            AVec V1 = new AVec { X0 = A };
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.V(1, new Bits(A));
            SimdFp.Abs_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.That(ThreadState.V0.X0, Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
            Assert.That(ThreadState.V0.X1, Is.Zero);
        }

        [Test, Pairwise, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_16B_8H_4S_2D([ValueSource("_16B8H4S2D_")] [Random(1)] ulong A0,
                                       [ValueSource("_16B8H4S2D_")] [Random(1)] ulong A1,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E20B820; // ABS V0.16B, V1.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            AVec V1 = new AVec { X0 = A0, X1 = A1 };
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            SimdFp.Abs_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.V0.X0, Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(ThreadState.V0.X1, Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("NEG <V><d>, <V><n>")]
        public void Neg_S_D([ValueSource("_D_")] [Random(1)] ulong A)
        {
            uint Opcode = 0x7EE0B820; // NEG D0, D1
            Bits Op = new Bits(Opcode);

            AVec V0 = new AVec { X1 = TestContext.CurrentContext.Random.NextULong() };
            AVec V1 = new AVec { X0 = A };
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.V(1, new Bits(A));
            SimdFp.Neg_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.That(ThreadState.V0.X0, Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
            Assert.That(ThreadState.V0.X1, Is.Zero);
        }

        [Test, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E20B820; // NEG V0.8B, V1.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            AVec V0 = new AVec { X1 = TestContext.CurrentContext.Random.NextULong() };
            AVec V1 = new AVec { X0 = A };
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.V(1, new Bits(A));
            SimdFp.Neg_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.That(ThreadState.V0.X0, Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
            Assert.That(ThreadState.V0.X1, Is.Zero);
        }

        [Test, Pairwise, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_16B_8H_4S_2D([ValueSource("_16B8H4S2D_")] [Random(1)] ulong A0,
                                       [ValueSource("_16B8H4S2D_")] [Random(1)] ulong A1,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E20B820; // NEG V0.16B, V1.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            AVec V1 = new AVec { X0 = A0, X1 = A1 };
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            SimdFp.Neg_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(ThreadState.V0.X0, Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(ThreadState.V0.X1, Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }
#endif
    }
}
