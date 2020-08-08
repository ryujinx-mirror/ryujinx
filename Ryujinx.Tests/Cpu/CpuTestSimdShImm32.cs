#define SimdShImm32

using ARMeilleure.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdShImm32")]
    public sealed class CpuTestSimdShImm32 : CpuTest32
    {
#if SimdShImm32

#region "ValueSource (Types)"
        private static ulong[] _1D_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFFFFFFFFFFul,
                                 0x8000000000000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _2S_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFFFFFF7FFFFFFFul,
                                 0x8000000080000000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _4H_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7FFF7FFF7FFF7FFFul,
                                 0x8000800080008000ul, 0xFFFFFFFFFFFFFFFFul };
        }

        private static ulong[] _8B_()
        {
            return new ulong[] { 0x0000000000000000ul, 0x7F7F7F7F7F7F7F7Ful,
                                 0x8080808080808080ul, 0xFFFFFFFFFFFFFFFFul };
        }
#endregion

#region "ValueSource (Opcodes)"
        private static uint[] _Vshr_Imm_SU8_()
        {
            return new uint[]
            {
                0xf2880110u, // VSRA.S8  D0, D0, #8
                0xf2880210u, // VRSHR.S8 D0, D0, #8
                0xf2880010u  // VSHR.S8  D0, D0, #8
            };
        }

        private static uint[] _Vshr_Imm_SU16_()
        {
            return new uint[]
            {
                0xf2900110u, // VSRA.S16  D0, D0, #16
                0xf2900210u, // VRSHR.S16 D0, D0, #16
                0xf2900010u  // VSHR.S16  D0, D0, #16
            };
        }

        private static uint[] _Vshr_Imm_SU32_()
        {
            return new uint[]
            {
                0xf2a00110u, // VSRA.S32  D0, D0, #32
                0xf2a00210u, // VRSHR.S32 D0, D0, #32
                0xf2a00010u  // VSHR.S32  D0, D0, #32
            };
        }

        private static uint[] _Vshr_Imm_SU64_()
        {
            return new uint[]
            {
                0xf2800190u, // VSRA.S64  D0, D0, #64
                0xf2800290u, // VRSHR.S64 D0, D0, #64
                0xf2800090u  // VSHR.S64  D0, D0, #64
            };
        }
