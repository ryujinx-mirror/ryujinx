#define SimdReg

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("SimdReg")]
    public sealed class CpuTestSimdReg : CpuTest
    {
#if SimdReg
        [SetUp]
        public void SetupTester()
        {
            AArch64.TakeReset(false);
        }

#region "ValueSource"
        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _4H2S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B4H2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B4H2S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

        [Test, Description("ADD <V><d>, <V><n>, <V><m>")]
        public void Add_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                            [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x5EE28420; // ADD D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Add_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("ADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Add_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                   [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E228420; // ADD V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Add_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("ADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Add_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                       [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                       [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                       [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E228420; // ADD V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Add_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Addhn_V_8H8B_4S4H_2D2S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                           [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                           [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                           [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E224020; // ADDHN V0.8B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Addhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("ADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Addhn_V_8H16B_4S8H_2D4S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E224020; // ADDHN2 V0.16B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            ulong _X0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_X0);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Addhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_X0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ADDP <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Addp_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E22BC20; // ADDP V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Addp_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("ADDP <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Addp_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E22BC20; // ADDP V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Addp_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("AND <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void And_V_8B([ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x0E221C20; // AND V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.And_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("AND <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void And_V_16B([ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x4E221C20; // AND V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.And_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("BIC <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bic_V_8B([ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x0E621C20; // BIC V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bic_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("BIC <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bic_V_16B([ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x4E621C20; // BIC V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Bic_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("BIF <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bif_V_8B([ValueSource("_8B_")] [Random(1)] ulong _Z,
                             [ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x2EE21C20; // BIF V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z, TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bif_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("BIF <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bif_V_16B([ValueSource("_8B_")] [Random(1)] ulong _Z0,
                              [ValueSource("_8B_")] [Random(1)] ulong _Z1,
                              [ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x6EE21C20; // BIF V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Bif_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("BIT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bit_V_8B([ValueSource("_8B_")] [Random(1)] ulong _Z,
                             [ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x2EA21C20; // BIT V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z, TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bit_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("BIT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bit_V_16B([ValueSource("_8B_")] [Random(1)] ulong _Z0,
                              [ValueSource("_8B_")] [Random(1)] ulong _Z1,
                              [ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x6EA21C20; // BIT V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Bit_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("BSL <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bsl_V_8B([ValueSource("_8B_")] [Random(1)] ulong _Z,
                             [ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x2E621C20; // BSL V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z, TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bsl_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("BSL <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bsl_V_16B([ValueSource("_8B_")] [Random(1)] ulong _Z0,
                              [ValueSource("_8B_")] [Random(1)] ulong _Z1,
                              [ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x6E621C20; // BSL V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Bsl_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ORN <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orn_V_8B([ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x0EE21C20; // ORN V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Orn_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("ORN <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orn_V_16B([ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x4EE21C20; // ORN V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Orn_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ORR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orr_V_8B([ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x0EA21C20; // ORR V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Orr_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("ORR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orr_V_16B([ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x4EA21C20; // ORR V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Orr_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("RADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Raddhn_V_8H8B_4S4H_2D2S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E224020; // RADDHN V0.8B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Raddhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("RADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Raddhn_V_8H16B_4S8H_2D4S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                             [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                             [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                             [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E224020; // RADDHN2 V0.16B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            ulong _X0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_X0);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Raddhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_X0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("RSUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Rsubhn_V_8H8B_4S4H_2D2S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E226020; // RSUBHN V0.8B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Rsubhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("RSUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Rsubhn_V_8H16B_4S8H_2D4S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                             [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                             [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                             [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E226020; // RSUBHN2 V0.16B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            ulong _X0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_X0);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Rsubhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_X0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("SUB <V><d>, <V><n>, <V><m>")]
        public void Sub_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                            [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x7EE28420; // SUB D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Sub_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("SUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sub_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                   [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E228420; // SUB V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Sub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("SUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sub_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                       [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                       [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                       [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E228420; // SUB V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Subhn_V_8H8B_4S4H_2D2S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                           [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                           [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                           [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E226020; // SUBHN V0.8B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Subhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("SUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Subhn_V_8H16B_4S8H_2D4S([ValueSource("_4H2S1D_")] [Random(1)] ulong A0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong A1,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B0,
                                            [ValueSource("_4H2S1D_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E226020; // SUBHN2 V0.16B, V1.8H, V2.8H
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            ulong _X0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_X0);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Subhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_X0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }
#endif
    }
}
