using ChocolArm64.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Handles
{
    class HSharedMem
    {
        private List<(AMemory, long)> Positions;

        public EventHandler<EventArgs> MemoryMapped;
        public EventHandler<EventArgs> MemoryUnmapped;

        public HSharedMem()
        {
            Positions = new List<(AMemory, long)>();
        }

        public void AddVirtualPosition(AMemory Memory, long Position)
        {
            lock (Positions)
            {
                Positions.Add((Memory, Position));

                MemoryMapped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveVirtualPosition(AMemory Memory, long Position)
        {
            lock (Positions)
            {
                Positions.Remove((Memory, Position));

                MemoryUnmapped?.Invoke(this, EventArgs.Empty);
            }
        }

        public (AMemory, long)[] GetVirtualPositions()
        {
            return Positions.ToArray();
        }
    }
}