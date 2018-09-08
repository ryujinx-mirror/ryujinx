using ChocolArm64.State;

using NUnit.Framework;

using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdCmp : CpuTest
    {
#region "ValueSource"
        private static float[] _floats_()
        {
            return new float[] { float.NegativeInfinity, float.MinValue, -1f, -0f,
                                 +0f, +1f, float.MaxValue, float.PositiveInfinity };
        }

        private static double[] _doubles_()
        {
            return new double[] { double.NegativeInfinity, double.MinValue, -1d, -0d,
                                  +0d, +1d, double.MaxValue, double.PositiveInfinity };
        }
#endregion

        private const int RndCnt = 2;

        [Test, Description("FCMEQ D0, D1, D2 | FCMGE D0, D1, D2 | FCMGT D0, D1, D2")]
        public void Fcmeq_Fcmge_Fcmgt_Reg_S_D([ValueSource("_doubles_")] [Random(RndCnt)] double A,
                                              [ValueSource("_doubles_")] [Random(RndCnt)] double B,
                                              [Values(0u, 1u, 3u)] uint EU) // EQ, GE, GT
        {
            uint Opcode = 0x5E62E420 | ((EU & 1) << 29) | ((EU >> 1) << 23);

            Vector128<float> V0 = Sse.StaticCast<double, float>(Sse2.SetAllVector128(TestContext.CurrentContext.Random.NextDouble()));
            Vector128<float> V1 = Sse.StaticCast<double, float>(Sse2.SetScalarVector128(A));
            Vector128<float> V2 = Sse.StaticCast<double, float>(Sse2.SetScalarVector128(B));

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            switch (EU)
            {
                case 0: Exp = (A == B ? Ones : Zeros); break;
                case 1: Exp = (A >= B ? Ones : Zeros); break;
                case 3: Exp = (A >  B ? Ones : Zeros); break;
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(VectorExtractDouble(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(VectorExtractDouble(ThreadState.V0, (byte)1), Is.Zero);
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMEQ S0, S1, S2 | FCMGE S0, S1, S2 | FCMGT S0, S1, S2")]
        public void Fcmeq_Fcmge_Fcmgt_Reg_S_S([ValueSource("_floats_")] [Random(RndCnt)] float A,
                                              [ValueSource("_floats_")] [Random(RndCnt)] float B,
                                              [Values(0u, 1u, 3u)] uint EU) // EQ, GE, GT
        {
            uint Opcode = 0x5E22E420 | ((EU & 1) << 29) | ((EU >> 1) << 23);

            Vector128<float> V0 = Sse.SetAllVector128(TestContext.CurrentContext.Random.NextFloat());
            Vector128<float> V1 = Sse.SetScalarVector128(A);
            Vector128<float> V2 = Sse.SetScalarVector128(B);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00};

            switch (EU)
            {
                case 0: Exp = (A == B ? Ones : Zeros); break;
                case 1: Exp = (A >= B ? Ones : Zeros); break;
                case 3: Exp = (A >  B ? Ones : Zeros); break;
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)1), Is.Zero);
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)2), Is.Zero);
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)3), Is.Zero);
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMEQ V0.2D, V1.2D, V2.2D | FCMGE V0.2D, V1.2D, V2.2D | FCMGT V0.2D, V1.2D, V2.2D")]
        public void Fcmeq_Fcmge_Fcmgt_Reg_V_2D([ValueSource("_doubles_")] [Random(RndCnt)] double A,
                                               [ValueSource("_doubles_")] [Random(RndCnt)] double B,
                                               [Values(0u, 1u, 3u)] uint EU) // EQ, GE, GT
        {
            uint Opcode = 0x4E62E420 | ((EU & 1) << 29) | ((EU >> 1) << 23);

            Vector128<float> V1 = Sse.StaticCast<double, float>(Sse2.SetAllVector128(A));
            Vector128<float> V2 = Sse.StaticCast<double, float>(Sse2.SetAllVector128(B));

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            switch (EU)
            {
                case 0: Exp = (A == B ? Ones : Zeros); break;
                case 1: Exp = (A >= B ? Ones : Zeros); break;
                case 3: Exp = (A >  B ? Ones : Zeros); break;
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(VectorExtractDouble(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(VectorExtractDouble(ThreadState.V0, (byte)1)), Is.EquivalentTo(Exp));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMEQ V0.2S, V1.2S, V2.2S | FCMGE V0.2S, V1.2S, V2.2S | FCMGT V0.2S, V1.2S, V2.2S")]
        public void Fcmeq_Fcmge_Fcmgt_Reg_V_2S([ValueSource("_floats_")] [Random(RndCnt)] float A,
                                               [ValueSource("_floats_")] [Random(RndCnt)] float B,
                                               [Values(0u, 1u, 3u)] uint EU) // EQ, GE, GT
        {
            uint Opcode = 0x0E22E420 | ((EU & 1) << 29) | ((EU >> 1) << 23);

            Vector128<float> V0 = Sse.SetAllVector128(TestContext.CurrentContext.Random.NextFloat());
            Vector128<float> V1 = Sse.SetVector128(0, 0, A, A);
            Vector128<float> V2 = Sse.SetVector128(0, 0, B, B);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1, V2: V2);

            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00};

            switch (EU)
            {
                case 0: Exp = (A == B ? Ones : Zeros); break;
                case 1: Exp = (A >= B ? Ones : Zeros); break;
                case 3: Exp = (A >  B ? Ones : Zeros); break;
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)1)), Is.EquivalentTo(Exp));
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)2), Is.Zero);
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)3), Is.Zero);
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMEQ V0.4S, V1.4S, V2.4S | FCMGE V0.4S, V1.4S, V2.4S | FCMGT V0.4S, V1.4S, V2.4S")]
        public void Fcmeq_Fcmge_Fcmgt_Reg_V_4S([ValueSource("_floats_")] [Random(RndCnt)] float A,
                                               [ValueSource("_floats_")] [Random(RndCnt)] float B,
                                               [Values(0u, 1u, 3u)] uint EU) // EQ, GE, GT
        {
            uint Opcode = 0x4E22E420 | ((EU & 1) << 29) | ((EU >> 1) << 23);

            Vector128<float> V1 = Sse.SetAllVector128(A);
            Vector128<float> V2 = Sse.SetAllVector128(B);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1, V2: V2);

            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00};

            switch (EU)
            {
                case 0: Exp = (A == B ? Ones : Zeros); break;
                case 1: Exp = (A >= B ? Ones : Zeros); break;
                case 3: Exp = (A >  B ? Ones : Zeros); break;
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)1)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)2)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)3)), Is.EquivalentTo(Exp));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMGT D0, D1, #0.0 | FCMGE D0, D1, #0.0 | FCMEQ D0, D1, #0.0 | FCMLE D0, D1, #0.0 | FCMLT D0, D1, #0.0")]
        public void Fcmgt_Fcmge_Fcmeq_Fcmle_Fcmlt_Zero_S_D([ValueSource("_doubles_")] [Random(RndCnt)] double A,
                                                           [Values(0u, 1u, 2u, 3u)] uint opU, // GT, GE, EQ, LE
                                                           [Values(0u, 1u)] uint bit13) // "LT"
        {
            uint Opcode = 0x5EE0C820 | (((opU & 1) & ~bit13) << 29) | (bit13 << 13) | (((opU >> 1) & ~bit13) << 12);

            Vector128<float> V0 = Sse.StaticCast<double, float>(Sse2.SetAllVector128(TestContext.CurrentContext.Random.NextDouble()));
            Vector128<float> V1 = Sse.StaticCast<double, float>(Sse2.SetScalarVector128(A));

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            double Zero  = +0d;
            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            if (bit13 == 0)
            {
                switch (opU)
                {
                    case 0: Exp = (A    >  Zero ? Ones : Zeros); break;
                    case 1: Exp = (A    >= Zero ? Ones : Zeros); break;
                    case 2: Exp = (A    == Zero ? Ones : Zeros); break;
                    case 3: Exp = (Zero >= A    ? Ones : Zeros); break;
                }
            }
            else
            {
                Exp = (Zero > A ? Ones : Zeros);
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(VectorExtractDouble(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(VectorExtractDouble(ThreadState.V0, (byte)1), Is.Zero);
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMGT S0, S1, #0.0 | FCMGE S0, S1, #0.0 | FCMEQ S0, S1, #0.0 | FCMLE S0, S1, #0.0 | FCMLT S0, S1, #0.0")]
        public void Fcmgt_Fcmge_Fcmeq_Fcmle_Fcmlt_Zero_S_S([ValueSource("_floats_")] [Random(RndCnt)] float A,
                                                           [Values(0u, 1u, 2u, 3u)] uint opU, // GT, GE, EQ, LE
                                                           [Values(0u, 1u)] uint bit13) // "LT"
        {
            uint Opcode = 0x5EA0C820 | (((opU & 1) & ~bit13) << 29) | (bit13 << 13) | (((opU >> 1) & ~bit13) << 12);

            Vector128<float> V0 = Sse.SetAllVector128(TestContext.CurrentContext.Random.NextFloat());
            Vector128<float> V1 = Sse.SetScalarVector128(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            float  Zero  = +0f;
            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00};

            if (bit13 == 0)
            {
                switch (opU)
                {
                    case 0: Exp = (A    >  Zero ? Ones : Zeros); break;
                    case 1: Exp = (A    >= Zero ? Ones : Zeros); break;
                    case 2: Exp = (A    == Zero ? Ones : Zeros); break;
                    case 3: Exp = (Zero >= A    ? Ones : Zeros); break;
                }
            }
            else
            {
                Exp = (Zero > A ? Ones : Zeros);
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)1), Is.Zero);
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)2), Is.Zero);
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)3), Is.Zero);
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMGT V0.2D, V1.2D, #0.0 | FCMGE V0.2D, V1.2D, #0.0 | FCMEQ V0.2D, V1.2D, #0.0 | FCMLE V0.2D, V1.2D, #0.0 | FCMLT V0.2D, V1.2D, #0.0")]
        public void Fcmgt_Fcmge_Fcmeq_Fcmle_Fcmlt_Zero_V_2D([ValueSource("_doubles_")] [Random(RndCnt)] double A,
                                                            [Values(0u, 1u, 2u, 3u)] uint opU, // GT, GE, EQ, LE
                                                            [Values(0u, 1u)] uint bit13) // "LT"
        {
            uint Opcode = 0x4EE0C820 | (((opU & 1) & ~bit13) << 29) | (bit13 << 13) | (((opU >> 1) & ~bit13) << 12);

            Vector128<float> V1 = Sse.StaticCast<double, float>(Sse2.SetAllVector128(A));

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1);

            double Zero  = +0d;
            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

            if (bit13 == 0)
            {
                switch (opU)
                {
                    case 0: Exp = (A    >  Zero ? Ones : Zeros); break;
                    case 1: Exp = (A    >= Zero ? Ones : Zeros); break;
                    case 2: Exp = (A    == Zero ? Ones : Zeros); break;
                    case 3: Exp = (Zero >= A    ? Ones : Zeros); break;
                }
            }
            else
            {
                Exp = (Zero > A ? Ones : Zeros);
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(VectorExtractDouble(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(VectorExtractDouble(ThreadState.V0, (byte)1)), Is.EquivalentTo(Exp));
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMGT V0.2S, V1.2S, #0.0 | FCMGE V0.2S, V1.2S, #0.0 | FCMEQ V0.2S, V1.2S, #0.0 | FCMLE V0.2S, V1.2S, #0.0 | FCMLT V0.2S, V1.2S, #0.0")]
        public void Fcmgt_Fcmge_Fcmeq_Fcmle_Fcmlt_Zero_V_2S([ValueSource("_floats_")] [Random(RndCnt)] float A,
                                                            [Values(0u, 1u, 2u, 3u)] uint opU, // GT, GE, EQ, LE
                                                            [Values(0u, 1u)] uint bit13) // "LT"
        {
            uint Opcode = 0x0EA0C820 | (((opU & 1) & ~bit13) << 29) | (bit13 << 13) | (((opU >> 1) & ~bit13) << 12);

            Vector128<float> V0 = Sse.SetAllVector128(TestContext.CurrentContext.Random.NextFloat());
            Vector128<float> V1 = Sse.SetVector128(0, 0, A, A);

            AThreadState ThreadState = SingleOpcode(Opcode, V0: V0, V1: V1);

            float  Zero  = +0f;
            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00};

            if (bit13 == 0)
            {
                switch (opU)
                {
                    case 0: Exp = (A    >  Zero ? Ones : Zeros); break;
                    case 1: Exp = (A    >= Zero ? Ones : Zeros); break;
                    case 2: Exp = (A    == Zero ? Ones : Zeros); break;
                    case 3: Exp = (Zero >= A    ? Ones : Zeros); break;
                }
            }
            else
            {
                Exp = (Zero > A ? Ones : Zeros);
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)1)), Is.EquivalentTo(Exp));
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)2), Is.Zero);
                Assert.That(Sse41.Extract(ThreadState.V0, (byte)3), Is.Zero);
            });

            CompareAgainstUnicorn();
        }

        [Test, Description("FCMGT V0.4S, V1.4S, #0.0 | FCMGE V0.4S, V1.4S, #0.0 | FCMEQ V0.4S, V1.4S, #0.0 | FCMLE V0.4S, V1.4S, #0.0 | FCMLT V0.4S, V1.4S, #0.0")]
        public void Fcmgt_Fcmge_Fcmeq_Fcmle_Fcmlt_Zero_V_4S([ValueSource("_floats_")] [Random(RndCnt)] float A,
                                                            [Values(0u, 1u, 2u, 3u)] uint opU, // GT, GE, EQ, LE
                                                            [Values(0u, 1u)] uint bit13) // "LT"
        {
            uint Opcode = 0x4EA0C820 | (((opU & 1) & ~bit13) << 29) | (bit13 << 13) | (((opU >> 1) & ~bit13) << 12);

            Vector128<float> V1 = Sse.SetAllVector128(A);

            AThreadState ThreadState = SingleOpcode(Opcode, V1: V1);

            float  Zero  = +0f;
            byte[] Exp   = default(byte[]);
            byte[] Ones  = new byte[] {0xFF, 0xFF, 0xFF, 0xFF};
            byte[] Zeros = new byte[] {0x00, 0x00, 0x00, 0x00};

            if (bit13 == 0)
            {
                switch (opU)
                {
                    case 0: Exp = (A    >  Zero ? Ones : Zeros); break;
                    case 1: Exp = (A    >= Zero ? Ones : Zeros); break;
                    case 2: Exp = (A    == Zero ? Ones : Zeros); break;
                    case 3: Exp = (Zero >= A    ? Ones : Zeros); break;
                }
            }
            else
            {
                Exp = (Zero > A ? Ones : Zeros);
            }

            Assert.Multiple(() =>
            {
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)0)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)1)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)2)), Is.EquivalentTo(Exp));
                Assert.That(BitConverter.GetBytes(Sse41.Extract(ThreadState.V0, (byte)3)), Is.EquivalentTo(Exp));
            });

            CompareAgainstUnicorn();
        }
    }
}
