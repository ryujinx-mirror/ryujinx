#define Alu32

using NUnit.Framework;
using System;

namespace Ryujinx.Tests.Cpu
{
    [Category("Alu32")]
    public sealed class CpuTestAlu32 : CpuTest32
    {
#if Alu32

#region "ValueSource (Opcodes)"
        private static uint[] _Ssat_Usat_()
        {
            return new uint[]
            {
                0xe6a00010u, // SSAT R0, #1, R0, LSL #0
                0xe6a00050u, // SSAT R0, #1, R0, ASR #32
                0xe6e00010u, // USAT R0, #0, R0, LSL #0
                0xe6e00050u  // USAT R0, #0, R0, ASR #32
            };
        }

        private static uint[] _Ssat16_Usat16_()
        {
            return new uint[]
            {
                0xe6a00f30u, // SSAT16 R0, #1, R0
                0xe6e00f30u, // USAT16 R0, #0, R0
            };
        }

        private static uint[] _Lsr_Lsl_Asr_Ror_()
        {
            return new uint[]
            {
                0xe1b00030u, // LSRS R0, R0, R0
                0xe1b00010u, // LSLS R0, R0, R0
                0xe1b00050u, // ASRS R0, R0, R0
                0xe1b00070u  // RORS R0, R0, R0
            };
        }
#endregion

        private const int RndCnt = 2;

        [Test, Pairwise, Description("RBIT <Rd>, <Rn>")]
        public void Rbit_32bit([Values(0u, 0xdu)] uint rd,
                               [Values(1u, 0xdu)] uint rm,
                               [Values(0x00000000u, 0x7FFFFFFFu,
                                       0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            uint opcode = 0xe6ff0f30u; // RBIT R0, R0
            opcode |= ((rm & 15) << 0) | ((rd & 15) << 12);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, r1: wn, sp: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Lsr_Lsl_Asr_Ror([ValueSource("_Lsr_Lsl_Asr_Ror_")] uint opcode,
                                    [Values(0x00000000u, 0x7FFFFFFFu,
                                            0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint shiftValue,
                                    [Range(0, 31)] [Values(32, 256, 768, -1, -23)] int shiftAmount)
        {
            uint rd = 0;
            uint rm = 1;
            uint rs = 2;
            opcode |= ((rm & 15) << 0) | ((rd & 15) << 12) | ((rs & 15) << 8);

            SingleOpcode(opcode, r1: shiftValue, r2: (uint)shiftAmount);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Ssat_Usat([ValueSource("_Ssat_Usat_")] uint opcode,
                              [Values(0u, 0xdu)] uint rd,
                              [Values(1u, 0xdu)] uint rn,
                              [Values(0u, 7u, 8u, 0xfu, 0x10u, 0x1fu)] uint sat,
                              [Values(0u, 7u, 8u, 0xfu, 0x10u, 0x1fu)] uint shift,
                              [Values(0x00000000u, 0x7FFFFFFFu,
                                      0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            opcode |= ((rn & 15) << 0) | ((shift & 31) << 7) | ((rd & 15) << 12) | ((sat & 31) << 16);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, r1: wn, sp: w31);

            CompareAgainstUnicorn();
        }

        [Test, Pairwise]
        public void Ssat16_Usat16([ValueSource("_Ssat16_Usat16_")] uint opcode,
                                  [Values(0u, 0xdu)] uint rd,
                                  [Values(1u, 0xdu)] uint rn,
                                  [Values(0u, 7u, 8u, 0xfu)] uint sat,
                                  [Values(0x00000000u, 0x7FFFFFFFu,
                                          0x80000000u, 0xFFFFFFFFu)] [Random(RndCnt)] uint wn)
        {
            opcode |= ((rn & 15) << 0) | ((rd & 15) << 12) | ((sat & 15) << 16);

            uint w31 = TestContext.CurrentContext.Random.NextUInt();

            SingleOpcode(opcode, r1: wn, sp: w31);

            CompareAgainstUnicorn();
        }
#endif
    }
}