#endregion

        private const int RndCnt = 2;
        private const int RndCntShiftImm = 2;

        [Test, Pairwise]
        public void Vshr_Imm_SU8([ValueSource("_Vshr_Imm_SU8_")] uint opcode,
                                 [Range(0u, 3u)] uint rd,
                                 [Range(0u, 3u)] uint rm,
                                 [ValueSource("_8B_")] [Random(RndCnt)] ulong z,
                                 [ValueSource("_8B_")] [Random(RndCnt)] ulong b,
                                 [Values(1u, 8u)] [Random(2u, 7u, RndCntShiftImm)] uint shiftImm,
                                 [Values] bool u,
                                 [Values] bool q)
        {
            uint imm6 = 16 - shiftImm;

            Vshr_Imm_SU(opcode, rd, rm, z, b, imm6, u, q);
        }

        [Test, Pairwise]
        public void Vshr_Imm_SU16([ValueSource("_Vshr_Imm_SU16_")] uint opcode,
                                  [Range(0u, 3u)] uint rd,
                                  [Range(0u, 3u)] uint rm,
                                  [ValueSource("_4H_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_4H_")] [Random(RndCnt)] ulong b,
                                  [Values(1u, 16u)] [Random(2u, 15u, RndCntShiftImm)] uint shiftImm,
                                  [Values] bool u,
                                  [Values] bool q)
        {
            uint imm6 = 32 - shiftImm;

            Vshr_Imm_SU(opcode, rd, rm, z, b, imm6, u, q);
        }

        [Test, Pairwise]
        public void Vshr_Imm_SU32([ValueSource("_Vshr_Imm_SU32_")] uint opcode,
                                  [Range(0u, 3u)] uint rd,
                                  [Range(0u, 3u)] uint rm,
                                  [ValueSource("_2S_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_2S_")] [Random(RndCnt)] ulong b,
                                  [Values(1u, 32u)] [Random(2u, 31u, RndCntShiftImm)] uint shiftImm,
                                  [Values] bool u,
                                  [Values] bool q)
        {
            uint imm6 = 64 - shiftImm;

            Vshr_Imm_SU(opcode, rd, rm, z, b, imm6, u, q);
        }

        [Test, Pairwise]
        public void Vshr_Imm_SU64([ValueSource("_Vshr_Imm_SU64_")] uint opcode,
                                  [Range(0u, 3u)] uint rd,
                                  [Range(0u, 3u)] uint rm,
                                  [ValueSource("_1D_")] [Random(RndCnt)] ulong z,
                                  [ValueSource("_1D_")] [Random(RndCnt)] ulong b,
                                  [Values(1u, 64u)] [Random(2u, 63u, RndCntShiftImm)] uint shiftImm,
                                  [Values] bool u,
                                  [Values] bool q)
        {
            uint imm6 = 64 - shiftImm;

            Vshr_Imm_SU(opcode, rd, rm, z, b, imm6, u, q);
        }

        private void Vshr_Imm_SU(uint opcode, uint rd, uint rm, ulong z, ulong b, uint imm6, bool u, bool q)
        {
            if (u)
            {
                opcode |= 1 << 24;
            }

            if (q)
            {
                opcode |= 1 << 6;

                rd >>= 1; rd <<= 1;
                rm >>= 1; rm <<= 1;
            }

            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((rm & 0xf) << 0)  | ((rm & 0x10) << 1);

            opcode |= (imm6 & 0x3f) << 16;

            V128 v0 = MakeVectorE0E1(z, ~z);
            V128 v1 = MakeVectorE0E1(b, ~b);

            SingleOpcode(opcode, v0: v0, v1: v1);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("VSHL.<size> {<Vd>}, <Vm>, #<imm>")]
        public void Vshl_Imm([Values(0u)] uint rd,
                             [Values(2u, 0u)] uint rm,
                             [Values(0u, 1u, 2u, 3u)] uint size,
                             [Random(RndCntShiftImm)] [Values(0u)] uint shiftImm,
                             [Random(RndCnt)] ulong z,
                             [Random(RndCnt)] ulong a,
                             [Random(RndCnt)] ulong b,
                             [Values] bool q)
        {
            uint opcode = 0xf2800510u; // VORR.I32 D0, #0 (immediate value changes it into SHL)
            if (q)
            {
                opcode |= 1 << 6;
                rm <<= 1;
                rd <<= 1;
            }

            uint imm = 1u << ((int)size + 3);
            imm |= shiftImm & (imm - 1);

            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((imm & 0x3f) << 16) | ((imm & 0x40) << 1);

            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(a, z);
            V128 v2 = MakeVectorE0E1(b, z);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("VSHRN.<size> <Vd>, <Vm>, #<imm>")]
        public void Vshrn_Imm([Values(0u, 1u)] uint rd,
                              [Values(2u, 0u)] uint rm,
                              [Values(0u, 1u, 2u)] uint size,
                              [Random(RndCntShiftImm)] [Values(0u)] uint shiftImm,
                              [Random(RndCnt)] ulong z,
                              [Random(RndCnt)] ulong a,
                              [Random(RndCnt)] ulong b)
        {
            uint opcode = 0xf2800810u; // VMOV.I16 D0, #0 (immediate value changes it into SHRN)

            uint imm = 1u << ((int)size + 3);
            imm |= shiftImm & (imm - 1);

            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((imm & 0x3f) << 16);

            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(a, z);
            V128 v2 = MakeVectorE0E1(b, z);

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise, Description("VQRSHRN.<type><size> <Vd>, <Vm>, #<imm>")]
        public void Vqrshrn_Imm([Values(0u, 1u)] uint rd,
                                [Values(2u, 0u)] uint rm,
                                [Values(0u, 1u, 2u)] uint size,
                                [Random(RndCntShiftImm)] [Values(0u)] uint shiftImm,
                                [Random(RndCnt)] ulong z,
                                [Random(RndCnt)] ulong a,
                                [Random(RndCnt)] ulong b,
                                [Values] bool u)
        {
            uint opcode = 0xf2800950u; // VORR.I16 Q0, #0 (immediate value changes it into QRSHRN)

            uint imm = 1u << ((int)size + 3);
            imm |= shiftImm & (imm - 1);

            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((imm & 0x3f) << 16);

            if (u)
            {
                opcode |= 1u << 24;
            }

            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(a, z);
            V128 v2 = MakeVectorE0E1(b, z);

            int fpscr = (int)TestContext.CurrentContext.Random.NextUInt() & (int)Fpsr.Qc;

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2, fpscr: fpscr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }

        [Test, Pairwise, Description("VQRSHRUN.<type><size> <Vd>, <Vm>, #<imm>")]
        public void Vqrshrun_Imm([Values(0u, 1u)] uint rd,
                                 [Values(2u, 0u)] uint rm,
                                 [Values(0u, 1u, 2u)] uint size,
                                 [Random(RndCntShiftImm)] [Values(0u)] uint shiftImm,
                                 [Random(RndCnt)] ulong z,
                                 [Random(RndCnt)] ulong a,
                                 [Random(RndCnt)] ulong b)
        {
            uint opcode = 0xf3800850u; // VMOV.I16 Q0, #0x80 (immediate value changes it into QRSHRUN)

            uint imm = 1u << ((int)size + 3);
            imm |= shiftImm & (imm - 1);

            opcode |= ((rm & 0xf) << 0) | ((rm & 0x10) << 1);
            opcode |= ((rd & 0xf) << 12) | ((rd & 0x10) << 18);
            opcode |= ((imm & 0x3f) << 16);

            V128 v0 = MakeVectorE0E1(z, z);
            V128 v1 = MakeVectorE0E1(a, z);
            V128 v2 = MakeVectorE0E1(b, z);

            int fpscr = (int)TestContext.CurrentContext.Random.NextUInt() & (int)Fpsr.Qc;

            SingleOpcode(opcode, v0: v0, v1: v1, v2: v2, fpscr: fpscr);

            CompareAgainstUnicorn(fpsrMask: Fpsr.Qc);
        }
#endif
    }
}
