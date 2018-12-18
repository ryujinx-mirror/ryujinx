using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics;

namespace Ryujinx.Tests.Cpu
{
    public class CpuTestSimdArithmetic : CpuTest
    {
        [TestCase(0x00000000u, 0x7F800000u)]
        [TestCase(0x80000000u, 0xFF800000u)]
        [TestCase(0x00FFF000u, 0x7E000000u)]
        [TestCase(0x41200000u, 0x3DCC8000u)]
        [TestCase(0xC1200000u, 0xBDCC8000u)]
        [TestCase(0x001FFFFFu, 0x7F800000u)]
        [TestCase(0x007FF000u, 0x7E800000u)]
        public void Frecpe_S(uint a, uint result)
        {
            uint opcode = 0x5EA1D820; // FRECPE S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x40000000u)]
        [TestCase(0x3F99999Au, false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, false, 0x40400000u)]
        [TestCase(0x40733333u, false, 0x40800000u)]
        [TestCase(0x3FC00000u, false, 0x40000000u)]
        [TestCase(0x40200000u, false, 0x40400000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frinta_S(uint a, bool defaultNaN, uint result)
        {
            uint opcode = 0x1E264020; // FRINTA S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x6E618820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTA V0.2D, V1.2D
        [TestCase(0x6E618820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E618820u, 0x3FF8000000000000ul, 0x3FF8000000000000ul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E218820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTA V0.4S, V1.4S
        [TestCase(0x6E218820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x2E218820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTA V0.2S, V1.2S
        [TestCase(0x2E218820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E218820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E218820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frinta_V(uint opcode, ulong a, ulong b, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, 'N', false, 0x40000000u)]
        [TestCase(0x3F99999Au, 'N', false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, 'P', false, 0x40800000u)]
        [TestCase(0x40733333u, 'P', false, 0x40800000u)]
        [TestCase(0x404CCCCDu, 'M', false, 0x40400000u)]
        [TestCase(0x40733333u, 'M', false, 0x40400000u)]
        [TestCase(0x3F99999Au, 'Z', false, 0x3F800000u)]
        [TestCase(0x3FE66666u, 'Z', false, 0x3F800000u)]
        [TestCase(0x00000000u, 'N', false, 0x00000000u)]
        [TestCase(0x00000000u, 'P', false, 0x00000000u)]
        [TestCase(0x00000000u, 'M', false, 0x00000000u)]
        [TestCase(0x00000000u, 'Z', false, 0x00000000u)]
        [TestCase(0x80000000u, 'N', false, 0x80000000u)]
        [TestCase(0x80000000u, 'P', false, 0x80000000u)]
        [TestCase(0x80000000u, 'M', false, 0x80000000u)]
        [TestCase(0x80000000u, 'Z', false, 0x80000000u)]
        [TestCase(0x7F800000u, 'N', false, 0x7F800000u)]
        [TestCase(0x7F800000u, 'P', false, 0x7F800000u)]
        [TestCase(0x7F800000u, 'M', false, 0x7F800000u)]
        [TestCase(0x7F800000u, 'Z', false, 0x7F800000u)]
        [TestCase(0xFF800000u, 'N', false, 0xFF800000u)]
        [TestCase(0xFF800000u, 'P', false, 0xFF800000u)]
        [TestCase(0xFF800000u, 'M', false, 0xFF800000u)]
        [TestCase(0xFF800000u, 'Z', false, 0xFF800000u)]
        [TestCase(0xFF800001u, 'N', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'P', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'M', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'Z', false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'N', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'P', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'M', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'Z', false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frinti_S(uint a, char roundMode, bool defaultNaN, uint result)
        {
            uint opcode = 0x1E27C020; // FRINTI S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            int fpcrTemp = 0x0;

            switch (roundMode)
            {
                case 'N': fpcrTemp = 0x0;      break;
                case 'P': fpcrTemp = 0x400000; break;
                case 'M': fpcrTemp = 0x800000; break;
                case 'Z': fpcrTemp = 0xC00000; break;
            }

            if (defaultNaN)
            {
                fpcrTemp |= 1 << 25;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'N', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTI V0.2D, V1.2D
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'N', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EE19820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTI V0.4S, V1.4S
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x6EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTI V0.2S, V1.2S
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'N', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'P', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'M', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x0000000080000000ul, 0x0000000000000000ul, 'Z', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'N', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'P', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'M', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'Z', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frinti_V(uint opcode, ulong a, ulong b, char roundMode, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            switch (roundMode)
            {
                case 'N': fpcrTemp = 0x0;      break;
                case 'P': fpcrTemp = 0x400000; break;
                case 'M': fpcrTemp = 0x800000; break;
                case 'Z': fpcrTemp = 0xC00000; break;
            }

            if (defaultNaN)
            {
                fpcrTemp |= 1 << 25;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x3F800000u)]
        [TestCase(0x3F99999Au, false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, false, 0x40400000u)]
        [TestCase(0x40733333u, false, 0x40400000u)]
        [TestCase(0x3FC00000u, false, 0x3F800000u)]
        [TestCase(0x40200000u, false, 0x40000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintm_S(uint a, bool defaultNaN, uint result)
        {
            uint opcode = 0x1E254020; // FRINTM S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTM V0.2D, V1.2D
        [TestCase(0x4E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x4E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)] // FRINTM V0.4S, V1.4S
        [TestCase(0x0E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F8000003F800000ul, 0x0000000000000000ul)] // FRINTM V0.2S, V1.2S
        [TestCase(0x0E219820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x0E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x0E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintm_V(uint opcode, ulong a, ulong b, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x40000000u)]
        [TestCase(0x3F99999Au, false, 0x3F800000u)]
        [TestCase(0x404CCCCDu, false, 0x40400000u)]
        [TestCase(0x40733333u, false, 0x40800000u)]
        [TestCase(0x3FC00000u, false, 0x40000000u)]
        [TestCase(0x40200000u, false, 0x40000000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintn_S(uint a, bool defaultNaN, uint result)
        {
            uint opcode = 0x1E244020; // FRINTN S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4E618820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTN V0.2D, V1.2D
        [TestCase(0x4E618820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x4E618820u, 0x3FF8000000000000ul, 0x3FF8000000000000ul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x4E218820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTN V0.4S, V1.4S
        [TestCase(0x4E218820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x0E218820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTN V0.2S, V1.2S
        [TestCase(0x0E218820u, 0x3FC000003FC00000ul, 0x3FC000003FC00000ul, false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x0E218820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x0E218820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x0E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0E218820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintn_V(uint opcode, ulong a, ulong b, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x3FE66666u, false, 0x40000000u)]
        [TestCase(0x3F99999Au, false, 0x40000000u)]
        [TestCase(0x404CCCCDu, false, 0x40800000u)]
        [TestCase(0x40733333u, false, 0x40800000u)]
        [TestCase(0x3FC00000u, false, 0x40000000u)]
        [TestCase(0x40200000u, false, 0x40400000u)]
        [TestCase(0x00000000u, false, 0x00000000u)]
        [TestCase(0x80000000u, false, 0x80000000u)]
        [TestCase(0x7F800000u, false, 0x7F800000u)]
        [TestCase(0xFF800000u, false, 0xFF800000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintp_S(uint a, bool defaultNaN, uint result)
        {
            uint opcode = 0x1E24C020; // FRINTP S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4EE18820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, false, 0x4000000000000000ul, 0x4000000000000000ul)] // FRINTP V0.2D, v1.2D
        [TestCase(0x4EE18820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x4EA18820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x4000000040000000ul, 0x4000000040000000ul)] // FRINTP V0.4S, v1.4S
        [TestCase(0x0EA18820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, false, 0x4000000040000000ul, 0x0000000000000000ul)] // FRINTP V0.2S, v1.2S
        [TestCase(0x0EA18820u, 0x0000000080000000ul, 0x0000000000000000ul, false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x0EA18820u, 0x7F800000FF800000ul, 0x0000000000000000ul, false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x0EA18820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0EA18820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintp_V(uint opcode, ulong a, ulong b, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

    	[TestCase(0x3FE66666u, 'N', false, 0x40000000u)]
    	[TestCase(0x3F99999Au, 'N', false, 0x3F800000u)]
    	[TestCase(0x404CCCCDu, 'P', false, 0x40800000u)]
    	[TestCase(0x40733333u, 'P', false, 0x40800000u)]
    	[TestCase(0x404CCCCDu, 'M', false, 0x40400000u)]
    	[TestCase(0x40733333u, 'M', false, 0x40400000u)]
    	[TestCase(0x3F99999Au, 'Z', false, 0x3F800000u)]
    	[TestCase(0x3FE66666u, 'Z', false, 0x3F800000u)]
    	[TestCase(0x00000000u, 'N', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'P', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'M', false, 0x00000000u)]
    	[TestCase(0x00000000u, 'Z', false, 0x00000000u)]
    	[TestCase(0x80000000u, 'N', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'P', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'M', false, 0x80000000u)]
    	[TestCase(0x80000000u, 'Z', false, 0x80000000u)]
    	[TestCase(0x7F800000u, 'N', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'P', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'M', false, 0x7F800000u)]
    	[TestCase(0x7F800000u, 'Z', false, 0x7F800000u)]
    	[TestCase(0xFF800000u, 'N', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'P', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'M', false, 0xFF800000u)]
    	[TestCase(0xFF800000u, 'Z', false, 0xFF800000u)]
    	[TestCase(0xFF800001u, 'N', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'P', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'M', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'Z', false, 0xFFC00001u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0xFF800001u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'N', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'P', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'M', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'Z', false, 0x7FC00002u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'N', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'P', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'M', true,  0x7FC00000u, Ignore = "NaN test.")]
    	[TestCase(0x7FC00002u, 'Z', true,  0x7FC00000u, Ignore = "NaN test.")]
    	public void Frintx_S(uint a, char roundMode, bool defaultNaN, uint result)
    	{
            uint opcode = 0x1E274020; // FRINTX S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

        	int fpcrTemp = 0x0;

        	switch (roundMode)
        	{
        		case 'N': fpcrTemp = 0x0;      break;
        		case 'P': fpcrTemp = 0x400000; break;
        		case 'M': fpcrTemp = 0x800000; break;
        		case 'Z': fpcrTemp = 0xC00000; break;
        	}

        	if (defaultNaN)
        	{
        		fpcrTemp |= 1 << 25;
        	}

        	CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

        	Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'N', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)] // FRINTX V0.2D, V1.2D
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'N', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'P', false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'M', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E619820u, 0x3FF3333333333333ul, 0x3FF3333333333333ul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E619820u, 0x3FFCCCCCCCCCCCCDul, 0x3FFCCCCCCCCCCCCDul, 'Z', false, 0x3FF0000000000000ul, 0x3FF0000000000000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x3F80000040000000ul)] // FRINTX V0.4S, V1.4S
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x4000000040000000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x6E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x3F8000003F800000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'N', false, 0x3F80000040000000ul, 0x0000000000000000ul)] // FRINTX V0.2S, V1.2S
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'P', false, 0x4000000040000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'M', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x3F99999A3FE66666ul, 0x3F99999A3FE66666ul, 'Z', false, 0x3F8000003F800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'N', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'P', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'M', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x0000000080000000ul, 0x0000000000000000ul, 'Z', false, 0x0000000080000000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'N', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'P', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'M', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0x7F800000FF800000ul, 0x0000000000000000ul, 'Z', false, 0x7F800000FF800000ul, 0x0000000000000000ul)]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'N', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'P', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'M', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x2E219820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, 'Z', true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintx_V(uint opcode, ulong a, ulong b, char roundMode, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            switch (roundMode)
            {
                case 'N': fpcrTemp = 0x0;      break;
                case 'P': fpcrTemp = 0x400000; break;
                case 'M': fpcrTemp = 0x800000; break;
                case 'Z': fpcrTemp = 0xC00000; break;
            }

            if (defaultNaN)
            {
                fpcrTemp |= 1 << 25;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0xBFF33333u, false, 0xBF800000u)]
        [TestCase(0x40200000u, false, 0x40000000u)]
        [TestCase(0xFF800001u, false, 0xFFC00001u, Ignore = "NaN test.")]
        [TestCase(0xFF800001u, true,  0x7FC00000u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, false, 0x7FC00002u, Ignore = "NaN test.")]
        [TestCase(0x7FC00002u, true,  0x7FC00000u, Ignore = "NaN test.")]
        public void Frintz_S(uint a, bool defaultNaN, uint result)
        {
            uint opcode = 0x1E25C020; // FRINTZ S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }

        [TestCase(0x4EE19820u, 0xBFF999999999999Aul, 0xBFF999999999999Aul, false, 0xBFF0000000000000ul, 0xBFF0000000000000ul)] // FRINTZ V0.2D, V1.2D
        [TestCase(0x4EE19820u, 0x4004000000000000ul, 0x4004000000000000ul, false, 0x4000000000000000ul, 0x4000000000000000ul)]
        [TestCase(0x0EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, false, 0xFFC000017FC00002ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        [TestCase(0x0EA19820u, 0xFF8000017FC00002ul, 0x0000000000000000ul, true,  0x7FC000007FC00000ul, 0x0000000000000000ul, Ignore = "NaN test.")]
        public void Frintz_V(uint opcode, ulong a, ulong b, bool defaultNaN, ulong result0, ulong result1)
        {
            Vector128<float> v1 = MakeVectorE0E1(a, b);

            int fpcrTemp = 0x0;

            if (defaultNaN)
            {
                fpcrTemp = 0x2000000;
            }

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1, fpcr: fpcrTemp);

            Assert.Multiple(() =>
            {
                Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result0));
                Assert.That(GetVectorE1(threadState.V0), Is.EqualTo(result1));
            });

            CompareAgainstUnicorn();
        }

        [TestCase(0x41200000u, 0x3EA18000u)]
        public void Frsqrte_S(uint a, uint result)
        {
            uint opcode = 0x7EA1D820; // FRSQRTE S0, S1

            Vector128<float> v1 = MakeVectorE0(a);

            CpuThreadState threadState = SingleOpcode(opcode, v1: v1);

            Assert.That(GetVectorE0(threadState.V0), Is.EqualTo(result));

            CompareAgainstUnicorn();
        }
    }
}
