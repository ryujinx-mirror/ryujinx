using System.Numerics;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    static class ImmUtils
    {
        public static uint ExpandImm(uint imm)
        {
            return BitOperations.RotateRight((byte)imm, (int)(imm >> 8) * 2);
        }

        public static bool ExpandedImmRotated(uint imm)
        {
            return (imm >> 8) != 0;
        }

        public static uint ExpandImm(uint imm8, uint imm3, uint i)
        {
            uint imm = CombineImmU12(imm8, imm3, i);

            if (imm >> 10 == 0)
            {
                return ((imm >> 8) & 3) switch
                {
                    0 => (byte)imm,
                    1 => (byte)imm * 0x00010001u,
                    2 => (byte)imm * 0x01000100u,
                    3 => (byte)imm * 0x01010101u,
                    _ => 0,
                };
            }
            else
            {
                return BitOperations.RotateRight(0x80u | (byte)imm, (int)(imm >> 7));
            }
        }

        public static bool ExpandedImmRotated(uint imm8, uint imm3, uint i)
        {
            uint imm = CombineImmU12(imm8, imm3, i);

            return (imm >> 7) != 0;
        }

        public static uint CombineImmU5(uint imm2, uint imm3)
        {
            return imm2 | (imm3 << 2);
        }

        public static uint CombineImmU5IImm4(uint i, uint imm4)
        {
            return i | (imm4 << 1);
        }

        public static uint CombineImmU8(uint imm4l, uint imm4h)
        {
            return imm4l | (imm4h << 4);
        }

        public static uint CombineImmU8(uint imm4, uint imm3, uint i)
        {
            return imm4 | (imm3 << 4) | (i << 7);
        }

        public static uint CombineImmU12(uint imm8, uint imm3, uint i)
        {
            return imm8 | (imm3 << 8) | (i << 11);
        }

        public static uint CombineImmU16(uint imm12, uint imm4)
        {
            return imm12 | (imm4 << 12);
        }

        public static uint CombineImmU16(uint imm8, uint imm3, uint i, uint imm4)
        {
            return imm8 | (imm3 << 8) | (i << 11) | (imm4 << 12);
        }

        public static int CombineSImm20Times2(uint imm11, uint imm6, uint j1, uint j2, uint s)
        {
            int imm32 = (int)(imm11 | (imm6 << 11) | (j1 << 17) | (j2 << 18) | (s << 19));

            return (imm32 << 13) >> 12;
        }

        public static int CombineSImm24Times2(uint imm11, uint imm10, uint j1, uint j2, uint s)
        {
            uint i1 = j1 ^ s ^ 1;
            uint i2 = j2 ^ s ^ 1;

            int imm32 = (int)(imm11 | (imm10 << 11) | (i2 << 21) | (i1 << 22) | (s << 23));

            return (imm32 << 8) >> 7;
        }

        public static int CombineSImm24Times4(uint imm10L, uint imm10H, uint j1, uint j2, uint s)
        {
            uint i1 = j1 ^ s ^ 1;
            uint i2 = j2 ^ s ^ 1;

            int imm32 = (int)(imm10L | (imm10H << 10) | (i2 << 20) | (i1 << 21) | (s << 22));

            return (imm32 << 9) >> 7;
        }

        public static uint CombineRegisterList(uint registerList, uint m)
        {
            return registerList | (m << 14);
        }

        public static uint CombineRegisterList(uint registerList, uint m, uint p)
        {
            return registerList | (m << 14) | (p << 15);
        }

        public static int ExtractSImm24Times4(uint encoding)
        {
            return (int)(encoding << 8) >> 6;
        }

        public static int ExtractT16UImm5Times2(uint encoding)
        {
            return (int)(encoding >> 18) & 0x3e;
        }

        public static int ExtractT16SImm8Times2(uint encoding)
        {
            return (int)(encoding << 24) >> 23;
        }

        public static int ExtractT16SImm11Times2(uint encoding)
        {
            return (int)(encoding << 21) >> 20;
        }
    }
}
