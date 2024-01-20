using System;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    static class RegisterUtils
    {
        public const int SpRegister = 13;
        public const int LrRegister = 14;
        public const int PcRegister = 15;

        private const int RmBit = 0;
        private const int RdRtBit = 12;
        private const int RdHiRnBit = 16;

        private const int RdRtT16Bit = 16;
        private const int RdRtT16AltBit = 24;

        private const int RdRt2RdHiT32Bit = 8;
        private const int RdT32AltBit = 0;
        private const int RtRdLoT32Bit = 12;

        public static int ExtractRt(uint encoding)
        {
            return (int)(encoding >> RdRtBit) & 0xf;
        }

        public static int ExtractRt2(uint encoding)
        {
            return (int)GetRt2((uint)ExtractRt(encoding));
        }

        public static int ExtractRd(InstFlags flags, uint encoding)
        {
            return flags.HasFlag(InstFlags.Rd16) ? ExtractRn(encoding) : ExtractRd(encoding);
        }

        public static int ExtractRd(uint encoding)
        {
            return (int)(encoding >> RdRtBit) & 0xf;
        }

        public static int ExtractRdHi(uint encoding)
        {
            return (int)(encoding >> RdHiRnBit) & 0xf;
        }

        public static int ExtractRn(uint encoding)
        {
            return (int)(encoding >> RdHiRnBit) & 0xf;
        }

        public static int ExtractRm(uint encoding)
        {
            return (int)(encoding >> RmBit) & 0xf;
        }

        public static uint GetRt2(uint rt)
        {
            return Math.Min(rt + 1, PcRegister);
        }

        public static int ExtractRdn(InstFlags flags, uint encoding)
        {
            if (flags.HasFlag(InstFlags.Dn))
            {
                return ((int)(encoding >> RdRtT16Bit) & 7) | (int)((encoding >> 4) & 8);
            }
            else
            {
                return ExtractRdT16(flags, encoding);
            }
        }

        public static int ExtractRdT16(InstFlags flags, uint encoding)
        {
            return flags.HasFlag(InstFlags.Rd16) ? (int)(encoding >> RdRtT16AltBit) & 7 : (int)(encoding >> RdRtT16Bit) & 7;
        }

        public static int ExtractRtT16(InstFlags flags, uint encoding)
        {
            return flags.HasFlag(InstFlags.Rd16) ? (int)(encoding >> RdRtT16AltBit) & 7 : (int)(encoding >> RdRtT16Bit) & 7;
        }

        public static int ExtractRdT32(InstFlags flags, uint encoding)
        {
            return flags.HasFlag(InstFlags.Rd16) ? (int)(encoding >> RdT32AltBit) & 0xf : (int)(encoding >> RdRt2RdHiT32Bit) & 0xf;
        }

        public static int ExtractRdLoT32(uint encoding)
        {
            return (int)(encoding >> RtRdLoT32Bit) & 0xf;
        }

        public static int ExtractRdHiT32(uint encoding)
        {
            return (int)(encoding >> RdRt2RdHiT32Bit) & 0xf;
        }

        public static int ExtractRtT32(uint encoding)
        {
            return (int)(encoding >> RtRdLoT32Bit) & 0xf;
        }

        public static int ExtractRt2T32(uint encoding)
        {
            return (int)(encoding >> RdRt2RdHiT32Bit) & 0xf;
        }
    }
}
