using System;

namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Range of memory composed of an address and size.
    /// </summary>
    public readonly struct HostMemoryRange : IEquatable<HostMemoryRange>
    {
        /// <summary>
        /// An empty memory range, with a null address and zero size.
        /// </summary>
        public static HostMemoryRange Empty => new(0, 0);

        /// <summary>
        /// Start address of the range.
        /// </summary>
        public nuint Address { get; }

        /// <summary>
        /// Size of the range in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// Address where the range ends (exclusive).
        /// </summary>
        public nuint EndAddress => Address + (nuint)Size;

        /// <summary>
        /// Creates a new memory range with the specified address and size.
        /// </summary>
        /// <param name="address">Start address</param>
        /// <param name="size">Size in bytes</param>
        public HostMemoryRange(nuint address, ulong size)
        {
            Address = address;
            Size = size;
        }

        /// <summary>
        /// Checks if the range overlaps with another.
        /// </summary>
        /// <param name="other">The other range to check for overlap</param>
        /// <returns>True if the ranges overlap, false otherwise</returns>
        public bool OverlapsWith(HostMemoryRange other)
        {
            nuint thisAddress = Address;
            nuint thisEndAddress = EndAddress;
            nuint otherAddress = other.Address;
            nuint otherEndAddress = other.EndAddress;

            return thisAddress < otherEndAddress && otherAddress < thisEndAddress;
        }

        public override bool Equals(object obj)
        {
            return obj is HostMemoryRange other && Equals(other);
        }

        public bool Equals(HostMemoryRange other)
        {
            return Address == other.Address && Size == other.Size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Address, Size);
        }

        public static bool operator ==(HostMemoryRange left, HostMemoryRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HostMemoryRange left, HostMemoryRange right)
        {
            return !(left == right);
        }
    }
}
