namespace Ryujinx.HLE.Utilities
{
    static class IntUtils
    {
        public static int AlignUp(int value, int size)
        {
            return (value + (size - 1)) & ~(size - 1);
        }

        public static long AlignUp(long value, int size)
        {
            return (value + (size - 1)) & ~((long)size - 1);
        }

        public static int AlignDown(int value, int size)
        {
            return value & ~(size - 1);
        }

        public static long AlignDown(long value, int size)
        {
            return value & ~((long)size - 1);
        }
    }
}
