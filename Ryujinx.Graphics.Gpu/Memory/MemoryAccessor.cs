using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    class MemoryAccessor
    {
        private GpuContext _context;

        public MemoryAccessor(GpuContext context)
        {
            _context = context;
        }

        public Span<byte> Read(ulong gpuVa, ulong maxSize)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            ulong size = Math.Min(_context.MemoryManager.GetSubSize(gpuVa), maxSize);

            return _context.PhysicalMemory.Read(processVa, size);
        }

        public T Read<T>(ulong gpuVa) where T : struct
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            ulong size = (uint)Marshal.SizeOf<T>();

            return MemoryMarshal.Cast<byte, T>(_context.PhysicalMemory.Read(processVa, size))[0];
        }

        public int ReadInt32(ulong gpuVa)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            return BitConverter.ToInt32(_context.PhysicalMemory.Read(processVa, 4));
        }

        public void Write(ulong gpuVa, int value)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, BitConverter.GetBytes(value));
        }

        public void Write(ulong gpuVa, Span<byte> data)
        {
            ulong processVa = _context.MemoryManager.Translate(gpuVa);

            _context.PhysicalMemory.Write(processVa, data);
        }
    }
}