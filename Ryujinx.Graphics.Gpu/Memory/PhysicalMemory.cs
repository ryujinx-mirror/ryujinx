using Ryujinx.Cpu;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Represents physical memory, accessible from the GPU.
    /// This is actually working CPU virtual addresses, of memory mapped on the application process.
    /// </summary>
    class PhysicalMemory
    {
        public const int PageSize = Cpu.MemoryManager.PageSize;

        private readonly Cpu.MemoryManager _cpuMemory;

        /// <summary>
        /// Creates a new instance of the physical memory.
        /// </summary>
        /// <param name="cpuMemory">CPU memory manager of the application process</param>
        public PhysicalMemory(Cpu.MemoryManager cpuMemory)
        {
            _cpuMemory = cpuMemory;
        }

        /// <summary>
        /// Gets a span of data from the application process.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes to be range</param>
        /// <returns>A read only span of the data at the specified memory location</returns>
        public ReadOnlySpan<byte> GetSpan(ulong address, int size)
        {
            return _cpuMemory.GetSpan(address, size);
        }

        /// <summary>
        /// Gets a writable region from the application process.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes to be range</param>
        /// <returns>A writable region with the data at the specified memory location</returns>
        public WritableRegion GetWritableRegion(ulong address, int size)
        {
            return _cpuMemory.GetWritableRegion(address, size);
        }

        /// <summary>
        /// Reads data from the application process.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="gpuVa">Address to read from</param>
        /// <returns>The data at the specified memory location</returns>
        public T Read<T>(ulong address) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(GetSpan(address, Unsafe.SizeOf<T>()))[0];
        }

        /// <summary>
        /// Writes data to the application process.
        /// </summary>
        /// <param name="address">Address to write into</param>
        /// <param name="data">Data to be written</param>
        public void Write(ulong address, ReadOnlySpan<byte> data)
        {
            _cpuMemory.Write(address, data);
        }

        /// <summary>
        /// Writes data to the application process, without any tracking.
        /// </summary>
        /// <param name="address">Address to write into</param>
        /// <param name="data">Data to be written</param>
        public void WriteUntracked(ulong address, ReadOnlySpan<byte> data)
        {
            _cpuMemory.WriteUntracked(address, data);
        }

        /// <summary>
        /// Checks if a specified virtual memory region has been modified by the CPU since the last call.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <param name="name">Resource name</param>
        /// <param name="modifiedRanges">Optional array where the modified ranges should be written</param>
        /// <returns>The number of modified ranges</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int QueryModified(ulong address, ulong size, ResourceName name, (ulong, ulong)[] modifiedRanges = null)
        {
            return _cpuMemory.QueryModified(address, size, (int)name, modifiedRanges);
        }
    }
}