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
#endif
    }
}
