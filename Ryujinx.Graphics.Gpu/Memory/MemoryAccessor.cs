using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// GPU mapped memory accessor.
    /// </summary>
    public class MemoryAccessor
    {
        private GpuContext _context;

        /// <summary>
        /// Creates a new instance of the GPU memory accessor.
        /// </summary>
        /// <param name="context">GPU context that the memory accessor belongs to</param>
        public MemoryAccessor(GpuContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Reads a byte array from GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the data is located</param>
        /// <param name="size">Size of the data in bytes</param>
        /// <returns>Byte array with the data</returns>
        public byte[] ReadBytes(ulong gpuVa, int size)
        {
            return GetSpan(gpuVa, size).ToArray();
        }

        /// <summary>
        /// Gets a read-only span of data from GPU mapped memory.
        /// This reads as much data as possible, up to the specified maximum size.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the data is located</param>
        /// <param name="size">Size of the data</param>
        /// <returns>The span of the data at the specified memory location</returns>
        public ReadOnlySpan<byte> GetSpan(ulong gpuVa, int size)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            return _context.PhysicalMemory.GetSpan(processVa, size);
        }

        /// <summary>
        /// Reads data from GPU mapped memory.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="gpuVa">GPU virtual address where the data is located</param>
        /// <returns>The data at the specified memory location</returns>
        public T Read<T>(ulong gpuVa) where T : unmanaged
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            return MemoryMarshal.Cast<byte, T>(_context.PhysicalMemory.GetSpan(processVa, Unsafe.SizeOf<T>()))[0];
        }

        /// <summary>
        /// Writes a 32-bits signed integer to GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address to write the value into</param>
        /// <param name="value">The value to be written</param>
        public void Write<T>(ulong gpuVa, T value) where T : unmanaged
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref value, 1)));
        }

        /// <summary>
        /// Writes data to GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address to write the data into</param>
        /// <param name="data">The data to be written</param>
        public void Write(ulong gpuVa, ReadOnlySpan<byte> data)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, data);
        }
    }
}