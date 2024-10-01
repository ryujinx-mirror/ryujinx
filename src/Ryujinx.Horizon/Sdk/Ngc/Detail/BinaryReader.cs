using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Ngc.Detail
{
    ref struct BinaryReader
    {
        private readonly ReadOnlySpan<byte> _data;
        private int _offset;

        public BinaryReader(ReadOnlySpan<byte> data)
        {
            _data = data;
        }

        public bool Read<T>(out T value) where T : unmanaged
        {
            int byteLength = Unsafe.SizeOf<T>();

            if ((uint)(_offset + byteLength) <= (uint)_data.Length)
            {
                value = MemoryMarshal.Cast<byte, T>(_data[_offset..])[0];
                _offset += byteLength;

                return true;
            }

            value = default;

            return false;
        }

        public int AllocateAndReadArray<T>(ref T[] array, int length, int maxLengthExclusive) where T : unmanaged
        {
            return AllocateAndReadArray(ref array, Math.Min(length, maxLengthExclusive));
        }

        public int AllocateAndReadArray<T>(ref T[] array, int length) where T : unmanaged
        {
            array = new T[length];

            return ReadArray(array);
        }

        public int ReadArray<T>(T[] array) where T : unmanaged
        {
            if (array != null)
            {
                int byteLength = array.Length * Unsafe.SizeOf<T>();
                byteLength = Math.Min(byteLength, _data.Length - _offset);

                MemoryMarshal.Cast<byte, T>(_data.Slice(_offset, byteLength)).CopyTo(array);

                _offset += byteLength;

                return byteLength / Unsafe.SizeOf<T>();
            }

            return 0;
        }
    }
}
