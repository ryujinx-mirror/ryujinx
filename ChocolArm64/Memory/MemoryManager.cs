using ChocolArm64.Events;
using ChocolArm64.Exceptions;
using ChocolArm64.Instructions;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

using static ChocolArm64.Memory.CompareExchange128;

namespace ChocolArm64.Memory
{
    public unsafe class MemoryManager : IMemory, IDisposable
    {
        private const int PtLvl0Bits = 13;
        private const int PtLvl1Bits = 14;
        public  const int PageBits = 12;

        private const int PtLvl0Size = 1 << PtLvl0Bits;
        private const int PtLvl1Size = 1 << PtLvl1Bits;
        public  const int PageSize   = 1 << PageBits;

        private const int PtLvl0Mask = PtLvl0Size - 1;
        private const int PtLvl1Mask = PtLvl1Size - 1;
        public  const int PageMask   = PageSize   - 1;

        private const int PtLvl0Bit = PageBits + PtLvl1Bits;
        private const int PtLvl1Bit = PageBits;

        private ConcurrentDictionary<long, IntPtr> _observedPages;

        public IntPtr Ram { get; private set; }

        private byte* _ramPtr;

        private byte*** _pageTable;

        public event EventHandler<MemoryAccessEventArgs> InvalidAccess;

        public event EventHandler<MemoryAccessEventArgs> ObservedAccess;

        public MemoryManager(IntPtr ram)
        {
            _observedPages = new ConcurrentDictionary<long, IntPtr>();

            Ram = ram;

            _ramPtr = (byte*)ram;

            _pageTable = (byte***)Marshal.AllocHGlobal(PtLvl0Size * IntPtr.Size);

            for (int l0 = 0; l0 < PtLvl0Size; l0++)
            {
                _pageTable[l0] = null;
            }
        }

        internal bool AtomicCompareExchange2xInt32(
            long position,
            int  expectedLow,
            int  expectedHigh,
            int  desiredLow,
            int  desiredHigh)
        {
            long expected = (uint)expectedLow;
            long desired  = (uint)desiredLow;

            expected |= (long)expectedHigh << 32;
            desired  |= (long)desiredHigh  << 32;

            return AtomicCompareExchangeInt64(position, expected, desired);
        }

