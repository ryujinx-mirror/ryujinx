using ChocolArm64.Exceptions;
using ChocolArm64.State;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ChocolArm64.Memory
{
    public unsafe class AMemory : IAMemory, IDisposable
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

        public IntPtr Ram { get; private set; }

        private byte* RamPtr;

        public AMemory()
        {
            Manager = new AMemoryMgr();

            Monitors = new Dictionary<int, ExMonitor>();

            ExAddrs = new HashSet<long>();

            Ram = Marshal.AllocHGlobal((IntPtr)AMemoryMgr.RamSize + AMemoryMgr.PageSize);

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

        public AVec ReadVector8(long Position)
        {
            return new AVec() { B0 = ReadByte(Position) };
        }

        public AVec ReadVector16(long Position)
        {
            return new AVec() { H0 = ReadUInt16(Position) };
        }

        public AVec ReadVector32(long Position)
        {
            return new AVec() { W0 = ReadUInt32(Position) };
        }

        public AVec ReadVector64(long Position)
        {
            return new AVec() { X0 = ReadUInt64(Position) };
        }

        public AVec ReadVector128(long Position)
        {
            return new AVec()
            {
                X0 = ReadUInt64(Position + 0),
                X1 = ReadUInt64(Position + 8)
            };
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

        public AVec ReadVector8Unchecked(long Position)
        {
            return new AVec() { B0 = ReadByteUnchecked(Position) };
        }

        public AVec ReadVector16Unchecked(long Position)
        {
            return new AVec() { H0 = ReadUInt16Unchecked(Position) };
        }

        public AVec ReadVector32Unchecked(long Position)
        {
            return new AVec() { W0 = ReadUInt32Unchecked(Position) };
        }

        public AVec ReadVector64Unchecked(long Position)
        {
            return new AVec() { X0 = ReadUInt64Unchecked(Position) };
        }

        public AVec ReadVector128Unchecked(long Position)
        {
            return new AVec()
            {
                X0 = ReadUInt64Unchecked(Position + 0),
                X1 = ReadUInt64Unchecked(Position + 8)
            };
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

        public void WriteVector8(long Position, AVec Value)
        {
            WriteByte(Position, Value.B0);
        }

        public void WriteVector16(long Position, AVec Value)
        {
            WriteUInt16(Position, Value.H0);
        }

        public void WriteVector32(long Position, AVec Value)
        {
            WriteUInt32(Position, Value.W0);
        }

        public void WriteVector64(long Position, AVec Value)
        {
            WriteUInt64(Position, Value.X0);
        }

        public void WriteVector128(long Position, AVec Value)
        {
            WriteUInt64(Position + 0, Value.X0);
            WriteUInt64(Position + 8, Value.X1);
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

        public void WriteVector8Unchecked(long Position, AVec Value)
        {
            WriteByteUnchecked(Position, Value.B0);
        }

        public void WriteVector16Unchecked(long Position, AVec Value)
        {
            WriteUInt16Unchecked(Position, Value.H0);
        }

        public void WriteVector32Unchecked(long Position, AVec Value)
        {
            WriteUInt32Unchecked(Position, Value.W0);
        }

        public void WriteVector64Unchecked(long Position, AVec Value)
        {
            WriteUInt64Unchecked(Position, Value.X0);
        }

        public void WriteVector128Unchecked(long Position, AVec Value)
        {
            WriteUInt64Unchecked(Position + 0, Value.X0);
            WriteUInt64Unchecked(Position + 8, Value.X1);
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
                Marshal.FreeHGlobal(Ram);

                Ram = IntPtr.Zero;
            }
        }
    }
}