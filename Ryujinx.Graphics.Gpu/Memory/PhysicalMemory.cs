using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    using CpuMemoryManager = ARMeilleure.Memory.MemoryManager;

    /// <summary>
    /// Represents physical memory, accessible from the GPU.
    /// This is actually working CPU virtual addresses, of memory mapped on the game process.
    /// </summary>
    class PhysicalMemory
    {
        private readonly CpuMemoryManager _cpuMemory;

        /// <summary>
        /// Creates a new instance of the physical memory.
        /// </summary>
        /// <param name="cpuMemory">CPU memory manager of the application process</param>
        public PhysicalMemory(CpuMemoryManager cpuMemory)
        {
            _cpuMemory = cpuMemory;
        }

        /// <summary>
        /// Reads data from the application process.
        /// </summary>
        /// <param name="address">Address to be read</param>
        /// <param name="size">Size in bytes to be read</param>
        /// <returns>The data at the specified memory location</returns>
        public Span<byte> Read(ulong address, ulong size)
        {
            return _cpuMemory.ReadBytes((long)address, (long)size);
        }

        /// <summary>
        /// Writes data to the application process.
        /// </summary>
        /// <param name="address">Address to write into</param>
        /// <param name="data">Data to be written</param>
        public void Write(ulong address, Span<byte> data)
        {
            _cpuMemory.WriteBytes((long)address, data.ToArray());
        }

        /// <summary>
        /// Gets the modified ranges for a given range of the application process mapped memory.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size, in bytes, of the range</param>
        /// <param name="name">Name of the GPU resource being checked</param>
        /// <returns>Ranges, composed of address and size, modified by the application process, form the CPU</returns>
        public (ulong, ulong)[] GetModifiedRanges(ulong address, ulong size, ResourceName name)
        {
            return _cpuMemory.GetModifiedRanges(address, size, (int)name);
        }
    }
}