using ChocolArm64.Exceptions;
using ChocolArm64.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace ChocolArm64.Memory
{
    public unsafe class AMemory : IAMemory, IDisposable
    {
        private const int PTLvl0Bits = 13;
        private const int PTLvl1Bits = 14;
        private const int PTPageBits = 12;

        private const int PTLvl0Size = 1 << PTLvl0Bits;
        private const int PTLvl1Size = 1 << PTLvl1Bits;
        public  const int PageSize   = 1 << PTPageBits;

        private const int PTLvl0Mask = PTLvl0Size - 1;
        private const int PTLvl1Mask = PTLvl1Size - 1;
        public  const int PageMask   = PageSize   - 1;

        private const int PTLvl0Bit = PTPageBits + PTLvl1Bits;
        private const int PTLvl1Bit = PTPageBits;

        private const long ErgMask = (4 << AThreadState.ErgSizeLog2) - 1;

        private class ArmMonitor
        {
            public long Position;
            public bool ExState;

            public bool HasExclusiveAccess(long Position)
            {
                return this.Position == Position && ExState;
            }
        }

        private Dictionary<int, ArmMonitor> Monitors;

        private ConcurrentDictionary<long, IntPtr> ObservedPages;

        public IntPtr Ram { get; private set; }

        private byte* RamPtr;

        private byte*** PageTable;

        public AMemory(IntPtr Ram)
        {
            Monitors = new Dictionary<int, ArmMonitor>();

            ObservedPages = new ConcurrentDictionary<long, IntPtr>();

            this.Ram = Ram;

            RamPtr = (byte*)Ram;

            PageTable = (byte***)Marshal.AllocHGlobal(PTLvl0Size * IntPtr.Size);

            for (int L0 = 0; L0 < PTLvl0Size; L0++)
            {
                PageTable[L0] = null;
            }
        }

        public void RemoveMonitor(int Core)
        {
            lock (Monitors)
            {
                ClearExclusive(Core);

                Monitors.Remove(Core);
            }
        }

        public void SetExclusive(int Core, long Position)
        {
            Position &= ~ErgMask;

            lock (Monitors)
            {
                foreach (ArmMonitor Mon in Monitors.Values)
                {
                    if (Mon.Position == Position && Mon.ExState)
                    {
                        Mon.ExState = false;
                    }
                }

                if (!Monitors.TryGetValue(Core, out ArmMonitor ThreadMon))
                {
                    ThreadMon = new ArmMonitor();

                    Monitors.Add(Core, ThreadMon);
                }

                ThreadMon.Position = Position;
                ThreadMon.ExState  = true;
            }
        }

        public bool TestExclusive(int Core, long Position)
        {
            //Note: Any call to this method also should be followed by a
            //call to ClearExclusiveForStore if this method returns true.
            Position &= ~ErgMask;

            Monitor.Enter(Monitors);

            if (!Monitors.TryGetValue(Core, out ArmMonitor ThreadMon))
            {
                return false;
            }

            bool ExState = ThreadMon.HasExclusiveAccess(Position);

            if (!ExState)
            {
                Monitor.Exit(Monitors);
            }

            return ExState;
        }

        public void ClearExclusiveForStore(int Core)
        {
            if (Monitors.TryGetValue(Core, out ArmMonitor ThreadMon))
            {
                ThreadMon.ExState = false;
            }

            Monitor.Exit(Monitors);
        }

        public void ClearExclusive(int Core)
        {
            lock (Monitors)
            {
                if (Monitors.TryGetValue(Core, out ArmMonitor ThreadMon))
                {
                    ThreadMon.ExState = false;
                }
            }
        }

        public void WriteInt32ToSharedAddr(long Position, int Value)
        {
            long MaskedPosition = Position & ~ErgMask;

            lock (Monitors)
            {
                foreach (ArmMonitor Mon in Monitors.Values)
                {
                    if (Mon.Position == MaskedPosition && Mon.ExState)
                    {
                        Mon.ExState = false;
                    }
                }

                WriteInt32(Position, Value);
            }
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

        public byte ReadByte(long Position)
        {
            return *((byte*)Translate(Position));
        }

        public ushort ReadUInt16(long Position)
        {
            return *((ushort*)Translate(Position));
        }

        public uint ReadUInt32(long Position)
        {
            return *((uint*)Translate(Position));
        }

        public ulong ReadUInt64(long Position)
        {
            return *((ulong*)Translate(Position));
        }

        public Vector128<float> ReadVector8(long Position)
        {
            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<byte, float>(Sse2.SetVector128(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ReadByte(Position)));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector16(long Position)
        {
            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse2.SetZeroVector128<ushort>(), ReadUInt16(Position), 0));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector32(long Position)
        {
            if (Sse.IsSupported)
            {
                return Sse.LoadScalarVector128((float*)Translate(Position));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector64(long Position)
        {
            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<double, float>(Sse2.LoadScalarVector128((double*)Translate(Position)));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector128(long Position)
        {
            if (Sse.IsSupported)
            {
                return Sse.LoadVector128((float*)Translate(Position));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public byte[] ReadBytes(long Position, long Size)
        {
            if ((uint)Size > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            EnsureRangeIsValid(Position, Size);

            byte[] Data = new byte[Size];

            Marshal.Copy((IntPtr)Translate(Position), Data, 0, (int)Size);

            return Data;
        }

        public void ReadBytes(long Position, byte[] Data, int StartIndex, int Size)
        {
            //Note: This will be moved later.
            EnsureRangeIsValid(Position, (uint)Size);

            Marshal.Copy((IntPtr)Translate(Position), Data, StartIndex, Size);
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

        public void WriteByte(long Position, byte Value)
        {
            *((byte*)TranslateWrite(Position)) = Value;
        }

        public void WriteUInt16(long Position, ushort Value)
        {
            *((ushort*)TranslateWrite(Position)) = Value;
        }

        public void WriteUInt32(long Position, uint Value)
        {
            *((uint*)TranslateWrite(Position)) = Value;
        }

        public void WriteUInt64(long Position, ulong Value)
        {
            *((ulong*)TranslateWrite(Position)) = Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector8(long Position, Vector128<float> Value)
        {
            if (Sse41.IsSupported)
            {
                WriteByte(Position, Sse41.Extract(Sse.StaticCast<float, byte>(Value), 0));
            }
            else if (Sse2.IsSupported)
            {
                WriteByte(Position, (byte)Sse2.Extract(Sse.StaticCast<float, ushort>(Value), 0));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector16(long Position, Vector128<float> Value)
        {
            if (Sse2.IsSupported)
            {
                WriteUInt16(Position, Sse2.Extract(Sse.StaticCast<float, ushort>(Value), 0));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector32(long Position, Vector128<float> Value)
        {
            if (Sse.IsSupported)
            {
                Sse.StoreScalar((float*)TranslateWrite(Position), Value);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector64(long Position, Vector128<float> Value)
        {
            if (Sse2.IsSupported)
            {
                Sse2.StoreScalar((double*)TranslateWrite(Position), Sse.StaticCast<float, double>(Value));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector128(long Position, Vector128<float> Value)
        {
            if (Sse.IsSupported)
            {
                Sse.Store((float*)TranslateWrite(Position), Value);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public void WriteBytes(long Position, byte[] Data)
        {
            EnsureRangeIsValid(Position, (uint)Data.Length);

            Marshal.Copy(Data, 0, (IntPtr)TranslateWrite(Position), Data.Length);
        }

        public void WriteBytes(long Position, byte[] Data, int StartIndex, int Size)
        {
            //Note: This will be moved later.
            //Using Translate instead of TranslateWrite is on purpose.
            EnsureRangeIsValid(Position, (uint)Size);

            Marshal.Copy(Data, StartIndex, (IntPtr)Translate(Position), Size);
        }

        public void CopyBytes(long Src, long Dst, long Size)
        {
            //Note: This will be moved later.
            EnsureRangeIsValid(Src, Size);
            EnsureRangeIsValid(Dst, Size);

            byte* SrcPtr = Translate(Src);
            byte* DstPtr = TranslateWrite(Dst);

            Buffer.MemoryCopy(SrcPtr, DstPtr, Size, Size);
        }

        public void Map(long VA, long PA, long Size)
        {
            SetPTEntries(VA, RamPtr + PA, Size);
        }

        public void Unmap(long Position, long Size)
        {
            SetPTEntries(Position, null, Size);

            StopObservingRegion(Position, Size);
        }

        public bool IsMapped(long Position)
        {
            if (!(IsValidPosition(Position)))
            {
                return false;
            }

            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                return false;
            }

            return PageTable[L0][L1] != null || ObservedPages.ContainsKey(Position >> PTPageBits);
        }

        public long GetPhysicalAddress(long VirtualAddress)
        {
            byte* Ptr = Translate(VirtualAddress);

            return (long)(Ptr - RamPtr);
        }

        internal byte* Translate(long Position)
        {
            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            long Old = Position;

            byte** Lvl1 = PageTable[L0];

            if ((Position >> (PTLvl0Bit + PTLvl0Bits)) != 0)
            {
                goto Unmapped;
            }

            if (Lvl1 == null)
            {
                goto Unmapped;
            }

            Position &= PageMask;

            byte* Ptr = Lvl1[L1];

            if (Ptr == null)
            {
                goto Unmapped;
            }

            return Ptr + Position;

Unmapped:
            return HandleNullPte(Old);
        }

        private byte* HandleNullPte(long Position)
        {
            long Key = Position >> PTPageBits;

            if (ObservedPages.TryGetValue(Key, out IntPtr Ptr))
            {
                return (byte*)Ptr + (Position & PageMask);
            }

            throw new VmmPageFaultException(Position);
        }

        internal byte* TranslateWrite(long Position)
        {
            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            long Old = Position;

            byte** Lvl1 = PageTable[L0];

            if ((Position >> (PTLvl0Bit + PTLvl0Bits)) != 0)
            {
                goto Unmapped;
            }

            if (Lvl1 == null)
            {
                goto Unmapped;
            }

            Position &= PageMask;

            byte* Ptr = Lvl1[L1];

            if (Ptr == null)
            {
                goto Unmapped;
            }

            return Ptr + Position;

Unmapped:
            return HandleNullPteWrite(Old);
        }

        private byte* HandleNullPteWrite(long Position)
        {
            long Key = Position >> PTPageBits;

            if (ObservedPages.TryGetValue(Key, out IntPtr Ptr))
            {
                SetPTEntry(Position, (byte*)Ptr);

                return (byte*)Ptr + (Position & PageMask);
            }

            throw new VmmPageFaultException(Position);
        }

        private void SetPTEntries(long VA, byte* Ptr, long Size)
        {
            long EndPosition = (VA + Size + PageMask) & ~PageMask;

            while ((ulong)VA < (ulong)EndPosition)
            {
                SetPTEntry(VA, Ptr);

                VA += PageSize;

                if (Ptr != null)
                {
                    Ptr += PageSize;
                }
            }
        }

        private void SetPTEntry(long Position, byte* Ptr)
        {
            if (!IsValidPosition(Position))
            {
                throw new ArgumentOutOfRangeException(nameof(Position));
            }

            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                byte** Lvl1 = (byte**)Marshal.AllocHGlobal(PTLvl1Size * IntPtr.Size);

                for (int ZL1 = 0; ZL1 < PTLvl1Size; ZL1++)
                {
                    Lvl1[ZL1] = null;
                }

                Thread.MemoryBarrier();

                PageTable[L0] = Lvl1;
            }

            PageTable[L0][L1] = Ptr;
        }

        public (bool[], int) IsRegionModified(long Position, long Size)
        {
            long EndPosition = (Position + Size + PageMask) & ~PageMask;

            Position &= ~PageMask;

            Size = EndPosition - Position;

            bool[] Modified = new bool[Size >> PTPageBits];

            int Count = 0;

            lock (ObservedPages)
            {
                for (int Page = 0; Page < Modified.Length; Page++)
                {
                    byte* Ptr = Translate(Position);

                    if (ObservedPages.TryAdd(Position >> PTPageBits, (IntPtr)Ptr))
                    {
                        Modified[Page] = true;

                        Count++;
                    }
                    else
                    {
                        long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
                        long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

                        byte** Lvl1 = PageTable[L0];

                        if (Lvl1 != null)
                        {
                            if (Modified[Page] = Lvl1[L1] != null)
                            {
                                Count++;
                            }
                        }
                    }

                    SetPTEntry(Position, null);

                    Position += PageSize;
                }
            }

            return (Modified, Count);
        }

        public void StopObservingRegion(long Position, long Size)
        {
            long EndPosition = (Position + Size + PageMask) & ~PageMask;

            while (Position < EndPosition)
            {
                lock (ObservedPages)
                {
                    if (ObservedPages.TryRemove(Position >> PTPageBits, out IntPtr Ptr))
                    {
                        SetPTEntry(Position, (byte*)Ptr);
                    }
                }

                Position += PageSize;
            }
        }

        public IntPtr GetHostAddress(long Position, long Size)
        {
            EnsureRangeIsValid(Position, Size);

            return (IntPtr)Translate(Position);
        }

        internal void EnsureRangeIsValid(long Position, long Size)
        {
            long EndPos = Position + Size;

            Position &= ~PageMask;

            long ExpectedPA = GetPhysicalAddress(Position);

            while ((ulong)Position < (ulong)EndPos)
            {
                long PA = GetPhysicalAddress(Position);

                if (PA != ExpectedPA)
                {
                    throw new VmmAccessException(Position, Size);
                }

                Position   += PageSize;
                ExpectedPA += PageSize;
            }
        }

        public bool IsValidPosition(long Position)
        {
            return Position >> (PTLvl0Bits + PTLvl1Bits + PTPageBits) == 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (PageTable == null)
            {
                return;
            }

            for (int L0 = 0; L0 < PTLvl0Size; L0++)
            {
                if (PageTable[L0] != null)
                {
                    Marshal.FreeHGlobal((IntPtr)PageTable[L0]);
                }

                PageTable[L0] = null;
            }

            Marshal.FreeHGlobal((IntPtr)PageTable);

            PageTable = null;
        }
    }
}