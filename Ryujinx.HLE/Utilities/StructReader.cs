using Ryujinx.Cpu;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    class StructReader
    {
        private IVirtualMemoryManager _memory;

        public ulong Position { get; private set; }

        public StructReader(IVirtualMemoryManager memory, ulong position)
        {
            _memory  = memory;
            Position = position;
        }

        public T Read<T>() where T : unmanaged
        {
            T value = MemoryHelper.Read<T>(_memory, Position);

            Position += (uint)Marshal.SizeOf<T>();

            return value;
        }

        public ReadOnlySpan<T> Read<T>(int size) where T : unmanaged
        {
            ReadOnlySpan<byte> data = _memory.GetSpan(Position, size);

            Position += (uint)size;

            return MemoryMarshal.Cast<byte, T>(data);
        }
    }
}
