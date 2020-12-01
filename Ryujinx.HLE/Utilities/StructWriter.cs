using Ryujinx.Cpu;
using Ryujinx.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    class StructWriter
    {
        private IVirtualMemoryManager _memory;

        public long Position { get; private set; }

        public StructWriter(IVirtualMemoryManager memory, long position)
        {
            _memory  = memory;
            Position = position;
        }

        public void Write<T>(T value) where T : struct
        {
            MemoryHelper.Write(_memory, Position, value);

            Position += Marshal.SizeOf<T>();
        }

        public void SkipBytes(long count)
        {
            Position += count;
        }
    }
}
