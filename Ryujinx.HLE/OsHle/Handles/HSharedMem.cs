using ChocolArm64.Memory;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.OsHle.Handles
{
    class HSharedMem
    {
        private List<(AMemory, long, long)> Positions;

        public EventHandler<EventArgs> MemoryMapped;
        public EventHandler<EventArgs> MemoryUnmapped;

        public HSharedMem()
        {
            Positions = new List<(AMemory, long, long)>();
        }

        public void AddVirtualPosition(AMemory Memory, long Position, long Size)
        {
            lock (Positions)
            {
                Positions.Add((Memory, Position, Size));

                MemoryMapped?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveVirtualPosition(AMemory Memory, long Position, long Size)
        {
            lock (Positions)
            {
                Positions.Remove((Memory, Position, Size));

                MemoryUnmapped?.Invoke(this, EventArgs.Empty);
            }
        }

        public (AMemory, long, long)[] GetVirtualPositions()
        {
            return Positions.ToArray();
        }
    }
}