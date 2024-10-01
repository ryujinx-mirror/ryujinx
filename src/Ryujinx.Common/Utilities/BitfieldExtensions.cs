using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Common.Utilities
{
    public static class BitfieldExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Extract<T>(this T value, int lsb) where T : IBinaryInteger<T>
        {
            int bitSize = Unsafe.SizeOf<T>() * 8;
            lsb &= bitSize - 1;

            return !T.IsZero((value >>> lsb) & T.One);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Extract<T>(this T value, int lsb, int length) where T : IBinaryInteger<T>
        {
            int bitSize = Unsafe.SizeOf<T>() * 8;
            lsb &= bitSize - 1;

            return (value >>> lsb) & (~T.Zero >>> (bitSize - length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ExtractSx<T>(this T value, int lsb, int length) where T : IBinaryInteger<T>
        {
            int bitSize = Unsafe.SizeOf<T>() * 8;
            int shift = lsb & (bitSize - 1);

            return (value << (bitSize - (shift + length))) >> (bitSize - length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Insert<T>(this T value, int lsb, bool toInsert) where T : IBinaryInteger<T>
        {
            int bitSize = Unsafe.SizeOf<T>() * 8;
            lsb &= bitSize - 1;

            T mask = T.One << lsb;

            return (value & ~mask) | (toInsert ? mask : T.Zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Insert<T>(this T value, int lsb, int length, T toInsert) where T : IBinaryInteger<T>
        {
            int bitSize = Unsafe.SizeOf<T>() * 8;
            lsb &= bitSize - 1;

            T mask = (~T.Zero >>> (bitSize - length)) << lsb;

            return (value & ~mask) | ((toInsert << lsb) & mask);
        }
    }
}
