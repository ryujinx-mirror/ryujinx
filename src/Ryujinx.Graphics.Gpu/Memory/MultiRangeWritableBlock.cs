using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// A writable block that targets a given MultiRange within a PhysicalMemory instance.
    /// </summary>
    internal class MultiRangeWritableBlock : IWritableBlock
    {
        private readonly MultiRange _range;
        private readonly PhysicalMemory _physicalMemory;

        /// <summary>
        /// Creates a new MultiRangeWritableBlock.
        /// </summary>
        /// <param name="range">The MultiRange to write to</param>
        /// <param name="physicalMemory">The PhysicalMemory the given MultiRange addresses</param>
        public MultiRangeWritableBlock(MultiRange range, PhysicalMemory physicalMemory)
        {
            _range = range;
            _physicalMemory = physicalMemory;
        }

        /// <summary>
        /// Write data to the MultiRange.
        /// </summary>
        /// <param name="va">Offset address</param>
        /// <param name="data">Data to write</param>
        /// <exception cref="ArgumentException">Throw when a non-zero offset is given</exception>
        public void Write(ulong va, ReadOnlySpan<byte> data)
        {
            if (va != 0)
            {
                throw new ArgumentException($"{nameof(va)} cannot be non-zero for {nameof(MultiRangeWritableBlock)}.");
            }

            _physicalMemory.Write(_range, data);
        }

        /// <summary>
        /// Write data to the MultiRange, without tracking.
        /// </summary>
        /// <param name="va">Offset address</param>
        /// <param name="data">Data to write</param>
        /// <exception cref="ArgumentException">Throw when a non-zero offset is given</exception>
        public void WriteUntracked(ulong va, ReadOnlySpan<byte> data)
        {
            if (va != 0)
            {
                throw new ArgumentException($"{nameof(va)} cannot be non-zero for {nameof(MultiRangeWritableBlock)}.");
            }

            _physicalMemory.WriteUntracked(_range, data);
        }
    }
}
