#define Simd

using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    using Tester;
    using Tester.Types;

    [Category("Simd")/*, Ignore("Tested: second half of 2018.")*/]
    public sealed class CpuTestSimd : CpuTest
    {
#if Simd
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

        private static ulong[] _1H1S1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x0000000000007FFFul,
                                 0x0000000000008000ul, 0x000000000000FFFFul,
                                 0x000000007FFFFFFFul, 0x0000000080000000ul,
                                 0x00000000FFFFFFFFul, 0x7FFFFFFFFFFFFFFFul,
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

        private static ulong[] _8B4H_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0xFFFFFFFFFFFFFFFFul };
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

        private const int RndCnt = 1;

        [Test, Description("ABS <V><d>, <V><n>")]
        public void Abs_S_D([Values(0u)] uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE0B800; // ABS D0, D0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Abs_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_8B_4H_2S([Values(0u)] uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E20B800; // ABS V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Abs_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ABS <Vd>.<T>, <Vn>.<T>")]
        public void Abs_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E20B800; // ABS V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Abs_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ADDP <V><d>, <Vn>.<T>")]
        public void Addp_S_2DD([Values(0u)] uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EF1B800; // ADDP D0, V0.2D
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Addp_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ADDV <V><d>, <Vn>.<T>")]
        public void Addv_V_8BB_4HH([Values(0u)] uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u)] uint size) // <8BB, 4HH>
        {
            uint Opcode = 0x0E31B800; // ADDV B0, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Addv_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("ADDV <V><d>, <Vn>.<T>")]
        public void Addv_V_16BB_8HH_4SS([Values(0u)] uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u)] uint size) // <16BB, 8HH, 4SS>
        {
            uint Opcode = 0x4E31B800; // ADDV B0, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Addv_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CLS <Vd>.<T>, <Vn>.<T>")]
        public void Cls_V_8B_4H_2S([Values(0u)] uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E204800; // CLS V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cls_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CLS <Vd>.<T>, <Vn>.<T>")]
        public void Cls_V_16B_8H_4S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E204800; // CLS V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cls_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CLZ <Vd>.<T>, <Vn>.<T>")]
        public void Clz_V_8B_4H_2S([Values(0u)] uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E204800; // CLZ V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Clz_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CLZ <Vd>.<T>, <Vn>.<T>")]
        public void Clz_V_16B_8H_4S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x6E204800; // CLZ V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Clz_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMEQ <V><d>, <V><n>, #0")]
        public void Cmeq_S_D([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE09800; // CMEQ D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmeq_Zero_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMEQ <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmeq_V_8B_4H_2S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E209800; // CMEQ V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmeq_Zero_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMEQ <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmeq_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E209800; // CMEQ V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cmeq_Zero_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGE <V><d>, <V><n>, #0")]
        public void Cmge_S_D([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x7EE08800; // CMGE D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmge_Zero_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmge_V_8B_4H_2S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E208800; // CMGE V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmge_Zero_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmge_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E208800; // CMGE V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cmge_Zero_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGT <V><d>, <V><n>, #0")]
        public void Cmgt_S_D([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE08800; // CMGT D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmgt_Zero_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmgt_V_8B_4H_2S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E208800; // CMGT V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmgt_Zero_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMGT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmgt_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E208800; // CMGT V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cmgt_Zero_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMLE <V><d>, <V><n>, #0")]
        public void Cmle_S_D([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x7EE09800; // CMLE D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmle_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMLE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmle_V_8B_4H_2S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E209800; // CMLE V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmle_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMLE <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmle_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E209800; // CMLE V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cmle_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMLT <V><d>, <V><n>, #0")]
        public void Cmlt_S_D([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x5EE0A800; // CMLT D0, D0, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmlt_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMLT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmlt_V_8B_4H_2S([Values(0u)] uint Rd,
                                    [Values(1u, 0u)] uint Rn,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                    [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                    [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E20A800; // CMLT V0.8B, V0.8B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cmlt_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CMLT <Vd>.<T>, <Vn>.<T>, #0")]
        public void Cmlt_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                        [Values(1u, 0u)] uint Rn,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                        [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                        [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x4E20A800; // CMLT V0.16B, V0.16B, #0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cmlt_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CNT <Vd>.<T>, <Vn>.<T>")]
        public void Cnt_V_8B([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x0E205800; // CNT V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Cnt_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("CNT <Vd>.<T>, <Vn>.<T>")]
        public void Cnt_V_16B([Values(0u)] uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x4E205800; // CNT V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Cnt_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("NEG <V><d>, <V><n>")]
        public void Neg_S_D([Values(0u)] uint Rd,
                            [Values(1u, 0u)] uint Rn,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong Z,
                            [ValueSource("_1D_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x7EE0B800; // NEG D0, D0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Neg_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_8B_4H_2S([Values(0u)] uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x2E20B800; // NEG V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Neg_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("NEG <Vd>.<T>, <Vn>.<T>")]
        public void Neg_V_16B_8H_4S_2D([Values(0u)] uint Rd,
                                       [Values(1u, 0u)] uint Rn,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong Z,
                                       [ValueSource("_8B4H2S1D_")] [Random(RndCnt)] ulong A,
                                       [Values(0b00u, 0b01u, 0b10u, 0b11u)] uint size) // <16B, 8H, 4S, 2D>
        {
            uint Opcode = 0x6E20B800; // NEG V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Neg_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("NOT <Vd>.<T>, <Vn>.<T>")]
        public void Not_V_8B([Values(0u)] uint Rd,
                             [Values(1u, 0u)] uint Rn,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                             [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x2E205800; // NOT V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Not_V(Op[30], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("NOT <Vd>.<T>, <Vn>.<T>")]
        public void Not_V_16B([Values(0u)] uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x6E205800; // NOT V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Not_V(Op[30], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("RBIT <Vd>.<T>, <Vn>.<T>")]
        public void Rbit_V_8B([Values(0u)] uint Rd,
                              [Values(1u, 0u)] uint Rn,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                              [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x2E605800; // RBIT V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Rbit_V(Op[30], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("RBIT <Vd>.<T>, <Vn>.<T>")]
        public void Rbit_V_16B([Values(0u)] uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x6E605800; // RBIT V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Rbit_V(Op[30], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("REV16 <Vd>.<T>, <Vn>.<T>")]
        public void Rev16_V_8B([Values(0u)] uint Rd,
                               [Values(1u, 0u)] uint Rn,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                               [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x0E201800; // REV16 V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Rev16_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("REV16 <Vd>.<T>, <Vn>.<T>")]
        public void Rev16_V_16B([Values(0u)] uint Rd,
                                [Values(1u, 0u)] uint Rn,
                                [ValueSource("_8B_")] [Random(RndCnt)] ulong Z,
                                [ValueSource("_8B_")] [Random(RndCnt)] ulong A)
        {
            uint Opcode = 0x4E201800; // REV16 V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Rev16_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("REV32 <Vd>.<T>, <Vn>.<T>")]
        public void Rev32_V_8B_4H([Values(0u)] uint Rd,
                                  [Values(1u, 0u)] uint Rn,
                                  [ValueSource("_8B4H_")] [Random(RndCnt)] ulong Z,
                                  [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                                  [Values(0b00u, 0b01u)] uint size) // <8B, 4H>
        {
            uint Opcode = 0x2E200800; // REV32 V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Rev32_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("REV32 <Vd>.<T>, <Vn>.<T>")]
        public void Rev32_V_16B_8H([Values(0u)] uint Rd,
                                   [Values(1u, 0u)] uint Rn,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong Z,
                                   [ValueSource("_8B4H_")] [Random(RndCnt)] ulong A,
                                   [Values(0b00u, 0b01u)] uint size) // <16B, 8H>
        {
            uint Opcode = 0x6E200800; // REV32 V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Rev32_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("REV64 <Vd>.<T>, <Vn>.<T>")]
        public void Rev64_V_8B_4H_2S([Values(0u)] uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <8B, 4H, 2S>
        {
            uint Opcode = 0x0E200800; // REV64 V0.8B, V0.8B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Rev64_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("REV64 <Vd>.<T>, <Vn>.<T>")]
        public void Rev64_V_16B_8H_4S([Values(0u)] uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_8B4H2S_")] [Random(RndCnt)] ulong A,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <16B, 8H, 4S>
        {
            uint Opcode = 0x4E200800; // REV64 V0.16B, V0.16B
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Rev64_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("SQXTN <Vb><d>, <Va><n>")]
        public void Sqxtn_S_HB_SH_DS([Values(0u)] uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint Opcode = 0x5E214800; // SQXTN B0, H0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Sqxtn_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("SQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtn_V_8H8B_4S4H_2D2S([Values(0u)] uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E214800; // SQXTN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Sqxtn_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("SQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtn_V_8H16B_4S8H_2D4S([Values(0u)] uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E214800; // SQXTN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Sqxtn_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("SQXTUN <Vb><d>, <Va><n>")]
        public void Sqxtun_S_HB_SH_DS([Values(0u)] uint Rd,
                                      [Values(1u, 0u)] uint Rn,
                                      [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong Z,
                                      [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong A,
                                      [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint Opcode = 0x7E212800; // SQXTUN B0, H0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Sqxtun_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("SQXTUN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtun_V_8H8B_4S4H_2D2S([Values(0u)] uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E212800; // SQXTUN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Sqxtun_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("SQXTUN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Sqxtun_V_8H16B_4S8H_2D4S([Values(0u)] uint Rd,
                                             [Values(1u, 0u)] uint Rn,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                             [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                             [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E212800; // SQXTUN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Sqxtun_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("UQXTN <Vb><d>, <Va><n>")]
        public void Uqxtn_S_HB_SH_DS([Values(0u)] uint Rd,
                                     [Values(1u, 0u)] uint Rn,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong Z,
                                     [ValueSource("_1H1S1D_")] [Random(RndCnt)] ulong A,
                                     [Values(0b00u, 0b01u, 0b10u)] uint size) // <HB, SH, DS>
        {
            uint Opcode = 0x7E214800; // UQXTN B0, H0
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0(A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.V(1, new Bits(A));
            SimdFp.Uqxtn_S(Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("UQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Uqxtn_V_8H8B_4S4H_2D2S([Values(0u)] uint Rd,
                                           [Values(1u, 0u)] uint Rn,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                           [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                           [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x2E214800; // UQXTN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Uqxtn_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("UQXTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Uqxtn_V_8H16B_4S8H_2D4S([Values(0u)] uint Rd,
                                            [Values(1u, 0u)] uint Rn,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                            [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                            [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x6E214800; // UQXTN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Uqxtn_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
            Assert.That(((ThreadState.Fpsr >> 27) & 1) != 0, Is.EqualTo(Shared.FPSR[27]));
        }

        [Test, Description("XTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Xtn_V_8H8B_4S4H_2D2S([Values(0u)] uint Rd,
                                         [Values(1u, 0u)] uint Rn,
                                         [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                         [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                         [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H8B, 4S4H, 2D2S>
        {
            uint Opcode = 0x0E212800; // XTN V0.8B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Xtn_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }

        [Test, Description("XTN{2} <Vd>.<Tb>, <Vn>.<Ta>")]
        public void Xtn_V_8H16B_4S8H_2D4S([Values(0u)] uint Rd,
                                          [Values(1u, 0u)] uint Rn,
                                          [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong Z,
                                          [ValueSource("_4H2S1D_")] [Random(RndCnt)] ulong A,
                                          [Values(0b00u, 0b01u, 0b10u)] uint size) // <8H16B, 4S8H, 2D4S>
        {
            uint Opcode = 0x4E212800; // XTN2 V0.16B, V0.8H
            Opcode |= ((Rn & 31) << 5) | ((Rd & 31) << 0);
            Opcode |= ((size & 3) << 22);
            Bits Op = new Bits(Opcode);

            Vector128<float> V0 = MakeVectorE0E1(Z, Z);
            Vector128<float> V1 = MakeVectorE0E1(A, A);
            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            AArch64.Vpart(0, 0, new Bits(Z)); AArch64.Vpart(0, 1, new Bits(Z));
            AArch64.Vpart(1, 0, new Bits(A)); AArch64.Vpart(1, 1, new Bits(A));
            SimdFp.Xtn_V(Op[30], Op[23, 22], Op[9, 5], Op[4, 0]);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 0).ToUInt64()));
                Assert.That(GetVectorE1(ThreadState.V0), Is.EqualTo(AArch64.Vpart(64, 0, 1).ToUInt64()));
            });
        }
#endif
    }
}
