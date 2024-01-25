using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray128 : IArray<byte>
    {
        private const int Size = 128;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }

    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray256 : IArray<byte>
    {
        private const int Size = 256;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }

    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray512 : IArray<byte>
    {
        private const int Size = 512;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }

    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray1024 : IArray<byte>
    {
        private const int Size = 1024;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }

    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray2048 : IArray<byte>
    {
        private const int Size = 2048;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }

    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray3000 : IArray<byte>
    {
        private const int Size = 3000;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }

    [StructLayout(LayoutKind.Sequential, Size = Size, Pack = 1)]
    public struct ByteArray4096 : IArray<byte>
    {
        private const int Size = 4096;

        byte _element;

        public readonly int Length => Size;
        public ref byte this[int index] => ref AsSpan()[index];
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref _element, Size);
    }
}
