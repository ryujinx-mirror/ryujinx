using Ryujinx.Memory.Range;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// A region of memory.
    /// </summary>
    abstract class AbstractRegion : INonOverlappingRange
    {
        /// <summary>
        /// Base address.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Size of the range in bytes.
        /// </summary>
        public ulong Size { get; protected set; }

        /// <summary>
        /// End address.
        /// </summary>
        public ulong EndAddress => Address + Size;

        /// <summary>
        /// Create a new region.
        /// </summary>
        /// <param name="address">Base address</param>
        /// <param name="size">Size of the range</param>
        protected AbstractRegion(ulong address, ulong size)
        {
            Address = address;
            Size = size;
        }

        /// <summary>
        /// Check if this range overlaps with another.
        /// </summary>
        /// <param name="address">Base address</param>
        /// <param name="size">Size of the range</param>
        /// <returns>True if overlapping, false otherwise</returns>
        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }

        /// <summary>
        /// Signals to the handles that a memory event has occurred, and unprotects the region. Assumes that the tracking lock has been obtained.
        /// </summary>
        /// <param name="address">Address accessed</param>
        /// <param name="size">Size of the region affected in bytes</param>
        /// <param name="write">Whether the region was written to or read</param>
        /// <param name="exemptId">Optional ID of the handles that should not be signalled</param>
        public abstract void Signal(ulong address, ulong size, bool write, int? exemptId);

        /// <summary>
        /// Signals to the handles that a precise memory event has occurred. Assumes that the tracking lock has been obtained.
        /// </summary>
        /// <param name="address">Address accessed</param>
        /// <param name="size">Size of the region affected in bytes</param>
        /// <param name="write">Whether the region was written to or read</param>
        /// <param name="exemptId">Optional ID of the handles that should not be signalled</param>
        public abstract void SignalPrecise(ulong address, ulong size, bool write, int? exemptId);

        /// <summary>
        /// Split this region into two, around the specified address.
        /// This region is updated to end at the split address, and a new region is created to represent past that point.
        /// </summary>
        /// <param name="splitAddress">Address to split the region around</param>
        /// <returns>The second part of the split region, with start address at the given split.</returns>
        public abstract INonOverlappingRange Split(ulong splitAddress);
    }
}
