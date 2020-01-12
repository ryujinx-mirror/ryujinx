using System;
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
        public byte[] ReadBytes(ulong gpuVa, ulong size)
        {
            return GetSpan(gpuVa, size).ToArray();
        }

        /// <summary>
        /// Gets a read-only span of data from GPU mapped memory.
        /// This reads as much data as possible, up to the specified maximum size.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the data is located</param>
        /// <param name="maxSize">Maximum size of the data</param>
        /// <returns>The span of the data at the specified memory location</returns>
        public ReadOnlySpan<byte> GetSpan(ulong gpuVa, ulong maxSize)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            ulong size = Math.Min(_context.MemoryManager.GetSubSize(gpuVa), maxSize);

            return _context.PhysicalMemory.GetSpan(processVa, size);
        }

        /// <summary>
        /// Reads a structure from GPU mapped memory.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="gpuVa">GPU virtual address where the structure is located</param>
        /// <returns>The structure at the specified memory location</returns>
        public T Read<T>(ulong gpuVa) where T : struct
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            ulong size = (uint)Marshal.SizeOf<T>();

            return MemoryMarshal.Cast<byte, T>(_context.PhysicalMemory.GetSpan(processVa, size))[0];
        }

        /// <summary>
        /// Reads a 32-bits signed integer from GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the value is located</param>
        /// <returns>The value at the specified memory location</returns>
        public int ReadInt32(ulong gpuVa)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            return BitConverter.ToInt32(_context.PhysicalMemory.GetSpan(processVa, 4));
        }

        /// <summary>
        /// Reads a 64-bits unsigned integer from GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the value is located</param>
        /// <returns>The value at the specified memory location</returns>
        public ulong ReadUInt64(ulong gpuVa)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            return BitConverter.ToUInt64(_context.PhysicalMemory.GetSpan(processVa, 8));
        }

        /// <summary>
        /// Reads a 8-bits unsigned integer from GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the value is located</param>
        /// <param name="value">The value to be written</param>
        public void WriteByte(ulong gpuVa, byte value)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, MemoryMarshal.CreateSpan(ref value, 1));
        }

        /// <summary>
        /// Writes a 32-bits signed integer to GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address to write the value into</param>
        /// <param name="value">The value to be written</param>
        public void Write(ulong gpuVa, int value)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes data to GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address to write the data into</param>
        /// <param name="data">The data to be written</param>
        public void Write(ulong gpuVa, Span<byte> data)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, data);
        }
    }
}