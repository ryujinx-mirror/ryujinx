#define SimdShImm32

using ARMeilleure.State;
using NUnit.Framework;

namespace Ryujinx.Tests.Cpu
{
    [Category("SimdShImm32")]
    public sealed class CpuTestSimdShImm32 : CpuTest32
    {
#if SimdShImm32
        private const int RndCnt = 2;

        [Test, Pairwise, Description("VSHL.<size> {<Vd>}, <Vm>, #<imm>")]
        public void Vshl_Imm([Values(0u)] uint rd,
                             [Values(2u, 0u)] uint rm,
                             [Values(0u, 1u, 2u, 3u)] uint size,
                             [Random(RndCnt), Values(0u)] uint shiftImm,
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

        [Test, Pairwise, Description("VSHR.<size> {<Vd>}, <Vm>, #<imm>")]
        public void Vshr_Imm([Values(0u)] uint rd,
                             [Values(2u, 0u)] uint rm,
                             [Values(0u, 1u, 2u, 3u)] uint size,
                             [Random(RndCnt), Values(0u)] uint shiftImm,
                             [Random(RndCnt)] ulong z,
                             [Random(RndCnt)] ulong a,
                             [Random(RndCnt)] ulong b,
                             [Values] bool u,
                             [Values] bool q)
        {
            uint opcode = 0xf2800010u; // VMOV.I32 D0, #0 (immediate value changes it into SHR)
            if (q)
            {
                opcode |= 1 << 6;
                rm <<= 1;
                rd <<= 1;
            }

            if (u)
            {
                opcode |= 1 << 24;
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

        [Test, Pairwise, Description("VSHRN.<size> {<Vd>}, <Vm>, #<imm>")]
        public void Vshrn_Imm([Values(0u, 1u)] uint rd,
                              [Values(2u, 0u)] uint rm,
                              [Values(0u, 1u, 2u)] uint size,
                              [Random(RndCnt), Values(0u)] uint shiftImm,
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
#endif
    }
}
