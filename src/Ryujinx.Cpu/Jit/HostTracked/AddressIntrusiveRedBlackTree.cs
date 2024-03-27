using Ryujinx.Common.Collections;
using System;

namespace Ryujinx.Cpu.Jit.HostTracked
{
    internal class AddressIntrusiveRedBlackTree<T> : IntrusiveRedBlackTree<T> where T : IntrusiveRedBlackTreeNode<T>, IComparable<T>, IComparable<ulong>
    {
        /// <summary>
        /// Retrieve the node that is considered equal to the specified address by the comparator.
        /// </summary>
        /// <param name="address">Address to compare with</param>
        /// <returns>Node that is equal to <paramref name="address"/></returns>
        public T GetNode(ulong address)
        {
            T node = Root;
            while (node != null)
            {
                int cmp = node.CompareTo(address);
                if (cmp < 0)
                {
                    node = node.Left;
                }
                else if (cmp > 0)
                {
                    node = node.Right;
                }
                else
                {
                    return node;
                }
            }
            return null;
        }
    }
}
