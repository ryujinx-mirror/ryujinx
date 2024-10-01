namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Range of memory composed of an address and size.
    /// </summary>
    public readonly record struct MemoryRange
    {
        /// <summary>
        /// Special address value used to indicate than an address is invalid.
        /// </summary>
        internal const ulong InvalidAddress = ulong.MaxValue;

        /// <summary>
        /// An empty memory range, with a null address and zero size.
        /// </summary>
        public static MemoryRange Empty => new(0UL, 0);

        /// <summary>
        /// Start address of the range.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Size of the range in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// Address where the range ends (exclusive).
        /// </summary>
        public ulong EndAddress => Address + Size;

        /// <summary>
        /// Creates a new memory range with the specified address and size.
        /// </summary>
        /// <param name="address">Start address</param>
        /// <param name="size">Size in bytes</param>
        public MemoryRange(ulong address, ulong size)
        {
            Address = address;
            Size = size;
        }

        /// <summary>
        /// Checks if the range overlaps with another.
        /// </summary>
        /// <param name="other">The other range to check for overlap</param>
        /// <returns>True if the ranges overlap, false otherwise</returns>
        public bool OverlapsWith(MemoryRange other)
        {
            ulong thisAddress = Address;
            ulong thisEndAddress = EndAddress;
            ulong otherAddress = other.Address;
            ulong otherEndAddress = other.EndAddress;

            // If any of the ranges if invalid (address + size overflows),
            // then they are never considered to overlap.
            if (thisEndAddress < thisAddress || otherEndAddress < otherAddress)
            {
                return false;
            }

            return thisAddress < otherEndAddress && otherAddress < thisEndAddress;
        }

        /// <summary>
        /// Checks if a given sub-range of memory is invalid.
        /// Those are used to represent unmapped memory regions (holes in the region mapping).
        /// </summary>
        /// <param name="subRange">Memory range to check</param>
        /// <returns>True if the memory range is considered invalid, false otherwise</returns>
        internal static bool IsInvalid(ref MemoryRange subRange)
        {
            return subRange.Address == InvalidAddress;
        }

        /// <summary>
        /// Returns a string summary of the memory range.
        /// </summary>
        /// <returns>A string summary of the memory range</returns>
        public override string ToString()
        {
            if (Address == InvalidAddress)
            {
                return $"[Unmapped 0x{Size:X}]";
            }

            return $"[0x{Address:X}, 0x{EndAddress:X})";
        }
    }
}
