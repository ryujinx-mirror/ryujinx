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

                    if (Rg.Size == 0)
                    {
                        //Region is empty, just remove it.
                        FreeRegions.Remove(Node);
                    }
                    else if (Node.Previous != null)
                    {
                        //Re-sort based on size (smaller first).
                        Node = Node.Previous;

                        FreeRegions.Remove(Node.Next);

                        while (Node != null && (ulong)Node.Value.Size > (ulong)Rg.Size)
                        {
                            Node = Node.Previous;
                        }

                        if (Node != null)
                        {
                            FreeRegions.AddAfter(Node, Rg);
                        }
                        else
                        {
                            FreeRegions.AddFirst(Rg);
                        }
                    }

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
            LinkedListNode<Region> PrevSz = null;

            while (Node != null)
            {
                LinkedListNode<Region> NextNode = Node.Next;

                Region Rg = Node.Value;

                long RgEnd = Rg.Position + Rg.Size;

                if (Rg.Position == End)
                {
                    //Current region position matches the end of the freed region,
                    //just merge the two and remove the current region from the list.
                    NewRg.Size += Rg.Size;

                    FreeRegions.Remove(Node);
                }
                else if (RgEnd == Position)
                {
                    //End of the current region matches the position of the freed region,
                    //just merge the two and remove the current region from the list.
                    NewRg.Position  = Rg.Position;
                    NewRg.Size     += Rg.Size;

                    FreeRegions.Remove(Node);
                }
                else
                {
                    if (PrevSz == null)
                    {
                        PrevSz = Node;
                    }
                    else if ((ulong)Rg.Size < (ulong)NewRg.Size &&
                             (ulong)Rg.Size > (ulong)PrevSz.Value.Size)
                    {
                        PrevSz = Node;
                    }
                }

                Node = NextNode;
            }

            if (PrevSz != null && (ulong)PrevSz.Value.Size < (ulong)Size)
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