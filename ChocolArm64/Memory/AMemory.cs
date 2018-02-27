using ChocolArm64.Exceptions;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ChocolArm64.Memory
{
    public unsafe class AMemory
    {
        private const long ErgMask = (4 << AThreadState.ErgSizeLog2) - 1;

        public AMemoryMgr Manager { get; private set; }

        private struct ExMonitor
        {
            public long Position { get; private set; }

            private bool ExState;

            public ExMonitor(long Position, bool ExState)
            {
                this.Position = Position;
                this.ExState  = ExState;
            }

            public bool HasExclusiveAccess(long Position)
            {
                return this.Position == Position && ExState;
            }

            public void Reset()
            {
                ExState = false;
            }
        }

        private Dictionary<int, ExMonitor> Monitors;

        private HashSet<long> ExAddrs;

        private byte* RamPtr;

        public AMemory(IntPtr Ram)
        {
            Manager = new AMemoryMgr();

            Monitors = new Dictionary<int, ExMonitor>();

            ExAddrs = new HashSet<long>();

            RamPtr = (byte*)Ram;
        }

        public void RemoveMonitor(int ThreadId)
        {
            lock (Monitors)
            {
                if (Monitors.TryGetValue(ThreadId, out ExMonitor Monitor))
                {
                    ExAddrs.Remove(Monitor.Position);
                }

                Monitors.Remove(ThreadId);
            }
        }

        public void SetExclusive(AThreadState ThreadState, long Position)
        {
            Position &= ~ErgMask;

            lock (Monitors)
            {
                if (Monitors.TryGetValue(ThreadState.ThreadId, out ExMonitor Monitor))
                {
                    ExAddrs.Remove(Monitor.Position);
                }

                bool ExState = ExAddrs.Add(Position);

                Monitor = new ExMonitor(Position, ExState);

                if (!Monitors.TryAdd(ThreadState.ThreadId, Monitor))
                {
                    Monitors[ThreadState.ThreadId] = Monitor;
                }
            }
        }

        public bool TestExclusive(AThreadState ThreadState, long Position)
        {
            Position &= ~ErgMask;

            lock (Monitors)
            {
                if (!Monitors.TryGetValue(ThreadState.ThreadId, out ExMonitor Monitor))
                {
                    return false;
                }

                return Monitor.HasExclusiveAccess(Position);
            }
        }

        public void ClearExclusive(AThreadState ThreadState)
        {
            lock (Monitors)
            {
                if (Monitors.TryGetValue(ThreadState.ThreadId, out ExMonitor Monitor))
                {
                    Monitor.Reset();
                    ExAddrs.Remove(Monitor.Position);
                }
            }
        }

        public bool AcquireAddress(long Position)
        {
            Position &= ~ErgMask;

            lock (Monitors)
            {
                return ExAddrs.Add(Position);
            }
        }

        public void ReleaseAddress(long Position)
        {
            Position &= ~ErgMask;

            lock (Monitors)
            {
                ExAddrs.Remove(Position);
            }
        }

        public sbyte ReadSByte(long Position) => (sbyte)ReadByte  (Position);
        public short ReadInt16(long Position) => (short)ReadUInt16(Position);
        public int   ReadInt32(long Position) =>   (int)ReadUInt32(Position);
        public long  ReadInt64(long Position) =>  (long)ReadUInt64(Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return *((byte*)(RamPtr + (uint)Position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return *((ushort*)(RamPtr + (uint)Position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return *((uint*)(RamPtr + (uint)Position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return *((ulong*)(RamPtr + (uint)Position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AVec ReadVector8(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return new AVec() { B0 = ReadByte(Position) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AVec ReadVector16(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return new AVec() { H0 = ReadUInt16(Position) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AVec ReadVector32(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return new AVec() { W0 = ReadUInt32(Position) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AVec ReadVector64(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return new AVec() { X0 = ReadUInt64(Position) };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AVec ReadVector128(long Position)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Read);
#endif

            return new AVec()
            {
                X0 = ReadUInt64(Position + 0),
                X1 = ReadUInt64(Position + 8)
            };
        }

        public void WriteSByte(long Position, sbyte Value) => WriteByte  (Position,   (byte)Value);
        public void WriteInt16(long Position, short Value) => WriteUInt16(Position, (ushort)Value);
        public void WriteInt32(long Position, int   Value) => WriteUInt32(Position,   (uint)Value);
        public void WriteInt64(long Position, long  Value) => WriteUInt64(Position,  (ulong)Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(long Position, byte Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            *((byte*)(RamPtr + (uint)Position)) = Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(long Position, ushort Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            *((ushort*)(RamPtr + (uint)Position)) = Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt32(long Position, uint Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            *((uint*)(RamPtr + (uint)Position)) = Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(long Position, ulong Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            *((ulong*)(RamPtr + (uint)Position)) = Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector8(long Position, AVec Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            WriteByte(Position, Value.B0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector16(long Position, AVec Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            WriteUInt16(Position, Value.H0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector32(long Position, AVec Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            WriteUInt32(Position, Value.W0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector64(long Position, AVec Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            WriteUInt64(Position, Value.X0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVector128(long Position, AVec Value)
        {
#if DEBUG
            EnsureAccessIsValid(Position, AMemoryPerm.Write);
#endif

            WriteUInt64(Position + 0, Value.X0);
            WriteUInt64(Position + 8, Value.X1);
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
    }
}