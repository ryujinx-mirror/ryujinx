using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Vic.Types
{
    static class BitfieldExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Extract(this int value, int lsb)
        {
            return ((value >> (lsb & 0x1f)) & 1) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Extract(this int value, int lsb, int length)
        {
            return (value >> (lsb & 0x1f)) & (int)(uint.MaxValue >> (32 - length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Extract(this long value, int lsb)
        {
            return ((int)(value >> (lsb & 0x3f)) & 1) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Extract(this long value, int lsb, int length)
        {
            return (int)(value >> (lsb & 0x3f)) & (int)(uint.MaxValue >> (32 - length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExtractSx(this long value, int lsb, int length)
        {
            int shift = lsb & 0x3f;

            return (int)((value << (64 - (shift + length))) >> (64 - length));
        }
    }
}
