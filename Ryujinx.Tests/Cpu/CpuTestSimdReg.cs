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
        private static ulong[] _1B1H1S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x000000000000007Ful,
                                 0x0000000000000080ul, 0x00000000000000FFul,
                                 0x0000000000007FFFul, 0x0000000000008000ul,
                                 0x000000000000FFFFul, 0x000000007FFFFFFFul,
                                 0x0000000080000000ul, 0x00000000FFFFFFFFul,
                                 0x7FFFFFFFFFFFFFFFul, 0x8000000000000000ul,
                                 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _1H1S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x0000000000007FFFul,
                                 0x0000000000008000ul, 0x000000000000FFFFul,
                                 0x000000007FFFFFFFul, 0x0000000080000000ul,
                                 0x00000000FFFFFFFFul };
        }

        private static ulong[] _4H2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
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

        private const int RndCnt = 4;

        [Test, Pairwise, Description("ADD <V><d>, <V><n>, <V><m>")]
        public void Add_S_D([Values(0u)]     uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [Values(2u, 0u)] uint Rm,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x5EE08400; // ADD D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Add_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Add_V_8B_4H_2S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [Values(2u, 0u)] uint Rm,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E208400; // ADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Add_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Add_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [Values(2u, 0u)] uint Rm,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E208400; // ADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Add_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Addhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [Values(2u, 0u)] uint Rm,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E204000; // ADDHN V0.8B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Addhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Addhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E204000; // ADDHN2 V0.16B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Addhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADDP <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Addp_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E20BC00; // ADDP V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Addp_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ADDP <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Addp_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E20BC00; // ADDP V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Addp_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("AND <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void And_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x0E201C00; // AND V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.And_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("AND <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void And_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x4E201C00; // AND V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.And_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BIC <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bic_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x0E601C00; // BIC V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bic_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BIC <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bic_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x4E601C00; // BIC V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Bic_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BIF <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bif_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x2EE01C00; // BIF V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bif_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BIF <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bif_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x6EE01C00; // BIF V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Bif_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BIT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bit_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x2EA01C00; // BIT V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bit_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BIT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bit_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x6EA01C00; // BIT V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Bit_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BSL <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bsl_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x2E601C00; // BSL V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Bsl_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("BSL <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Bsl_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x6E601C00; // BSL V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Bsl_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMEQ <V><d>, <V><n>, <V><m>")]
        public void Cmeq_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x7EE08C00; // CMEQ D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmeq_Reg_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmeq_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E208C00; // CMEQ V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmeq_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMEQ <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmeq_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E208C00; // CMEQ V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Cmeq_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMGE <V><d>, <V><n>, <V><m>")]
        public void Cmge_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x5EE03C00; // CMGE D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmge_Reg_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmge_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E203C00; // CMGE V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmge_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMGE <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmge_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E203C00; // CMGE V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Cmge_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMGT <V><d>, <V><n>, <V><m>")]
        public void Cmgt_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x5EE03400; // CMGT D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmgt_Reg_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmgt_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E203400; // CMGT V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmgt_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMGT <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmgt_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E203400; // CMGT V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Cmgt_Reg_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMHI <V><d>, <V><n>, <V><m>")]
        public void Cmhi_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x7EE03400; // CMHI D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhi_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMHI <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhi_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E203400; // CMHI V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhi_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMHI <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhi_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E203400; // CMHI V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Cmhi_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMHS <V><d>, <V><n>, <V><m>")]
        public void Cmhs_S_D([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x7EE03C00; // CMHS D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhs_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMHS <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhs_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E203C00; // CMHS V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmhs_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMHS <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmhs_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E203C00; // CMHS V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Cmhs_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMTST <V><d>, <V><n>, <V><m>")]
        public void Cmtst_S_D([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x5EE08C00; // CMTST D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmtst_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMTST <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmtst_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E208C00; // CMTST V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Cmtst_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("CMTST <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Cmtst_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [Values(2u, 0u)] uint Rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E208C00; // CMTST V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Cmtst_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("EOR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Eor_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x2E201C00; // EOR V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Eor_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("EOR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Eor_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x6E201C00; // EOR V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Eor_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ORN <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orn_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x0EE01C00; // ORN V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Orn_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ORN <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orn_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x4EE01C00; // ORN V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Orn_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ORR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orr_V_8B([Values(0u)]     uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [Values(2u, 0u)] uint Rm,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x0EA01C00; // ORR V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Orr_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ORR <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Orr_V_16B([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x4EA01C00; // ORR V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Orr_V(Op[30], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("RADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Raddhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E204000; // RADDHN V0.8B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Raddhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("RADDHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Raddhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [Values(2u, 0u)] uint Rm,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E204000; // RADDHN2 V0.16B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Raddhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("RSUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Rsubhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E206000; // RSUBHN V0.8B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Rsubhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("RSUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Rsubhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [Values(2u, 0u)] uint Rm,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E206000; // RSUBHN2 V0.16B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Rsubhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Saba_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E207C00; // SABA V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Saba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Saba_V_16B_8H_4S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E207C00; // SABA V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Saba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabal_V_8B8H_4H4S_2S2D([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [Values(2u, 0u)] uint Rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint Opcode = 0x0E205000; // SABAL V0.8H, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Sabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabal_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x4E205000; // SABAL2 V0.8H, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE1(A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Sabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sabd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E207400; // SABD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Sabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sabd_V_16B_8H_4S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E207400; // SABD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Sabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabdl_V_8B8H_4H4S_2S2D([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [Values(2u, 0u)] uint Rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint Opcode = 0x0E207000; // SABDL V0.8H, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Sabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Sabdl_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x4E207000; // SABDL2 V0.8H, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE1(A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Sabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Saddw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint Rd,
                                                 [Values(1u, 0u)] uint Rn,
                                                 [Values(2u, 0u)] uint Rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint Opcode = 0x0E201000; // SADDW V0.8H, V0.8H, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Saddw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Saddw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint Rd,
                                                  [Values(1u, 0u)] uint Rn,
                                                  [Values(2u, 0u)] uint Rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint Opcode = 0x4E201000; // SADDW2 V0.8H, V0.8H, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Saddw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHA256H <Qd>, <Qn>, <Vm>.4S")]
        public void Sha256h_V([Values(0u)]     uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [Values(2u, 0u)] uint Rm,
                              [Random(RndCnt / 2)] ulong Z0, [Random(RndCnt / 2)] ulong Z1,
                              [Random(RndCnt / 2)] ulong A0, [Random(RndCnt / 2)] ulong A1,
                              [Random(RndCnt / 2)] ulong B0, [Random(RndCnt / 2)] ulong B1)
        {
            uint Opcode = 0x5E004000; // SHA256H Q0, Q0, V0.4S
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z0, Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z0)); AArch64.Vpart(0, 1, new Bits(Z1));
            AArch64.Vpart(1, 0, new Bits(A0)); AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0)); AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sha256h_V(Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));

                Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(AArch64.Vpart(64, 1, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(AArch64.Vpart(64, 1, 1).ToUInt64()));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V2), Is.EqualTo(AArch64.Vpart(64, 2, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V2), Is.EqualTo(AArch64.Vpart(64, 2, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHA256H2 <Qd>, <Qn>, <Vm>.4S")]
        public void Sha256h2_V([Values(0u)]     uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [Values(2u, 0u)] uint Rm,
                               [Random(RndCnt / 2)] ulong Z0, [Random(RndCnt / 2)] ulong Z1,
                               [Random(RndCnt / 2)] ulong A0, [Random(RndCnt / 2)] ulong A1,
                               [Random(RndCnt / 2)] ulong B0, [Random(RndCnt / 2)] ulong B1)
        {
            uint Opcode = 0x5E005000; // SHA256H2 Q0, Q0, V0.4S
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z0, Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z0)); AArch64.Vpart(0, 1, new Bits(Z1));
            AArch64.Vpart(1, 0, new Bits(A0)); AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0)); AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sha256h2_V(Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));

                Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(AArch64.Vpart(64, 1, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(AArch64.Vpart(64, 1, 1).ToUInt64()));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V2), Is.EqualTo(AArch64.Vpart(64, 2, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V2), Is.EqualTo(AArch64.Vpart(64, 2, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHA256SU1 <Vd>.4S, <Vn>.4S, <Vm>.4S")]
        public void Sha256su1_V([Values(0u)]     uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [Values(2u, 0u)] uint Rm,
                                [Random(RndCnt / 2)] ulong Z0, [Random(RndCnt / 2)] ulong Z1,
                                [Random(RndCnt / 2)] ulong A0, [Random(RndCnt / 2)] ulong A1,
                                [Random(RndCnt / 2)] ulong B0, [Random(RndCnt / 2)] ulong B1)
        {
            uint Opcode = 0x5E006000; // SHA256SU1 V0.4S, V0.4S, V0.4S
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z0, Z1);
            Vector128<float> V1 = MakeVectorE0E1(A0, A1);
            Vector128<float> V2 = MakeVectorE0E1(B0, B1);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z0)); AArch64.Vpart(0, 1, new Bits(Z1));
            AArch64.Vpart(1, 0, new Bits(A0)); AArch64.Vpart(1, 1, new Bits(A1));
            AArch64.Vpart(2, 0, new Bits(B0)); AArch64.Vpart(2, 1, new Bits(B1));
            SimdFp.Sha256su1_V(Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V1), Is.EqualTo(AArch64.Vpart(64, 1, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V1), Is.EqualTo(AArch64.Vpart(64, 1, 1).ToUInt64()));

                Assert.That(GetVectorE0(ThreadState.V2), Is.EqualTo(AArch64.Vpart(64, 2, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V2), Is.EqualTo(AArch64.Vpart(64, 2, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E200400; // SHADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Shadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shadd_V_16B_8H_4S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [Values(2u, 0u)] uint Rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E200400; // SHADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Shadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shsub_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E202400; // SHSUB V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Shsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Shsub_V_16B_8H_4S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [Values(2u, 0u)] uint Rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E202400; // SHSUB V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Shsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SQADD <V><d>, <V><n>, <V><m>")]
        public void Sqadd_S_B_H_S_D([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x5E200C00; // SQADD B0, B0, B0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqadd_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E200C00; // SQADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqadd_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [Values(2u, 0u)] uint Rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E200C00; // SQADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQDMULH <V><d>, <V><n>, <V><m>")]
        public void Sqdmulh_S_H_S([Values(0u)]     uint Rd,
                                  [Values(1u, 0u)] uint Rn,
                                  [Values(2u, 0u)] uint Rm,
                                  [ValueSource("_1H1S_")] [Random(RndCnt)] ulong Z,
                                  [ValueSource("_1H1S_")] [Random(RndCnt)] ulong A,
                                  [ValueSource("_1H1S_")] [Random(RndCnt)] ulong B,
                                  [Values(0b01u, 0b10u)] uint size) // <H, S>
        {
            uint Opcode = 0x5E20B400; // SQDMULH B0, B0, B0 (RESERVED)
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqdmulh_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqdmulh_V_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b01u, 0b10u)] uint size) // <4H, 2S>
        {
            uint Opcode = 0x0E20B400; // SQDMULH V0.8B, V0.8B, V0.8B (RESERVED)
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqdmulh_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqdmulh_V_8H_4S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b01u, 0b10u)] uint size) // <8H, 4S>
        {
            uint Opcode = 0x4E20B400; // SQDMULH V0.16B, V0.16B, V0.16B (RESERVED)
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqdmulh_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQRDMULH <V><d>, <V><n>, <V><m>")]
        public void Sqrdmulh_S_H_S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [Values(2u, 0u)] uint Rm,
                                   [ValueSource("_1H1S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_1H1S_")] [Random(RndCnt)] ulong A,
                                   [ValueSource("_1H1S_")] [Random(RndCnt)] ulong B,
                                   [Values(0b01u, 0b10u)] uint size) // <H, S>
        {
            uint Opcode = 0x7E20B400; // SQRDMULH B0, B0, B0 (RESERVED)
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqrdmulh_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQRDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqrdmulh_V_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b01u, 0b10u)] uint size) // <4H, 2S>
        {
            uint Opcode = 0x2E20B400; // SQRDMULH V0.8B, V0.8B, V0.8B (RESERVED)
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqrdmulh_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQRDMULH <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqrdmulh_V_8H_4S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b01u, 0b10u)] uint size) // <8H, 4S>
        {
            uint Opcode = 0x6E20B400; // SQRDMULH V0.16B, V0.16B, V0.16B (RESERVED)
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqrdmulh_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQSUB <V><d>, <V><n>, <V><m>")]
        public void Sqsub_S_B_H_S_D([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x5E202C00; // SQSUB B0, B0, B0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqsub_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqsub_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E202C00; // SQSUB V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sqsub_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [Values(2u, 0u)] uint Rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E202C00; // SQSUB V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Sqsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("SRHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Srhadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [Values(2u, 0u)] uint Rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E201400; // SRHADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Srhadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SRHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Srhadd_V_16B_8H_4S([Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [Values(2u, 0u)] uint Rm,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                       [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E201400; // SRHADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Srhadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SSUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Ssubw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint Rd,
                                                 [Values(1u, 0u)] uint Rn,
                                                 [Values(2u, 0u)] uint Rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint Opcode = 0x0E203000; // SSUBW V0.8H, V0.8H, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Ssubw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SSUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Ssubw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint Rd,
                                                  [Values(1u, 0u)] uint Rn,
                                                  [Values(2u, 0u)] uint Rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint Opcode = 0x4E203000; // SSUBW2 V0.8H, V0.8H, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Ssubw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SUB <V><d>, <V><n>, <V><m>")]
        public void Sub_S_D([Values(0u)]     uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [Values(2u, 0u)] uint Rm,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong B)
        {
            uint Opcode = 0x7EE08400; // SUB D0, D0, D0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Sub_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sub_V_8B_4H_2S([Values(0u)]     uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [Values(2u, 0u)] uint Rm,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E208400; // SUB V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Sub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Sub_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [Values(2u, 0u)] uint Rm,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E208400; // SUB V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Sub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Subhn_V_8H8B_4S4H_2D2S([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [Values(2u, 0u)] uint Rm,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E206000; // SUBHN V0.8B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Subhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("SUBHN{2} <Vd>.<Tb>, <Vn>.<Ta>, <Vm>.<Ta>")]
        public void Subhn_V_8H16B_4S8H_2D4S([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E206000; // SUBHN2 V0.16B, V0.8H, V0.8H
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Subhn_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("TRN1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn1_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E002800; // TRN1 V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Trn1_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("TRN1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn1_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E002800; // TRN1 V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Trn1_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("TRN2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn2_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E006800; // TRN2 V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Trn2_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("TRN2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Trn2_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E006800; // TRN2 V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Trn2_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uaba_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E207C00; // UABA V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uaba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABA <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uaba_V_16B_8H_4S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E207C00; // UABA V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uaba_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabal_V_8B8H_4H4S_2S2D([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [Values(2u, 0u)] uint Rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint Opcode = 0x2E205000; // UABAL V0.8H, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Uabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABAL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabal_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x6E205000; // UABAL2 V0.8H, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE1(A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uabal_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uabd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E207400; // UABD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uabd_V_16B_8H_4S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E207400; // UABD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uabd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabdl_V_8B8H_4H4S_2S2D([Values(0u)]     uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [Values(2u, 0u)] uint Rm,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                           [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H, 4H4S, 2S2D>
        {
            uint Opcode = 0x2E207000; // UABDL V0.8H, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Uabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UABDL{2} <Vd>.<Ta>, <Vn>.<Tb>, <Vm>.<Tb>")]
        public void Uabdl_V_16B8H_8H4S_4S2D([Values(0u)]     uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [Values(2u, 0u)] uint Rm,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                            [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H, 8H4S, 4S2D>
        {
            uint Opcode = 0x6E207000; // UABDL2 V0.8H, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE1(A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uabdl_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Uaddw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint Rd,
                                                 [Values(1u, 0u)] uint Rn,
                                                 [Values(2u, 0u)] uint Rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint Opcode = 0x2E201000; // UADDW V0.8H, V0.8H, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Uaddw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UADDW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Uaddw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint Rd,
                                                  [Values(1u, 0u)] uint Rn,
                                                  [Values(2u, 0u)] uint Rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint Opcode = 0x6E201000; // UADDW2 V0.8H, V0.8H, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uaddw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E200400; // UHADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uhadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhadd_V_16B_8H_4S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [Values(2u, 0u)] uint Rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E200400; // UHADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uhadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhsub_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E202400; // UHSUB V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uhsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UHSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uhsub_V_16B_8H_4S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [Values(2u, 0u)] uint Rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E202400; // UHSUB V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uhsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UQADD <V><d>, <V><n>, <V><m>")]
        public void Uqadd_S_B_H_S_D([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x7E200C00; // UQADD B0, B0, B0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Uqadd_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("UQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E200C00; // UQADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Uqadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("UQADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqadd_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [Values(2u, 0u)] uint Rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E200C00; // UQADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Uqadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("UQSUB <V><d>, <V><n>, <V><m>")]
        public void Uqsub_S_B_H_S_D([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_1B1H1S1D_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <B, H, S, D>
        {
            uint Opcode = 0x7E202C00; // UQSUB B0, B0, B0
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Uqsub_S(Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("UQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqsub_V_8B_4H_2S([Values(0u)]     uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [Values(2u, 0u)] uint Rm,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E202C00; // UQSUB V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Uqsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("UQSUB <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uqsub_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [Values(2u, 0u)] uint Rm,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                         [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E202C00; // UQSUB V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            int Fpsr = (int)TestContext.CurrentContext.Random.NextUInt();

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2, Fpsr: Fpsr);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            Shared.FPSR = new Bits((uint)Fpsr);
            SimdFp.Uqsub_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(ThreadState.Fpsr, Is.EqualTo((int)Shared.FPSR.ToUInt32()));
        }

        [Test, Pairwise, Description("URHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Urhadd_V_8B_4H_2S([Values(0u)]     uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [Values(2u, 0u)] uint Rm,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E201400; // URHADD V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Urhadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("URHADD <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Urhadd_V_16B_8H_4S([Values(0u)]     uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [Values(2u, 0u)] uint Rm,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                       [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                       [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E201400; // URHADD V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Urhadd_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("USUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Usubw_V_8B8H8H_4H4S4S_2S2D2D([Values(0u)]     uint Rd,
                                                 [Values(1u, 0u)] uint Rn,
                                                 [Values(2u, 0u)] uint Rm,
                                                 [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                 [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                 [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                 [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B8H8H, 4H4S4S, 2S2D2D>
        {
            uint Opcode = 0x2E203000; // USUBW V0.8H, V0.8H, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B));
            SimdFp.Usubw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("USUBW{2} <Vd>.<Ta>, <Vn>.<Ta>, <Vm>.<Tb>")]
        public void Usubw_V_16B8H8H_8H4S4S_4S2D2D([Values(0u)]     uint Rd,
                                                  [Values(1u, 0u)] uint Rn,
                                                  [Values(2u, 0u)] uint Rm,
                                                  [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                                  [ValueSource("_4H2S1D_")]   [Random(RndCnt)] ulong A,
                                                  [ValueSource("_8B4H2S_")]   [Random(RndCnt)] ulong B,
                                                  [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B8H8H, 8H4S4S, 4S2D2D>
        {
            uint Opcode = 0x6E203000; // USUBW2 V0.8H, V0.8H, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE1(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Usubw_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UZP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp1_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E001800; // UZP1 V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uzp1_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UZP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp1_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E001800; // UZP1 V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uzp1_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UZP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp2_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E005800; // UZP2 V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Uzp2_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("UZP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Uzp2_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E005800; // UZP2 V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Uzp2_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ZIP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip1_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E003800; // ZIP1 V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Zip1_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ZIP1 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip1_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E003800; // ZIP1 V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Zip1_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ZIP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip2_V_8B_4H_2S([Values(0u)]     uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [Values(2u, 0u)] uint Rm,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong B,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E007800; // ZIP2 V0.8B, V0.8B, V0.8B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            Vector128<float> V2 = MakeVectorE0(B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            AArch64.V(2, new Bits(B));
            SimdFp.Zip2_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Pairwise, Description("ZIP2 <Vd>.<T>, <Vn>.<T>, <Vm>.<T>")]
        public void Zip2_V_16B_8H_4S_2D([Values(0u)]     uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [Values(2u, 0u)] uint Rm,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong B,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E007800; // ZIP2 V0.16B, V0.16B, V0.16B
            Opcode |= ((Rm & 31) << 16) | ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            Vector128<float> V2 = MakeVectorE0E1(B, B);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            AArch64.Vpart(2, 0, new Bits(B)); AArch64.Vpart(2, 1, new Bits(B));
            SimdFp.Zip2_V(Op[30], Op[23, 22], Op[20, 16], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }
#endif
    }
}
