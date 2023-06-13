using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    public ref struct SpanReader
    {
        private ReadOnlySpan<byte> _input;

        public int Length => _input.Length;

        public SpanReader(ReadOnlySpan<byte> input)
        {
            _input = input;
        }

        public T Read<T>() where T : unmanaged
        {
            T value = MemoryMarshal.Cast<byte, T>(_input)[0];

            _input = _input.Slice(Unsafe.SizeOf<T>());

            return value;
        }

        public bool TryRead<T>(out T value) where T : unmanaged
        {
            int valueSize = Unsafe.SizeOf<T>();

            if (valueSize > _input.Length)
            {
                value = default;

                return false;
            }

            value = MemoryMarshal.Cast<byte, T>(_input)[0];

            _input = _input.Slice(valueSize);

            return true;
        }

        public ReadOnlySpan<byte> GetSpan(int size)
        {
            ReadOnlySpan<byte> data = _input.Slice(0, size);

            _input = _input.Slice(size);

            return data;
        }

        public ReadOnlySpan<byte> GetSpanSafe(int size)
        {
            return GetSpan((int)Math.Min((uint)_input.Length, (uint)size));
        }

        public T ReadAt<T>(int offset) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(_input.Slice(offset))[0];
        }

        public ReadOnlySpan<byte> GetSpanAt(int offset, int size)
        {
            return _input.Slice(offset, size);
        }

        public void Skip(int size)
        {
            _input = _input.Slice(size);
        }
    }
}