        internal bool AtomicCompareExchangeInt128(
            long  position,
            ulong expectedLow,
            ulong expectedHigh,
            ulong desiredLow,
            ulong desiredHigh)
        {
            if ((position & 0xf) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            IntPtr ptr = new IntPtr(TranslateWrite(position));

            return InterlockedCompareExchange128(ptr, expectedLow, expectedHigh, desiredLow, desiredHigh);
        }

        internal Vector128<float> AtomicReadInt128(long position)
        {
            if ((position & 0xf) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            IntPtr ptr = new IntPtr(Translate(position));

            InterlockedRead128(ptr, out ulong low, out ulong high);

            Vector128<float> vector = default(Vector128<float>);

            vector = VectorHelper.VectorInsertInt(low,  vector, 0, 3);
            vector = VectorHelper.VectorInsertInt(high, vector, 1, 3);

            return vector;
        }

        public bool AtomicCompareExchangeByte(long position, byte expected, byte desired)
        {
            int* ptr = (int*)Translate(position);

            int currentValue = *ptr;

            int expected32 = (currentValue & ~byte.MaxValue) | expected;
            int desired32  = (currentValue & ~byte.MaxValue) | desired;

            return Interlocked.CompareExchange(ref *ptr, desired32, expected32) == expected32;
        }

        public bool AtomicCompareExchangeInt16(long position, short expected, short desired)
        {
            if ((position & 1) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            int* ptr = (int*)Translate(position);

            int currentValue = *ptr;

            int expected32 = (currentValue & ~ushort.MaxValue) | (ushort)expected;
            int desired32  = (currentValue & ~ushort.MaxValue) | (ushort)desired;

            return Interlocked.CompareExchange(ref *ptr, desired32, expected32) == expected32;
        }

        public bool AtomicCompareExchangeInt32(long position, int expected, int desired)
        {
            if ((position & 3) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            int* ptr = (int*)TranslateWrite(position);

            return Interlocked.CompareExchange(ref *ptr, desired, expected) == expected;
        }

        public bool AtomicCompareExchangeInt64(long position, long expected, long desired)
        {
            if ((position & 7) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            long* ptr = (long*)TranslateWrite(position);

            return Interlocked.CompareExchange(ref *ptr, desired, expected) == expected;
        }

        public int AtomicIncrementInt32(long position)
        {
            if ((position & 3) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            int* ptr = (int*)TranslateWrite(position);

            return Interlocked.Increment(ref *ptr);
        }

        public int AtomicDecrementInt32(long position)
        {
            if ((position & 3) != 0)
            {
                AbortWithAlignmentFault(position);
            }

            int* ptr = (int*)TranslateWrite(position);

            return Interlocked.Decrement(ref *ptr);
        }

        private void AbortWithAlignmentFault(long position)
        {
            //TODO: Abort mode and exception support on the CPU.
            throw new InvalidOperationException($"Tried to compare exchange a misaligned address 0x{position:X16}.");
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

        public byte ReadByte(long position)
        {
            return *((byte*)Translate(position));
        }

        public ushort ReadUInt16(long position)
        {
            if ((position & 1) == 0)
            {
                return *((ushort*)Translate(position));
            }
            else
            {
                return (ushort)(ReadByte(position + 0) << 0 |
                                ReadByte(position + 1) << 8);
            }
        }

        public uint ReadUInt32(long position)
        {
            if ((position & 3) == 0)
            {
                return *((uint*)Translate(position));
            }
            else
            {
                return (uint)(ReadUInt16(position + 0) << 0 |
                              ReadUInt16(position + 2) << 16);
            }
        }

        public ulong ReadUInt64(long position)
        {
            if ((position & 7) == 0)
            {
                return *((ulong*)Translate(position));
            }
            else
            {
                return (ulong)ReadUInt32(position + 0) << 0 |
                       (ulong)ReadUInt32(position + 4) << 32;
            }
        }

        public Vector128<float> ReadVector8(long position)
        {
            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<byte, float>(Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ReadByte(position)));
            }
            else
            {
                Vector128<float> value = VectorHelper.VectorSingleZero();

                value = VectorHelper.VectorInsertInt(ReadByte(position), value, 0, 0);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector16(long position)
        {
            if (Sse2.IsSupported && (position & 1) == 0)
            {
                return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse2.SetZeroVector128<ushort>(), ReadUInt16(position), 0));
            }
            else
            {
                Vector128<float> value = VectorHelper.VectorSingleZero();

                value = VectorHelper.VectorInsertInt(ReadUInt16(position), value, 0, 1);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector32(long position)
        {
            if (Sse.IsSupported && (position & 3) == 0)
            {
                return Sse.LoadScalarVector128((float*)Translate(position));
            }
            else
            {
                Vector128<float> value = VectorHelper.VectorSingleZero();

                value = VectorHelper.VectorInsertInt(ReadUInt32(position), value, 0, 2);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector64(long position)
        {
            if (Sse2.IsSupported && (position & 7) == 0)
            {
                return Sse.StaticCast<double, float>(Sse2.LoadScalarVector128((double*)Translate(position)));
            }
            else
            {
                Vector128<float> value = VectorHelper.VectorSingleZero();

                value = VectorHelper.VectorInsertInt(ReadUInt64(position), value, 0, 3);

                return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector128(long position)
        {
            if (Sse.IsSupported && (position & 15) == 0)
            {
                return Sse.LoadVector128((float*)Translate(position));
            }
            else
            {
                Vector128<float> value = VectorHelper.VectorSingleZero();

                value = VectorHelper.VectorInsertInt(ReadUInt64(position + 0), value, 0, 3);
                value = VectorHelper.VectorInsertInt(ReadUInt64(position + 8), value, 1, 3);

                return value;
            }
        }

        public byte[] ReadBytes(long position, long size)
        {
            long endAddr = position + size;

            if ((ulong)size > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if ((ulong)endAddr < (ulong)position)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            byte[] data = new byte[size];

            int offset = 0;

            while ((ulong)position < (ulong)endAddr)
            {
                long pageLimit = (position + PageSize) & ~(long)PageMask;

                if ((ulong)pageLimit > (ulong)endAddr)
                {
                    pageLimit = endAddr;
                }

                int copySize = (int)(pageLimit - position);

                Marshal.Copy((IntPtr)Translate(position), data, offset, copySize);

                position += copySize;
                offset   += copySize;
            }

            return data;
        }

        public void ReadBytes(long position, byte[] data, int startIndex, int size)
        {
            //Note: This will be moved later.
            long endAddr = position + size;

            if ((ulong)size > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if ((ulong)endAddr < (ulong)position)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            int offset = startIndex;

            while ((ulong)position < (ulong)endAddr)
            {
                long pageLimit = (position + PageSize) & ~(long)PageMask;

                if ((ulong)pageLimit > (ulong)endAddr)
                {
                    pageLimit = endAddr;
                }

                int copySize = (int)(pageLimit - position);

                Marshal.Copy((IntPtr)Translate(position), data, offset, copySize);

                position += copySize;
                offset   += copySize;
            }
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

        public void WriteByte(long position, byte value)
        {
            *((byte*)TranslateWrite(position)) = value;
        }

        public void WriteUInt16(long position, ushort value)
        {
            if ((position & 1) == 0)
            {
                *((ushort*)TranslateWrite(position)) = value;
            }
            else
            {
                WriteByte(position + 0, (byte)(value >> 0));
                WriteByte(position + 1, (byte)(value >> 8));
            }
        }

        public void WriteUInt32(long position, uint value)
        {
            if ((position & 3) == 0)
            {
                *((uint*)TranslateWrite(position)) = value;
            }
            else
            {
                WriteUInt16(position + 0, (ushort)(value >> 0));
                WriteUInt16(position + 2, (ushort)(value >> 16));
            }
        }

        public void WriteUInt64(long position, ulong value)
        {
            if ((position & 7) == 0)
            {
                *((ulong*)TranslateWrite(position)) = value;
            }
            else
            {
                WriteUInt32(position + 0, (uint)(value >> 0));
                WriteUInt32(position + 4, (uint)(value >> 32));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector8(long position, Vector128<float> value)
        {
            if (Sse41.IsSupported)
            {
                WriteByte(position, Sse41.Extract(Sse.StaticCast<float, byte>(value), 0));
            }
            else if (Sse2.IsSupported)
            {
                WriteByte(position, (byte)Sse2.Extract(Sse.StaticCast<float, ushort>(value), 0));
            }
            else
            {
                WriteByte(position, (byte)VectorHelper.VectorExtractIntZx(value, 0, 0));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector16(long position, Vector128<float> value)
        {
            if (Sse2.IsSupported)
            {
                WriteUInt16(position, Sse2.Extract(Sse.StaticCast<float, ushort>(value), 0));
            }
            else
            {
                WriteUInt16(position, (ushort)VectorHelper.VectorExtractIntZx(value, 0, 1));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector32(long position, Vector128<float> value)
        {
            if (Sse.IsSupported && (position & 3) == 0)
            {
                Sse.StoreScalar((float*)TranslateWrite(position), value);
            }
            else
            {
                WriteUInt32(position, (uint)VectorHelper.VectorExtractIntZx(value, 0, 2));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector64(long position, Vector128<float> value)
        {
            if (Sse2.IsSupported && (position & 7) == 0)
            {
                Sse2.StoreScalar((double*)TranslateWrite(position), Sse.StaticCast<float, double>(value));
            }
            else
            {
                WriteUInt64(position, VectorHelper.VectorExtractIntZx(value, 0, 3));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector128(long position, Vector128<float> value)
        {
            if (Sse.IsSupported && (position & 15) == 0)
            {
                Sse.Store((float*)TranslateWrite(position), value);
            }
            else
            {
                WriteUInt64(position + 0, VectorHelper.VectorExtractIntZx(value, 0, 3));
                WriteUInt64(position + 8, VectorHelper.VectorExtractIntZx(value, 1, 3));
            }
        }

        public void WriteBytes(long position, byte[] data)
        {
            long endAddr = position + data.Length;

            if ((ulong)endAddr < (ulong)position)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            int offset = 0;

            while ((ulong)position < (ulong)endAddr)
            {
                long pageLimit = (position + PageSize) & ~(long)PageMask;

                if ((ulong)pageLimit > (ulong)endAddr)
                {
                    pageLimit = endAddr;
                }

                int copySize = (int)(pageLimit - position);

                Marshal.Copy(data, offset, (IntPtr)TranslateWrite(position), copySize);

                position += copySize;
                offset   += copySize;
            }
        }

        public void WriteBytes(long position, byte[] data, int startIndex, int size)
        {
            //Note: This will be moved later.
            long endAddr = position + size;

            if ((ulong)endAddr < (ulong)position)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            int offset = startIndex;

            while ((ulong)position < (ulong)endAddr)
            {
                long pageLimit = (position + PageSize) & ~(long)PageMask;

                if ((ulong)pageLimit > (ulong)endAddr)
                {
                    pageLimit = endAddr;
                }

                int copySize = (int)(pageLimit - position);

                Marshal.Copy(data, offset, (IntPtr)TranslateWrite(position), copySize);

                position += copySize;
                offset   += copySize;
            }
        }

        public void CopyBytes(long src, long dst, long size)
        {
            //Note: This will be moved later.
            if (IsContiguous(src, size) &&
                IsContiguous(dst, size))
            {
                byte* srcPtr = Translate(src);
                byte* dstPtr = TranslateWrite(dst);

                Buffer.MemoryCopy(srcPtr, dstPtr, size, size);
            }
            else
            {
                WriteBytes(dst, ReadBytes(src, size));
            }
        }

        public void Map(long va, long pa, long size)
        {
            SetPtEntries(va, _ramPtr + pa, size);
        }

        public void Unmap(long position, long size)
        {
            SetPtEntries(position, null, size);

            StopObservingRegion(position, size);
        }

        public bool IsMapped(long position)
        {
            if (!(IsValidPosition(position)))
            {
                return false;
            }

            long l0 = (position >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (position >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return false;
            }

            return _pageTable[l0][l1] != null || _observedPages.ContainsKey(position >> PageBits);
        }

        public long GetPhysicalAddress(long virtualAddress)
        {
            byte* ptr = Translate(virtualAddress);

            return (long)(ptr - _ramPtr);
        }

        internal byte* Translate(long position)
        {
            long l0 = (position >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (position >> PtLvl1Bit) & PtLvl1Mask;

            long old = position;

            byte** lvl1 = _pageTable[l0];

            if ((position >> (PtLvl0Bit + PtLvl0Bits)) != 0)
            {
                goto Unmapped;
            }

            if (lvl1 == null)
            {
                goto Unmapped;
            }

            position &= PageMask;

            byte* ptr = lvl1[l1];

            if (ptr == null)
            {
                goto Unmapped;
            }

            return ptr + position;

Unmapped:
            return HandleNullPte(old);
        }

        private byte* HandleNullPte(long position)
        {
            long key = position >> PageBits;

            if (_observedPages.TryGetValue(key, out IntPtr ptr))
            {
                return (byte*)ptr + (position & PageMask);
            }

            InvalidAccess?.Invoke(this, new MemoryAccessEventArgs(position));

            throw new VmmPageFaultException(position);
        }

        internal byte* TranslateWrite(long position)
        {
            long l0 = (position >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (position >> PtLvl1Bit) & PtLvl1Mask;

            long old = position;

            byte** lvl1 = _pageTable[l0];

            if ((position >> (PtLvl0Bit + PtLvl0Bits)) != 0)
            {
                goto Unmapped;
            }

            if (lvl1 == null)
            {
                goto Unmapped;
            }

            position &= PageMask;

            byte* ptr = lvl1[l1];

            if (ptr == null)
            {
                goto Unmapped;
            }

            return ptr + position;

Unmapped:
            return HandleNullPteWrite(old);
        }

        private byte* HandleNullPteWrite(long position)
        {
            long key = position >> PageBits;

            MemoryAccessEventArgs e = new MemoryAccessEventArgs(position);

            if (_observedPages.TryGetValue(key, out IntPtr ptr))
            {
                SetPtEntry(position, (byte*)ptr);

                ObservedAccess?.Invoke(this, e);

                return (byte*)ptr + (position & PageMask);
            }

            InvalidAccess?.Invoke(this, e);

            throw new VmmPageFaultException(position);
        }

        private void SetPtEntries(long va, byte* ptr, long size)
        {
            long endPosition = (va + size + PageMask) & ~PageMask;

            while ((ulong)va < (ulong)endPosition)
            {
                SetPtEntry(va, ptr);

                va += PageSize;

                if (ptr != null)
                {
                    ptr += PageSize;
                }
            }
        }

        private void SetPtEntry(long position, byte* ptr)
        {
            if (!IsValidPosition(position))
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            long l0 = (position >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (position >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                byte** lvl1 = (byte**)Marshal.AllocHGlobal(PtLvl1Size * IntPtr.Size);

                for (int zl1 = 0; zl1 < PtLvl1Size; zl1++)
                {
                    lvl1[zl1] = null;
                }

                Thread.MemoryBarrier();

                _pageTable[l0] = lvl1;
            }

            _pageTable[l0][l1] = ptr;
        }

        public void StartObservingRegion(long position, long size)
        {
            long endPosition = (position + size + PageMask) & ~PageMask;

            position &= ~PageMask;

            while ((ulong)position < (ulong)endPosition)
            {
                _observedPages[position >> PageBits] = (IntPtr)Translate(position);

                SetPtEntry(position, null);

                position += PageSize;
            }
        }

        public void StopObservingRegion(long position, long size)
        {
            long endPosition = (position + size + PageMask) & ~PageMask;

            while (position < endPosition)
            {
                lock (_observedPages)
                {
                    if (_observedPages.TryRemove(position >> PageBits, out IntPtr ptr))
                    {
                        SetPtEntry(position, (byte*)ptr);
                    }
                }

                position += PageSize;
            }
        }

        public bool TryGetHostAddress(long position, long size, out IntPtr ptr)
        {
            if (IsContiguous(position, size))
            {
                ptr = (IntPtr)Translate(position);

                return true;
            }

            ptr = IntPtr.Zero;

            return false;
        }

        private bool IsContiguous(long position, long size)
        {
            long endPos = position + size;

            position &= ~PageMask;

            long expectedPa = GetPhysicalAddress(position);

            while ((ulong)position < (ulong)endPos)
            {
                long pa = GetPhysicalAddress(position);

                if (pa != expectedPa)
                {
                    return false;
                }

                position   += PageSize;
                expectedPa += PageSize;
            }

            return true;
        }

        public bool IsValidPosition(long position)
        {
            return position >> (PtLvl0Bits + PtLvl1Bits + PageBits) == 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_pageTable == null)
            {
                return;
            }

            for (int l0 = 0; l0 < PtLvl0Size; l0++)
            {
                if (_pageTable[l0] != null)
                {
                    Marshal.FreeHGlobal((IntPtr)_pageTable[l0]);
                }

                _pageTable[l0] = null;
            }

            Marshal.FreeHGlobal((IntPtr)_pageTable);

            _pageTable = null;
        }
    }
}