namespace Ryujinx.Graphics.Nvdec.Vp9.Dsp
{
    internal static class TxfmCommon
    {
        // Constants used by all idct/dct functions
        public const int DctConstBits = 14;
        public const int DctConstRounding = 1 << (DctConstBits - 1);

        public const int UnitQuantShift = 2;
        public const int UnitQuantFactor = 1 << UnitQuantShift;

        // Constants:
        //  for (int i = 1; i < 32; ++i)
        //    Console.WriteLine("public const short CosPi{0}_64 = {1};", i, MathF.Round(16384 * MathF.Cos(i * MathF.PI / 64)));
        // Note: sin(k * Pi / 64) = cos((32 - k) * Pi / 64)
        public const short CosPi1_64 = 16364;
        public const short CosPi2_64 = 16305;
        public const short CosPi3_64 = 16207;
        public const short CosPi4_64 = 16069;
        public const short CosPi5_64 = 15893;
        public const short CosPi6_64 = 15679;
        public const short CosPi7_64 = 15426;
        public const short CosPi8_64 = 15137;
        public const short CosPi9_64 = 14811;
        public const short CosPi10_64 = 14449;
        public const short CosPi11_64 = 14053;
        public const short CosPi12_64 = 13623;
        public const short CosPi13_64 = 13160;
        public const short CosPi14_64 = 12665;
        public const short CosPi15_64 = 12140;
        public const short CosPi16_64 = 11585;
        public const short CosPi17_64 = 11003;
        public const short CosPi18_64 = 10394;
        public const short CosPi19_64 = 9760;
        public const short CosPi20_64 = 9102;
        public const short CosPi21_64 = 8423;
        public const short CosPi22_64 = 7723;
        public const short CosPi23_64 = 7005;
        public const short CosPi24_64 = 6270;
        public const short CosPi25_64 = 5520;
        public const short CosPi26_64 = 4756;
        public const short CosPi27_64 = 3981;
        public const short CosPi28_64 = 3196;
        public const short CosPi29_64 = 2404;
        public const short CosPi30_64 = 1606;
        public const short CosPi31_64 = 804;

        //  16384 * sqrt(2) * sin(kPi / 9) * 2 / 3
        public const short SinPi1_9 = 5283;
        public const short SinPi2_9 = 9929;
        public const short SinPi3_9 = 13377;
        public const short SinPi4_9 = 15212;
    }
}
