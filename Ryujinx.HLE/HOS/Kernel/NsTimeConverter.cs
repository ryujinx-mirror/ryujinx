namespace Ryujinx.HLE.HOS.Kernel
{
    static class NsTimeConverter
    {
        public static int GetTimeMs(ulong Ns)
        {
            ulong Ms = Ns / 1_000_000;

            if (Ms < int.MaxValue)
            {
                return (int)Ms;
            }
            else
            {
                return int.MaxValue;
            }
        }
    }
}