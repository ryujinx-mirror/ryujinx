using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    public ref struct SpanWriter
    {
        private Span<byte> _output;

        public readonly int Length => _output.Length;

        public SpanWriter(Span<byte> output)
        {
            _output = output;
        }

        public void Write<T>(T value) where T : unmanaged
        {
            MemoryMarshal.Cast<byte, T>(_output)[0] = value;
            _output = _output[Unsafe.SizeOf<T>()..];
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            data.CopyTo(_output[..data.Length]);
            _output = _output[data.Length..];
        }

        public readonly void WriteAt<T>(int offset, T value) where T : unmanaged
        {
            MemoryMarshal.Cast<byte, T>(_output[offset..])[0] = value;
        }

        public readonly void WriteAt(int offset, ReadOnlySpan<byte> data)
        {
            data.CopyTo(_output.Slice(offset, data.Length));
        }

        public void Skip(int size)
        {
            _output = _output[size..];
        }
    }
}
