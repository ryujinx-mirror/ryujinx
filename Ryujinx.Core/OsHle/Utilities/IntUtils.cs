namespace Ryujinx.Core.OsHle.Utilities
{
    static class IntUtils
    {
        public static int RoundUp(int Value, int Size)
        {
            return (Value + (Size - 1)) & ~(Size - 1);
        }

        public static long RoundUp(long Value, int Size)
        {
            return (Value + (Size - 1)) & ~((long)Size - 1);
        }
    }
}
