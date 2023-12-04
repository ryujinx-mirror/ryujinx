using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Spv.Generator
{
    /// <summary>
    /// Similar to System.HashCode, but without introducing random values.
    /// The same primes and shifts are used.
    /// </summary>
    internal static class DeterministicHashCode
    {
        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;

        public static int GetHashCode(string value)
        {
            uint hash = (uint)value.Length + Prime1;

            for (int i = 0; i < value.Length; i++)
            {
                hash += (hash << 7) ^ value[i];
            }

            return (int)MixFinal(hash);
        }

        public static int Combine<T>(ReadOnlySpan<T> values)
        {
            uint hashCode = Prime2;
            hashCode += 4 * (uint)values.Length;

            foreach (T value in values)
            {
                uint hc = (uint)(value?.GetHashCode() ?? 0);
                hashCode = MixStep(hashCode, hc);
            }

            return (int)MixFinal(hashCode);
        }

        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
            uint hc2 = (uint)(value2?.GetHashCode() ?? 0);

            uint hash = Prime2;
            hash += 8;

            hash = MixStep(hash, hc1);
            hash = MixStep(hash, hc2);

            return (int)MixFinal(hash);
        }

        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
            uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
            uint hc3 = (uint)(value3?.GetHashCode() ?? 0);

            uint hash = Prime2;
            hash += 12;

            hash = MixStep(hash, hc1);
            hash = MixStep(hash, hc2);
            hash = MixStep(hash, hc3);

            return (int)MixFinal(hash);
        }

        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
            uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
            uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
            uint hc4 = (uint)(value4?.GetHashCode() ?? 0);

            uint hash = Prime2;
            hash += 16;

            hash = MixStep(hash, hc1);
            hash = MixStep(hash, hc2);
            hash = MixStep(hash, hc3);
            hash = MixStep(hash, hc4);

            return (int)MixFinal(hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixStep(uint hashCode, uint mixValue)
        {
            return BitOperations.RotateLeft(hashCode + mixValue * Prime3, 17) * Prime4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixFinal(uint hash)
        {
            hash ^= hash >> 15;
            hash *= Prime2;
            hash ^= hash >> 13;
            hash *= Prime3;
            hash ^= hash >> 16;
            return hash;
        }
    }
}
