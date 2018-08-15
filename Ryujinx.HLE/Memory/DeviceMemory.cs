using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.Memory
{
    class DeviceMemory : IDisposable
    {
        public const long RamSize = 4L * 1024 * 1024 * 1024;

        public ArenaAllocator Allocator { get; private set; }

        public IntPtr RamPointer { get; private set; }

        private unsafe byte* RamPtr;

        public unsafe DeviceMemory()
        {
            Allocator = new ArenaAllocator(RamSize);

            RamPointer = Marshal.AllocHGlobal(new IntPtr(RamSize));

            RamPtr = (byte*)RamPointer;
        }

        public sbyte ReadSByte(long Position)
        {
            return (sbyte)ReadByte(Position);
        }

        public short ReadInt16(long Position)
        {
            return (short)ReadUInt16(Position);
        }

        public int ReadInt32(long Position)
        {
            return (int)ReadUInt32(Position);
        }

        public long ReadInt64(long Position)
        {
            return (long)ReadUInt64(Position);
        }

        public unsafe byte ReadByte(long Position)
        {
            return *((byte*)(RamPtr + Position));
        }

        public unsafe ushort ReadUInt16(long Position)
        {
            return *((ushort*)(RamPtr + Position));
        }

        public unsafe uint ReadUInt32(long Position)
        {
            return *((uint*)(RamPtr + Position));
        }

        public unsafe ulong ReadUInt64(long Position)
        {
            return *((ulong*)(RamPtr + Position));
        }

        public void WriteSByte(long Position, sbyte Value)
        {
            WriteByte(Position, (byte)Value);
        }

        public void WriteInt16(long Position, short Value)
        {
            WriteUInt16(Position, (ushort)Value);
        }

        public void WriteInt32(long Position, int Value)
        {
            WriteUInt32(Position, (uint)Value);
        }

        public void WriteInt64(long Position, long Value)
        {
            WriteUInt64(Position, (ulong)Value);
        }

        public unsafe void WriteByte(long Position, byte Value)
        {
            *((byte*)(RamPtr + Position)) = Value;
        }

        public unsafe void WriteUInt16(long Position, ushort Value)
        {
            *((ushort*)(RamPtr + Position)) = Value;
        }

        public unsafe void WriteUInt32(long Position, uint Value)
        {
            *((uint*)(RamPtr + Position)) = Value;
        }

        public unsafe void WriteUInt64(long Position, ulong Value)
        {
            *((ulong*)(RamPtr + Position)) = Value;
        }

        public void FillWithZeros(long Position, int Size)
        {
            int Size8 = Size & ~(8 - 1);

            for (int Offs = 0; Offs < Size8; Offs += 8)
            {
                WriteInt64(Position + Offs, 0);
            }

            for (int Offs = Size8; Offs < (Size - Size8); Offs++)
            {
                WriteByte(Position + Offs, 0);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            Marshal.FreeHGlobal(RamPointer);
        }
    }
}