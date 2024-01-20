using System;
using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Cpu.LightningJit.Cache
{
    readonly struct CacheEntry : IComparable<CacheEntry>
    {
        public int Offset { get; }
        public int Size { get; }

        public CacheEntry(int offset, int size)
        {
            Offset = offset;
            Size = size;
        }

        public int CompareTo([AllowNull] CacheEntry other)
        {
            return Offset.CompareTo(other.Offset);
        }
    }
}
