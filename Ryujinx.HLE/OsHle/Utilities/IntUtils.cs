namespace Ryujinx.HLE.OsHle.Utilities
{
    static class IntUtils
    {
        public static int AlignUp(int Value, int Size)
        {
            return (Value + (Size - 1)) & ~(Size - 1);
        }

        public static long AlignUp(long Value, int Size)
        {
            return (Value + (Size - 1)) & ~((long)Size - 1);
        }
    }
}
