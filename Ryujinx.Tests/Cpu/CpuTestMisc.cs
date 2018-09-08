using ChocolArm64.State;

using NUnit.Framework;

using System.Runtime.Intrinsics.X86;

namespace Ryujinx.Tests.Cpu
{
    [Category("Misc"), Explicit]
    public sealed class CpuTestMisc : CpuTest
    {
        [TestCase(0xFFFFFFFDu)] // Roots.
        [TestCase(0x00000005u)]
        public void Misc1(uint A)
        {
            // ((A + 3) * (A - 5)) / ((A + 5) * (A - 3)) = 0

            /*
            ADD W2, W0, 3
            SUB W1, W0, #5
            MUL W2, W2, W1
            ADD W1, W0, 5
            SUB W0, W0, #3
            MUL W0, W1, W0
            SDIV W0, W2, W0
            BRK #0
            RET
            */

            SetThreadState(X0: A);
            Opcode(0x11000C02);
            Opcode(0x51001401);
            Opcode(0x1B017C42);
            Opcode(0x11001401);
            Opcode(0x51000C00);
            Opcode(0x1B007C20);
            Opcode(0x1AC00C40);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            ExecuteOpcodes();

            Assert.That(GetThreadState().X0, Is.Zero);
        }

        [TestCase(-20f,  -5f)] // 18 integer solutions.
        [TestCase(-12f,  -6f)]
        [TestCase(-12f,   3f)]
        [TestCase( -8f,  -8f)]
        [TestCase( -6f, -12f)]
        [TestCase( -5f, -20f)]
        [TestCase( -4f,   2f)]
        [TestCase( -3f,  12f)]
        [TestCase( -2f,   4f)]
        [TestCase(  2f,  -4f)]
        [TestCase(  3f, -12f)]
        [TestCase(  4f,  -2f)]
        [TestCase(  5f,  20f)]
        [TestCase(  6f,  12f)]
        [TestCase(  8f,   8f)]
        [TestCase( 12f,  -3f)]
        [TestCase( 12f,   6f)]
        [TestCase( 20f,   5f)]
        public void Misc2(float A, float B)
        {
            // 1 / ((1 / A + 1 / B) ^ 2) = 16

            /*
            FMOV S2, 1.0e+0
            FDIV S0, S2, S0
            FDIV S1, S2, S1
            FADD S0, S0, S1
            FDIV S0, S2, S0
            FMUL S0, S0, S0
            BRK #0
            RET
            */

            SetThreadState(
                V0: Sse.SetScalarVector128(A),
                V1: Sse.SetScalarVector128(B));
            Opcode(0x1E2E1002);
            Opcode(0x1E201840);
            Opcode(0x1E211841);
            Opcode(0x1E212800);
            Opcode(0x1E201840);
            Opcode(0x1E200800);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            ExecuteOpcodes();

            Assert.That(Sse41.Extract(GetThreadState().V0, (byte)0), Is.EqualTo(16f));
        }

        [TestCase(-20d,  -5d)] // 18 integer solutions.
        [TestCase(-12d,  -6d)]
        [TestCase(-12d,   3d)]
        [TestCase( -8d,  -8d)]
        [TestCase( -6d, -12d)]
        [TestCase( -5d, -20d)]
        [TestCase( -4d,   2d)]
        [TestCase( -3d,  12d)]
        [TestCase( -2d,   4d)]
        [TestCase(  2d,  -4d)]
        [TestCase(  3d, -12d)]
        [TestCase(  4d,  -2d)]
        [TestCase(  5d,  20d)]
        [TestCase(  6d,  12d)]
        [TestCase(  8d,   8d)]
        [TestCase( 12d,  -3d)]
        [TestCase( 12d,   6d)]
        [TestCase( 20d,   5d)]
        public void Misc3(double A, double B)
        {
            // 1 / ((1 / A + 1 / B) ^ 2) = 16

            /*
            FMOV D2, 1.0e+0
            FDIV D0, D2, D0
            FDIV D1, D2, D1
            FADD D0, D0, D1
            FDIV D0, D2, D0
            FMUL D0, D0, D0
            BRK #0
            RET
            */

            SetThreadState(
                V0: Sse.StaticCast<double, float>(Sse2.SetScalarVector128(A)),
                V1: Sse.StaticCast<double, float>(Sse2.SetScalarVector128(B)));
            Opcode(0x1E6E1002);
            Opcode(0x1E601840);
            Opcode(0x1E611841);
            Opcode(0x1E612800);
            Opcode(0x1E601840);
            Opcode(0x1E600800);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            ExecuteOpcodes();

            Assert.That(VectorExtractDouble(GetThreadState().V0, (byte)0), Is.EqualTo(16d));
        }

