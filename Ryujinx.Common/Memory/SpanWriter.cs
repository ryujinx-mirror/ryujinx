using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    public ref struct SpanWriter
    {
        private Span<byte> _output;

        public int Length => _output.Length;

        public SpanWriter(Span<byte> output)
        {
            _output = output;
        }

        public void Write<T>(T value) where T : unmanaged
        {
            MemoryMarshal.Cast<byte, T>(_output)[0] = value;
            _output = _output.Slice(Unsafe.SizeOf<T>());
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            data.CopyTo(_output.Slice(0, data.Length));
            _output = _output.Slice(data.Length);
        }

        public void WriteAt<T>(int offset, T value) where T : unmanaged
        {
            MemoryMarshal.Cast<byte, T>(_output.Slice(offset))[0] = value;
        }

        public void WriteAt(int offset, ReadOnlySpan<byte> data)
        {
            data.CopyTo(_output.Slice(offset, data.Length));
        }

        public void Skip(int size)
        {
            _output = _output.Slice(size);
        }
    }
}
