#define SimdReg

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("SimdReg")/*, Ignore("Tested: second half of 2018.")*/]
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

            ulong _E0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_E0);
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
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_E0));
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

        [Test, Description("CMEQ <V><d>, <V><n>, <V><m>")]
        public void Cmeq_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                             [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x7EE28C20; // CMEQ D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmeq_Reg_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("CMEQ <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmeq_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E228C20; // CMEQ V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmeq_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmeq_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E228C20; // CMEQ V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Cmeq_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGE <V><d>, <V><n>, <V><m>")]
        public void Cmge_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                             [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x5EE23C20; // CMGE D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmge_Reg_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("CMGE <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmge_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E223C20; // CMGE V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmge_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmge_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E223C20; // CMGE V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Cmge_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGT <V><d>, <V><n>, <V><m>")]
        public void Cmgt_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                             [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x5EE23420; // CMGT D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmgt_Reg_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("CMGT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmgt_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E223420; // CMGT V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmgt_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmgt_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E223420; // CMGT V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Cmgt_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMHI <V><d>, <V><n>, <V><m>")]
        public void Cmhi_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                             [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x7EE23420; // CMHI D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhi_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("CMHI <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhi_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E223420; // CMHI V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhi_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("CMHI <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhi_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E223420; // CMHI V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Cmhi_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMHS <V><d>, <V><n>, <V><m>")]
        public void Cmhs_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                             [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x7EE23C20; // CMHS D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhs_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("CMHS <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhs_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E223C20; // CMHS V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhs_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("CMHS <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhs_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                        [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E223C20; // CMHS V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Cmhs_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMTST <V><d>, <V><n>, <V><m>")]
        public void Cmtst_S_D([ValueSource("_1D_")] [Random(1)] ulong A,
                              [ValueSource("_1D_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x5EE28C20; // CMTST D0, D1, D2
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmtst_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Description("CMTST <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmtst_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E228C20; // CMTST V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmtst_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("CMTST <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmtst_V_16B_8H_4S_2D([ValueSource("_8B4H2S1D_")] [Random(1)] ulong A0,
                                         [ValueSource("_8B4H2S1D_")] [Random(1)] ulong A1,
                                         [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B0,
                                         [ValueSource("_8B4H2S1D_")] [Random(1)] ulong B1,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E228C20; // CMTST V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Cmtst_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("EOR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Eor_V_8B([ValueSource("_8B_")] [Random(1)] ulong A,
                             [ValueSource("_8B_")] [Random(1)] ulong B)
        {
            uint Opcode = 0x2E221C20; // EOR V0.8B, V1.8B, V2.8B
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE1(TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Eor_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("EOR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Eor_V_16B([ValueSource("_8B_")] [Random(1)] ulong A0,
                              [ValueSource("_8B_")] [Random(1)] ulong A1,
                              [ValueSource("_8B_")] [Random(1)] ulong B0,
                              [ValueSource("_8B_")] [Random(1)] ulong B1)
        {
            uint Opcode = 0x6E221C20; // EOR V0.16B, V1.16B, V2.16B
            Bits Op = new Bits(Opcode);

            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Eor_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

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

            ulong _E0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_E0);
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
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_E0));
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

            ulong _E0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_E0);
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
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_E0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("SABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Saba_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E227C20; // SABA V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z, TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Saba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("SABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Saba_V_16B_8H_4S([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong _Z1,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E227C20; // SABA V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
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
            SimdFp.Saba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabal_V_8B8H_4H4S_2S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z0,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong _Z1,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E225020; // SABAL V0.8H, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE0(A0);
            Vector128<float> V2 = MakeVectorE0(B0);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(2, 0, new Bits(B0));
            SimdFp.Sabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabal_V_16B8H_8H4S_4S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z0,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong _Z1,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E225020; // SABAL2 V0.8H, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE1(A1);
            Vector128<float> V2 = MakeVectorE1(B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("SABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sabd_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E227420; // SABD V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Sabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("SABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sabd_V_16B_8H_4S([ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E227420; // SABD V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("SABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabdl_V_8B8H_4H4S_2S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E227020; // SABDL V0.8H, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A0);
            Vector128<float> V2 = MakeVectorE0(B0);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(2, 0, new Bits(B0));
            SimdFp.Sabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("SABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabdl_V_16B8H_8H4S_4S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E227020; // SABDL2 V0.8H, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE1(A1);
            Vector128<float> V2 = MakeVectorE1(B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
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

            ulong _E0 = TestContext.CurrentContext.Random.NextULong();
            Vector128<float> V0 = MakeVectorE0(_E0);
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
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(_E0));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("UABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uaba_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E227C20; // UABA V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z, TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uaba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("UABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uaba_V_16B_8H_4S([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong _Z1,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E227C20; // UABA V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
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
            SimdFp.Uaba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabal_V_8B8H_4H4S_2S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z0,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong _Z1,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E225020; // UABAL V0.8H, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE0(A0);
            Vector128<float> V2 = MakeVectorE0(B0);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(2, 0, new Bits(B0));
            SimdFp.Uabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabal_V_16B8H_8H4S_4S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong _Z0,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong _Z1,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E225020; // UABAL2 V0.8H, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(_Z0, _Z1);
            Vector128<float> V1 = MakeVectorE1(A1);
            Vector128<float> V2 = MakeVectorE1(B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(_Z0));
            AArch64.Vpart(0, 1, new Bits(_Z1));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Uabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("UABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uabd_V_8B_4H_2S([ValueSource("_8B4H2S_")] [Random(1)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(1)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E227420; // UABD V0.8B, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.V(64, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.Zero);
            });
        }

        [Test, Pairwise, Description("UABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uabd_V_16B_8H_4S([ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                     [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E227420; // UABD V0.16B, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Uabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("UABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabdl_V_8B8H_4H4S_2S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong A0,
                                           [ValueSource("_8B4H2S_")] [Random(1)] ulong B0,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E227020; // UABDL V0.8H, V1.8B, V2.8B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE0(A0);
            Vector128<float> V2 = MakeVectorE0(B0);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 0, new Bits(A0));
            AArch64.Vpart(2, 0, new Bits(B0));
            SimdFp.Uabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("UABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabdl_V_16B8H_8H4S_4S2D([ValueSource("_8B4H2S_")] [Random(1)] ulong A1,
                                            [ValueSource("_8B4H2S_")] [Random(1)] ulong B1,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E227020; // UABDL2 V0.8H, V1.16B, V2.16B
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(TestContext.CurrentContext.Random.NextULong(),
                                                 TestContext.CurrentContext.Random.NextULong());
            Vector128<float> V1 = MakeVectorE1(A1);
            Vector128<float> V2 = MakeVectorE1(B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Uabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }
#endif
    }
}