        [Test]
        public void MiscF([Range(0u, 92u, 1u)] uint A)
        {
            ulong F_n(uint n)
            {
                ulong a = 0, b = 1, c;

                if (n == 0)
                {
                    return a;
                }

                for (uint i = 2; i <= n; i++)
                {
                    c = a + b;
                    a = b;
                    b = c;
                }

                return b;
            }

            /*
            0x0000000000001000: MOV W4, W0
            0x0000000000001004: CBZ W0, #0x3C
            0x0000000000001008: CMP W0, #1
            0x000000000000100C: B.LS #0x48
            0x0000000000001010: MOVZ W2, #0x2
            0x0000000000001014: MOVZ X1, #0x1
            0x0000000000001018: MOVZ X3, #0
            0x000000000000101C: ADD X0, X3, X1
            0x0000000000001020: ADD W2, W2, #1
            0x0000000000001024: MOV X3, X1
            0x0000000000001028: MOV X1, X0
            0x000000000000102C: CMP W4, W2
            0x0000000000001030: B.HS #0x1C
            0x0000000000001034: BRK #0
            0x0000000000001038: RET
            0x000000000000103C: MOVZ X0, #0
            0x0000000000001040: BRK #0
            0x0000000000001044: RET
            0x0000000000001048: MOVZ X0, #0x1
            0x000000000000104C: BRK #0
            0x0000000000001050: RET
            */

            SetThreadState(X0: A);
            Opcode(0x2A0003E4);
            Opcode(0x340001C0);
            Opcode(0x7100041F);
            Opcode(0x540001E9);
            Opcode(0x52800042);
            Opcode(0xD2800021);
            Opcode(0xD2800003);
            Opcode(0x8B010060);
            Opcode(0x11000442);
            Opcode(0xAA0103E3);
            Opcode(0xAA0003E1);
            Opcode(0x6B02009F);
            Opcode(0x54FFFF62);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            Opcode(0xD2800000);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            Opcode(0xD2800020);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            ExecuteOpcodes();

            Assert.That(GetThreadState().X0, Is.EqualTo(F_n(A)));
        }

        [Test]
        public void MiscR()
        {
            const ulong Result = 5;

            /*
            0x0000000000001000: MOV X0, #2
            0x0000000000001004: MOV X1, #3
            0x0000000000001008: ADD X0, X0, X1
            0x000000000000100C: BRK #0
            0x0000000000001010: RET
            */

            Opcode(0xD2800040);
            Opcode(0xD2800061);
            Opcode(0x8B010000);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            ExecuteOpcodes();

            Assert.That(GetThreadState().X0, Is.EqualTo(Result));

            Reset();

            /*
            0x0000000000001000: MOV X0, #3
            0x0000000000001004: MOV X1, #2
            0x0000000000001008: ADD X0, X0, X1
            0x000000000000100C: BRK #0
            0x0000000000001010: RET
            */

            Opcode(0xD2800060);
            Opcode(0xD2800041);
            Opcode(0x8B010000);
            Opcode(0xD4200000);
            Opcode(0xD65F03C0);
            ExecuteOpcodes();

            Assert.That(GetThreadState().X0, Is.EqualTo(Result));
        }

        [TestCase( 0ul)]
        [TestCase( 1ul)]
        [TestCase( 2ul)]
        [TestCase(42ul)]
        public void SanityCheck(ulong A)
        {
            uint Opcode = 0xD503201F; // NOP
            AThreadState ThreadState = SingleOpcode(Opcode, X0: A);

            Assert.That(ThreadState.X0, Is.EqualTo(A));
        }
    }
}
