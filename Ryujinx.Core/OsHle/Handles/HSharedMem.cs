using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Handles
{
    class HSharedMem
    {
        private List<long> Positions;

        public EventHandler<EventArgs> MemoryMapped;
        public EventHandler<EventArgs> MemoryUnmapped;

        public HSharedMem()
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

        public long[] GetVirtualPositions()
        {
            return Positions.ToArray();
        }
    }
}