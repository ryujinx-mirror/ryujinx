using System;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Handles
{
    class HSharedMem
    {
        private List<long> Positions;

        public int PositionsCount => Positions.Count;

        public EventHandler<EventArgs> MemoryMapped;
        public EventHandler<EventArgs> MemoryUnmapped;

        public HSharedMem(long PhysPos)
        {
            Positions = new List<long>();
        }

        public void AddVirtualPosition(long Position)
        {
            lock (Positions)
            {
                Positions.Add(Position);

                MemoryMapped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveVirtualPosition(long Position)
        {
            lock (Positions)
            {
                Positions.Remove(Position);

                MemoryUnmapped?.Invoke(this, EventArgs.Empty);
            }
        }

        public long GetVirtualPosition(int Index)
        {
            lock (Positions)
            {
                if (Index < 0 || Index >= Positions.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(Index));
                }

                return Positions[Index];
            }
        }

        public bool TryGetLastVirtualPosition(out long Position)
        {
            lock (Positions)
            {
                if (Positions.Count > 0)
                {
                    Position = Positions[Positions.Count - 1];

                    return true;
                }

                Position = 0;

                return false;
            }
        }
    }
}