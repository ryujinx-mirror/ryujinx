namespace Ryujinx.Cpu.LightningJit.Arm64
{
    static class ImmUtils
    {
        public static int ExtractSImm14Times4(uint encoding)
        {
            return ((int)(encoding >> 5) << 18) >> 16;
        }

        public static int ExtractSImm19Times4(uint encoding)
        {
            return ((int)(encoding >> 5) << 13) >> 11;
        }

        public static int ExtractSImm26Times4(uint encoding)
        {
            return (int)(encoding << 6) >> 4;
        }
    }
}
