using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KPageList : IEnumerable<KPageNode>
    {
        public LinkedList<KPageNode> Nodes { get; private set; }

        public KPageList()
        {
            Nodes = new LinkedList<KPageNode>();
        }

        public KernelResult AddRange(ulong Address, ulong PagesCount)
        {
            if (PagesCount != 0)
            {
                if (Nodes.Last != null)
                {
                    KPageNode LastNode = Nodes.Last.Value;

                    if (LastNode.Address + LastNode.PagesCount * KMemoryManager.PageSize == Address)
                    {
                        Address     = LastNode.Address;
                        PagesCount += LastNode.PagesCount;

                        Nodes.RemoveLast();
                    }
                }

                Nodes.AddLast(new KPageNode(Address, PagesCount));
            }

            return KernelResult.Success;
        }

        public ulong GetPagesCount()
        {
            ulong Sum = 0;

            foreach (KPageNode Node in Nodes)
            {
                Sum += Node.PagesCount;
            }

            return Sum;
        }

        public bool IsEqual(KPageList Other)
        {
            LinkedListNode<KPageNode> ThisNode  = Nodes.First;
            LinkedListNode<KPageNode> OtherNode = Other.Nodes.First;

            while (ThisNode != null && OtherNode != null)
            {
                if (ThisNode.Value.Address    != OtherNode.Value.Address ||
                    ThisNode.Value.PagesCount != OtherNode.Value.PagesCount)
                {
                    return false;
                }

                ThisNode  = ThisNode.Next;
                OtherNode = OtherNode.Next;
            }

            return ThisNode == null && OtherNode == null;
        }

        public IEnumerator<KPageNode> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}