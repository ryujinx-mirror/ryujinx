using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// GPU mapped memory accessor.
    /// </summary>
    class MemoryAccessor
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
        /// Reads data from GPU mapped memory.
        /// This reads as much data as possible, up to the specified maximum size.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the data is located</param>
        /// <param name="maxSize">Maximum size of the data</param>
        /// <returns>The data at the specified memory location</returns>
        public Span<byte> Read(ulong gpuVa, ulong maxSize)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            ulong size = Math.Min(_context.MemoryManager.GetSubSize(gpuVa), maxSize);

            return _context.PhysicalMemory.Read(processVa, size);
        }

        /// <summary>
        /// Reads a structure from GPU mapped memory.
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="gpuVa">GPU virtual address where the strcture is located</param>
        /// <returns>The structure at the specified memory location</returns>
        public T Read<T>(ulong gpuVa) where T : struct
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            ulong size = (uint)Marshal.SizeOf<T>();

            return MemoryMarshal.Cast<byte, T>(_context.PhysicalMemory.Read(processVa, size))[0];
        }

        /// <summary>
        /// Reads a 32-bits signed integer from GPU mapped memory.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the value is located</param>
        /// <returns>The value at the specified memory location</returns>
        public int ReadInt32(ulong gpuVa)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            return BitConverter.ToInt32(_context.PhysicalMemory.Read(processVa, 4));
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