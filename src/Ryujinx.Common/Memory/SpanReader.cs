using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    public ref struct SpanReader
    {
        private ReadOnlySpan<byte> _input;

        public readonly int Length => _input.Length;

        public SpanReader(ReadOnlySpan<byte> input)
        {
            _input = input;
        }

        public T Read<T>() where T : unmanaged
        {
            T value = MemoryMarshal.Cast<byte, T>(_input)[0];

            _input = _input[Unsafe.SizeOf<T>()..];

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

            _input = _input[valueSize..];

            return true;
        }

        public ReadOnlySpan<byte> GetSpan(int size)
        {
            ReadOnlySpan<byte> data = _input[..size];

            _input = _input[size..];

            return data;
        }

        public ReadOnlySpan<byte> GetSpanSafe(int size)
        {
            return GetSpan((int)Math.Min((uint)_input.Length, (uint)size));
        }

        public readonly T ReadAt<T>(int offset) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(_input[offset..])[0];
        }

        public readonly ReadOnlySpan<byte> GetSpanAt(int offset, int size)
        {
            return _input.Slice(offset, size);
        }

        public void Skip(int size)
        {
            _input = _input[size..];
        }
    }
}
