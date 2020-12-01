using Ryujinx.Cpu;
using Ryujinx.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Utilities
{
    class StructReader
    {
        private IVirtualMemoryManager _memory;

        public long Position { get; private set; }

        public StructReader(IVirtualMemoryManager memory, long position)
        {
            _memory  = memory;
            Position = position;
        }

        public T Read<T>() where T : struct
        {
            T value = MemoryHelper.Read<T>(_memory, Position);

            Position += Marshal.SizeOf<T>();

            return value;
        }

        public T[] Read<T>(int size) where T : struct
        {
            int structSize = Marshal.SizeOf<T>();

            int count = size / structSize;

            T[] output = new T[count];

            for (int index = 0; index < count; index++)
            {
                output[index] = MemoryHelper.Read<T>(_memory, Position);

                Position += structSize;
            }

            return output;
        }
    }
}
