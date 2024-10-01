using System;
using System.Threading;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A bitmap that can be safely modified from multiple threads.
    /// </summary>
    internal class ConcurrentBitmap
    {
        public const int IntSize = 64;

        public const int IntShift = 6;
        public const int IntMask = IntSize - 1;

        /// <summary>
        /// Masks representing the bitmap. Least significant bit first, 64-bits per mask.
        /// </summary>
        public readonly long[] Masks;

        /// <summary>
        /// Create a new multithreaded bitmap.
        /// </summary>
        /// <param name="count">The number of bits to reserve</param>
        /// <param name="set">Whether the bits should be initially set or not</param>
        public ConcurrentBitmap(int count, bool set)
        {
            Masks = new long[(count + IntMask) / IntSize];

            if (set)
            {
                Array.Fill(Masks, -1L);
            }
        }

        /// <summary>
        /// Check if any bit in the bitmap is set.
        /// </summary>
        /// <returns>True if any bits are set, false otherwise</returns>
        public bool AnySet()
        {
            for (int i = 0; i < Masks.Length; i++)
            {
                if (Interlocked.Read(ref Masks[i]) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a bit in the bitmap is set.
        /// </summary>
        /// <param name="bit">The bit index to check</param>
        /// <returns>True if the bit is set, false otherwise</returns>
        public bool IsSet(int bit)
        {
            int wordIndex = bit >> IntShift;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            return (Interlocked.Read(ref Masks[wordIndex]) & wordMask) != 0;
        }

        /// <summary>
        /// Check if any bit in a range of bits in the bitmap are set. (inclusive)
        /// </summary>
        /// <param name="start">The first bit index to check</param>
        /// <param name="end">The last bit index to check</param>
        /// <returns>True if a bit is set, false otherwise</returns>
        public bool IsSet(int start, int end)
        {
            if (start == end)
            {
                return IsSet(start);
            }

            int startIndex = start >> IntShift;
            int startBit = start & IntMask;
            long startMask = -1L << startBit;

            int endIndex = end >> IntShift;
            int endBit = end & IntMask;
            long endMask = (long)(ulong.MaxValue >> (IntMask - endBit));

            long startValue = Interlocked.Read(ref Masks[startIndex]);

            if (startIndex == endIndex)
            {
                return (startValue & startMask & endMask) != 0;
            }

            if ((startValue & startMask) != 0)
            {
                return true;
            }

            for (int i = startIndex + 1; i < endIndex; i++)
            {
                if (Interlocked.Read(ref Masks[i]) != 0)
                {
                    return true;
                }
            }

            long endValue = Interlocked.Read(ref Masks[endIndex]);

            if ((endValue & endMask) != 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set a bit at a specific index to either true or false.
        /// </summary>
        /// <param name="bit">The bit index to set</param>
        /// <param name="value">Whether the bit should be set or not</param>
        public void Set(int bit, bool value)
        {
            int wordIndex = bit >> IntShift;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            if (value)
            {
                Interlocked.Or(ref Masks[wordIndex], wordMask);
            }
            else
            {
                Interlocked.And(ref Masks[wordIndex], ~wordMask);
            }
        }

        /// <summary>
        /// Clear the bitmap entirely, setting all bits to 0.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Masks.Length; i++)
            {
                Interlocked.Exchange(ref Masks[i], 0);
            }
        }
    }
}
