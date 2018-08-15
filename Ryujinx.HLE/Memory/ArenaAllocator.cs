using System.Collections.Generic;

namespace Ryujinx.HLE.Memory
{
    class ArenaAllocator
    {
        private class Region
        {
            public long Position { get; set; }
            public long Size     { get; set; }

            public Region(long Position, long Size)
            {
                this.Position = Position;
                this.Size     = Size;
            }
        }

        private LinkedList<Region> FreeRegions;

        public long TotalAvailableSize { get; private set; }
        public long TotalUsedSize      { get; private set; }

        public ArenaAllocator(long ArenaSize)
        {
            TotalAvailableSize = ArenaSize;

            FreeRegions = new LinkedList<Region>();

            FreeRegions.AddFirst(new Region(0, ArenaSize));
        }

        public bool TryAllocate(long Size, out long Position)
        {
            LinkedListNode<Region> Node = FreeRegions.First;

            while (Node != null)
            {
                Region Rg = Node.Value;

                if ((ulong)Rg.Size >= (ulong)Size)
                {
                    Position = Rg.Position;

                    Rg.Position += Size;
                    Rg.Size     -= Size;

                    TotalUsedSize += Size;

                    return true;
                }

                Node = Node.Next;
            }

            Position = 0;

            return false;
        }

        public void Free(long Position, long Size)
        {
            long End = Position + Size;

            Region NewRg = new Region(Position, Size);

            LinkedListNode<Region> Node   = FreeRegions.First;
            LinkedListNode<Region> PrevSz = Node;

            while (Node != null)
            {
                LinkedListNode<Region> NextNode = Node.Next;

                Region Rg = Node.Value;

                long RgEnd = Rg.Position + Rg.Size;

                if (Rg.Position == End)
                {
                    NewRg.Size += Rg.Size;

                    FreeRegions.Remove(Node);
                }
                else if (RgEnd == Position)
                {
                    NewRg.Position  = Rg.Position;
                    NewRg.Size     += Rg.Size;

                    FreeRegions.Remove(Node);
                }
                else if ((ulong)Rg.Size < (ulong)NewRg.Size &&
                         (ulong)Rg.Size > (ulong)PrevSz.Value.Size)
                {
                    PrevSz = Node;
                }

                Node = NextNode;
            }

            if ((ulong)PrevSz.Value.Size < (ulong)Size)
            {
                FreeRegions.AddAfter(PrevSz, NewRg);
            }
            else
            {
                FreeRegions.AddFirst(NewRg);
            }

            TotalUsedSize -= Size;
        }
    }
}