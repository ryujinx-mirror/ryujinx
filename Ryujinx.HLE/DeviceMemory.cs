using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE
{
    class DeviceMemory : IDisposable
    {
        public const long RamSize = 4L * 1024 * 1024 * 1024;

        public IntPtr RamPointer { get; private set; }

        private unsafe byte* _ramPtr;

        public unsafe DeviceMemory()
        {
            RamPointer = Marshal.AllocHGlobal(new IntPtr(RamSize));

            _ramPtr = (byte*)RamPointer;
        }

        public sbyte ReadSByte(long position)
        {
            return (sbyte)ReadByte(position);
        }

        public short ReadInt16(long position)
        {
            return (short)ReadUInt16(position);
        }

        public int ReadInt32(long position)
        {
            return (int)ReadUInt32(position);
        }

        public long ReadInt64(long position)
        {
            return (long)ReadUInt64(position);
        }

        public unsafe byte ReadByte(long position)
        {
            return *(_ramPtr + position);
        }

        public unsafe ushort ReadUInt16(long position)
        {
            return *((ushort*)(_ramPtr + position));
        }

        public unsafe uint ReadUInt32(long position)
        {
            return *((uint*)(_ramPtr + position));
        }

        public unsafe ulong ReadUInt64(long position)
        {
            return *((ulong*)(_ramPtr + position));
        }

        public void WriteSByte(long position, sbyte value)
        {
            WriteByte(position, (byte)value);
        }

        public void WriteInt16(long position, short value)
        {
            WriteUInt16(position, (ushort)value);
        }

        public void WriteInt32(long position, int value)
        {
            WriteUInt32(position, (uint)value);
        }

        public void WriteInt64(long position, long value)
        {
            WriteUInt64(position, (ulong)value);
        }

        public unsafe void WriteByte(long position, byte value)
        {
            *(_ramPtr + position) = value;
        }

        public unsafe void WriteUInt16(long position, ushort value)
        {
            *((ushort*)(_ramPtr + position)) = value;
        }

        public unsafe void WriteUInt32(long position, uint value)
        {
            *((uint*)(_ramPtr + position)) = value;
        }

        public unsafe void WriteUInt64(long position, ulong value)
        {
            *((ulong*)(_ramPtr + position)) = value;
        }

        public void FillWithZeros(long position, int size)
        {
            int size8 = size & ~(8 - 1);

            for (int offs = 0; offs < size8; offs += 8)
            {
                WriteInt64(position + offs, 0);
            }

            for (int offs = size8; offs < (size - size8); offs++)
            {
                WriteByte(position + offs, 0);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            Marshal.FreeHGlobal(RamPointer);
        }
    }
}