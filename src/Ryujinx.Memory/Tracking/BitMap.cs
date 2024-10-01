using System.Runtime.CompilerServices;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A bitmap that can check or set large ranges of true/false values at once.
    /// </summary>
    readonly struct BitMap
    {
        public const int IntSize = 64;

        private const int IntShift = 6;
        private const int IntMask = IntSize - 1;

        /// <summary>
        /// Masks representing the bitmap. Least significant bit first, 64-bits per mask.
        /// </summary>
        public readonly long[] Masks;

        /// <summary>
        /// Create a new bitmap.
        /// </summary>
        /// <param name="count">The number of bits to reserve</param>
        public BitMap(int count)
        {
            Masks = new long[(count + IntMask) / IntSize];
        }

        /// <summary>
        /// Check if any bit in the bitmap is set.
        /// </summary>
        /// <returns>True if any bits are set, false otherwise</returns>
        public bool AnySet()
        {
            for (int i = 0; i < Masks.Length; i++)
            {
                if (Masks[i] != 0)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int bit)
        {
            int wordIndex = bit >> IntShift;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            return (Masks[wordIndex] & wordMask) != 0;
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

            if (startIndex == endIndex)
            {
                return (Masks[startIndex] & startMask & endMask) != 0;
            }

            if ((Masks[startIndex] & startMask) != 0)
            {
                return true;
            }

            for (int i = startIndex + 1; i < endIndex; i++)
            {
                if (Masks[i] != 0)
                {
                    return true;
                }
            }

            if ((Masks[endIndex] & endMask) != 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set a bit at a specific index to 1.
        /// </summary>
        /// <param name="bit">The bit index to set</param>
        /// <returns>True if the bit is set, false if it was already set</returns>
        public bool Set(int bit)
        {
            int wordIndex = bit >> IntShift;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            if ((Masks[wordIndex] & wordMask) != 0)
            {
                return false;
            }

            Masks[wordIndex] |= wordMask;

            return true;
        }

        /// <summary>
        /// Set a range of bits in the bitmap to 1.
        /// </summary>
        /// <param name="start">The first bit index to set</param>
        /// <param name="end">The last bit index to set</param>
        public void SetRange(int start, int end)
        {
            if (start == end)
            {
                Set(start);
                return;
            }

            int startIndex = start >> IntShift;
            int startBit = start & IntMask;
            long startMask = -1L << startBit;

            int endIndex = end >> IntShift;
            int endBit = end & IntMask;
            long endMask = (long)(ulong.MaxValue >> (IntMask - endBit));

            if (startIndex == endIndex)
            {
                Masks[startIndex] |= startMask & endMask;
            }
            else
            {
                Masks[startIndex] |= startMask;

                for (int i = startIndex + 1; i < endIndex; i++)
                {
                    Masks[i] |= -1;
                }

                Masks[endIndex] |= endMask;
            }
        }

        /// <summary>
        /// Clear a bit at a specific index to 0.
        /// </summary>
        /// <param name="bit">The bit index to clear</param>
        /// <returns>True if the bit was set, false if it was not</returns>
        public bool Clear(int bit)
        {
            int wordIndex = bit >> IntShift;
            int wordBit = bit & IntMask;

            long wordMask = 1L << wordBit;

            bool wasSet = (Masks[wordIndex] & wordMask) != 0;

            Masks[wordIndex] &= ~wordMask;

            return wasSet;
        }

        /// <summary>
        /// Clear the bitmap entirely, setting all bits to 0.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Masks.Length; i++)
            {
                Masks[i] = 0;
            }
        }
    }
}
