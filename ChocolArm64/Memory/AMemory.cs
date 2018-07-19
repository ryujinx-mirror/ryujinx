using ChocolArm64.Exceptions;
using ChocolArm64.State;
using System;
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
        private const long ErgMask = (4 << AThreadState.ErgSizeLog2) - 1;

        public AMemoryMgr Manager { get; private set; }

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

        public IntPtr Ram { get; private set; }

        private byte* RamPtr;

        private int HostPageSize;

        public AMemory()
        {
            Manager = new AMemoryMgr();

            Monitors = new Dictionary<int, ArmMonitor>();

            IntPtr Size = (IntPtr)AMemoryMgr.RamSize + AMemoryMgr.PageSize;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ram = AMemoryWin32.Allocate(Size);

                HostPageSize = AMemoryWin32.GetPageSize(Ram, Size);
            }
            else
            {
                Ram = Marshal.AllocHGlobal(Size);
            }

            RamPtr = (byte*)Ram;
        }

        public void RemoveMonitor(AThreadState State)
        {
            lock (Monitors)
            {
                ClearExclusive(State);

                Monitors.Remove(State.ThreadId);
            }
        }

        public void SetExclusive(AThreadState ThreadState, long Position)
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

                if (!Monitors.TryGetValue(ThreadState.ThreadId, out ArmMonitor ThreadMon))
                {
                    ThreadMon = new ArmMonitor();

                    Monitors.Add(ThreadState.ThreadId, ThreadMon);
                }

                ThreadMon.Position = Position;
                ThreadMon.ExState  = true;
            }
        }

        public bool TestExclusive(AThreadState ThreadState, long Position)
        {
            //Note: Any call to this method also should be followed by a
            //call to ClearExclusiveForStore if this method returns true.
            Position &= ~ErgMask;

            Monitor.Enter(Monitors);

            if (!Monitors.TryGetValue(ThreadState.ThreadId, out ArmMonitor ThreadMon))
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

        public void ClearExclusiveForStore(AThreadState ThreadState)
        {
            if (Monitors.TryGetValue(ThreadState.ThreadId, out ArmMonitor ThreadMon))
            {
                ThreadMon.ExState = false;
            }

            Monitor.Exit(Monitors);
        }

        public void ClearExclusive(AThreadState ThreadState)
        {
            lock (Monitors)
            {
                if (Monitors.TryGetValue(ThreadState.ThreadId, out ArmMonitor ThreadMon))
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

        public int GetHostPageSize()
        {
            return HostPageSize;
        }

        public bool[] IsRegionModified(long Position, long Size)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return null;
            }

            long EndPos = Position + Size;

            if ((ulong)EndPos < (ulong)Position)
            {
                return null;
            }

            if ((ulong)EndPos > AMemoryMgr.RamSize)
            {
                return null;
            }

            IntPtr MemAddress = new IntPtr(RamPtr + Position);
            IntPtr MemSize    = new IntPtr(Size);

            int HostPageMask = HostPageSize - 1;

            Position &= ~HostPageMask;

            Size = EndPos - Position;

            IntPtr[] Addresses  = new IntPtr[(Size + HostPageMask) / HostPageSize];

            AMemoryWin32.IsRegionModified(MemAddress, MemSize, Addresses, out int Count);

            bool[] Modified = new bool[Addresses.Length];

            for (int Index = 0; Index < Count; Index++)
            {
                long VA = Addresses[Index].ToInt64() - Ram.ToInt64();

                Modified[(VA - Position) / HostPageSize] = true;
            }

            return Modified;
        }

        public IntPtr GetHostAddress(long Position, long Size)
        {
            EnsureRangeIsValid(Position, Size, AMemoryPerm.Read);

            return (IntPtr)(RamPtr + (ulong)Position);
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
            EnsureAccessIsValid(Position, AMemoryPerm.Read);

            return ReadByteUnchecked(Position);
        }

        public ushort ReadUInt16(long Position)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Read);
            EnsureAccessIsValid(Position + 1, AMemoryPerm.Read);

            return ReadUInt16Unchecked(Position);
        }

        public uint ReadUInt32(long Position)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Read);
            EnsureAccessIsValid(Position + 3, AMemoryPerm.Read);

            return ReadUInt32Unchecked(Position);
        }

        public ulong ReadUInt64(long Position)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Read);
            EnsureAccessIsValid(Position + 7, AMemoryPerm.Read);

            return ReadUInt64Unchecked(Position);
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

        public Vector128<float> ReadVector32(long Position)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Read);
            EnsureAccessIsValid(Position + 3, AMemoryPerm.Read);

            if (Sse.IsSupported)
            {
                return Sse.LoadScalarVector128((float*)(RamPtr + (uint)Position));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public Vector128<float> ReadVector64(long Position)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Read);
            EnsureAccessIsValid(Position + 7, AMemoryPerm.Read);

            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<double, float>(Sse2.LoadScalarVector128((double*)(RamPtr + (uint)Position)));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public Vector128<float> ReadVector128(long Position)
        {
            EnsureAccessIsValid(Position + 0,  AMemoryPerm.Read);
            EnsureAccessIsValid(Position + 15, AMemoryPerm.Read);

            if (Sse.IsSupported)
            {
                return Sse.LoadVector128((float*)(RamPtr + (uint)Position));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public sbyte ReadSByteUnchecked(long Position)
        {
            return (sbyte)ReadByteUnchecked(Position);
        }

        public short ReadInt16Unchecked(long Position)
        {
            return (short)ReadUInt16Unchecked(Position);
        }

        public int ReadInt32Unchecked(long Position)
        {
            return (int)ReadUInt32Unchecked(Position);
        }

        public long ReadInt64Unchecked(long Position)
        {
            return (long)ReadUInt64Unchecked(Position);
        }

        public byte ReadByteUnchecked(long Position)
        {
            return *((byte*)(RamPtr + (uint)Position));
        }

        public ushort ReadUInt16Unchecked(long Position)
        {
            return *((ushort*)(RamPtr + (uint)Position));
        }

        public uint ReadUInt32Unchecked(long Position)
        {
            return *((uint*)(RamPtr + (uint)Position));
        }

        public ulong ReadUInt64Unchecked(long Position)
        {
            return *((ulong*)(RamPtr + (uint)Position));
        }

        public Vector128<float> ReadVector8Unchecked(long Position)
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
        public Vector128<float> ReadVector16Unchecked(long Position)
        {
            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<ushort, float>(Sse2.Insert(Sse2.SetZeroVector128<ushort>(), ReadUInt16Unchecked(Position), 0));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Vector128<float> ReadVector32Unchecked(long Position)
        {
            if (Sse.IsSupported)
            {
                return Sse.LoadScalarVector128((float*)(RamPtr + (uint)Position));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Vector128<float> ReadVector64Unchecked(long Position)
        {
            if (Sse2.IsSupported)
            {
                return Sse.StaticCast<double, float>(Sse2.LoadScalarVector128((double*)(RamPtr + (uint)Position)));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> ReadVector128Unchecked(long Position)
        {
            if (Sse.IsSupported)
            {
                return Sse.LoadVector128((float*)(RamPtr + (uint)Position));
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

            EnsureRangeIsValid(Position, Size, AMemoryPerm.Read);

            byte[] Data = new byte[Size];

            Marshal.Copy((IntPtr)(RamPtr + (uint)Position), Data, 0, (int)Size);

            return Data;
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
            EnsureAccessIsValid(Position, AMemoryPerm.Write);

            WriteByteUnchecked(Position, Value);
        }

        public void WriteUInt16(long Position, ushort Value)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Write);
            EnsureAccessIsValid(Position + 1, AMemoryPerm.Write);

            WriteUInt16Unchecked(Position, Value);
        }

        public void WriteUInt32(long Position, uint Value)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Write);
            EnsureAccessIsValid(Position + 3, AMemoryPerm.Write);

            WriteUInt32Unchecked(Position, Value);
        }

        public void WriteUInt64(long Position, ulong Value)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Write);
            EnsureAccessIsValid(Position + 7, AMemoryPerm.Write);

            WriteUInt64Unchecked(Position, Value);
        }

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

        public void WriteVector32(long Position, Vector128<float> Value)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Write);
            EnsureAccessIsValid(Position + 3, AMemoryPerm.Write);

            if (Sse.IsSupported)
            {
                Sse.StoreScalar((float*)(RamPtr + (uint)Position), Value);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public void WriteVector64(long Position, Vector128<float> Value)
        {
            EnsureAccessIsValid(Position + 0, AMemoryPerm.Write);
            EnsureAccessIsValid(Position + 7, AMemoryPerm.Write);

            if (Sse2.IsSupported)
            {
                Sse2.StoreScalar((double*)(RamPtr + (uint)Position), Sse.StaticCast<float, double>(Value));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public void WriteVector128(long Position, Vector128<float> Value)
        {
            EnsureAccessIsValid(Position + 0,  AMemoryPerm.Write);
            EnsureAccessIsValid(Position + 15, AMemoryPerm.Write);

            if (Sse.IsSupported)
            {
                Sse.Store((float*)(RamPtr + (uint)Position), Value);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public void WriteSByteUnchecked(long Position, sbyte Value)
        {
            WriteByteUnchecked(Position, (byte)Value);
        }

        public void WriteInt16Unchecked(long Position, short Value)
        {
            WriteUInt16Unchecked(Position, (ushort)Value);
        }

        public void WriteInt32Unchecked(long Position, int Value)
        {
            WriteUInt32Unchecked(Position, (uint)Value);
        }

        public void WriteInt64Unchecked(long Position, long Value)
        {
            WriteUInt64Unchecked(Position, (ulong)Value);
        }

        public void WriteByteUnchecked(long Position, byte Value)
        {
            *((byte*)(RamPtr + (uint)Position)) = Value;
        }

        public void WriteUInt16Unchecked(long Position, ushort Value)
        {
            *((ushort*)(RamPtr + (uint)Position)) = Value;
        }

        public void WriteUInt32Unchecked(long Position, uint Value)
        {
            *((uint*)(RamPtr + (uint)Position)) = Value;
        }

        public void WriteUInt64Unchecked(long Position, ulong Value)
        {
            *((ulong*)(RamPtr + (uint)Position)) = Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector8Unchecked(long Position, Vector128<float> Value)
        {
            if (Sse41.IsSupported)
            {
                WriteByteUnchecked(Position, Sse41.Extract(Sse.StaticCast<float, byte>(Value), 0));
            }
            else if (Sse2.IsSupported)
            {
                WriteByteUnchecked(Position, (byte)Sse2.Extract(Sse.StaticCast<float, ushort>(Value), 0));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector16Unchecked(long Position, Vector128<float> Value)
        {
            if (Sse2.IsSupported)
            {
                WriteUInt16Unchecked(Position, Sse2.Extract(Sse.StaticCast<float, ushort>(Value), 0));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteVector32Unchecked(long Position, Vector128<float> Value)
        {
            if (Sse.IsSupported)
            {
                Sse.StoreScalar((float*)(RamPtr + (uint)Position), Value);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteVector64Unchecked(long Position, Vector128<float> Value)
        {
            if (Sse2.IsSupported)
            {
                Sse2.StoreScalar((double*)(RamPtr + (uint)Position), Sse.StaticCast<float, double>(Value));
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector128Unchecked(long Position, Vector128<float> Value)
        {
            if (Sse.IsSupported)
            {
                Sse.Store((float*)(RamPtr + (uint)Position), Value);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public void WriteBytes(long Position, byte[] Data)
        {
            EnsureRangeIsValid(Position, (uint)Data.Length, AMemoryPerm.Write);

            Marshal.Copy(Data, 0, (IntPtr)(RamPtr + (uint)Position), Data.Length);
        }

        private void EnsureRangeIsValid(long Position, long Size, AMemoryPerm Perm)
        {
            long EndPos = Position + Size;

            Position &= ~AMemoryMgr.PageMask;

            while ((ulong)Position < (ulong)EndPos)
            {
                EnsureAccessIsValid(Position, Perm);

                Position += AMemoryMgr.PageSize;
            }
        }

        private void EnsureAccessIsValid(long Position, AMemoryPerm Perm)
        {
            if (!Manager.IsMapped(Position))
            {
                throw new VmmPageFaultException(Position);
            }

            if (!Manager.HasPermission(Position, Perm))
            {
                throw new VmmAccessViolationException(Position, Perm);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Ram != IntPtr.Zero)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    AMemoryWin32.Free(Ram);
                }
                else
                {
                    Marshal.FreeHGlobal(Ram);
                }

                Ram = IntPtr.Zero;
            }
        }
    }
}