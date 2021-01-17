using System;

namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Range of memory composed of an address and size.
    /// </summary>
    public struct MemoryRange : IEquatable<MemoryRange>
    {
        /// <summary>
        /// An empty memory range, with a null address and zero size.
        /// </summary>
        public static MemoryRange Empty => new MemoryRange(0UL, 0);

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

            return thisAddress < otherEndAddress && otherAddress < thisEndAddress;
        }

        public override bool Equals(object obj)
        {
            return obj is MemoryRange other && Equals(other);
        }

        public bool Equals(MemoryRange other)
        {
            return Address == other.Address && Size == other.Size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Address, Size);
        }
    }
}
