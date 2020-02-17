using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ARMeilleure.IntermediateRepresentation
{
    /// <summary>
    /// Represents a efficient linked list that stores the pointer on the object directly and does not allocate.
    /// </summary>
    /// <typeparam name="T">Type of the list items</typeparam>
    class IntrusiveList<T> where T : class, IIntrusiveListNode<T>
    {
        /// <summary>
        /// First item of the list, or null if empty.
        /// </summary>
        public T First { get; private set; }

        /// <summary>
        /// Last item of the list, or null if empty.
        /// </summary>
        public T Last { get; private set; }

        /// <summary>
        /// Total number of items on the list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Adds a item as the first item of the list.
        /// </summary>
        /// <param name="newNode">Item to be added</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFirst(T newNode)
        {
            if (First != null)
            {
                AddBefore(First, newNode);
            }
            else
            {
                Debug.Assert(newNode.ListPrevious == null);
                Debug.Assert(newNode.ListNext == null);
                Debug.Assert(Last == null);

                First = newNode;
                Last = newNode;

                Debug.Assert(Count == 0);

                Count = 1;
            }
        }

        /// <summary>
        /// Adds a item as the last item of the list.
        /// </summary>
        /// <param name="newNode">Item to be added</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLast(T newNode)
        {
            if (Last != null)
            {
                AddAfter(Last, newNode);
            }
            else
            {
                Debug.Assert(newNode.ListPrevious == null);
                Debug.Assert(newNode.ListNext == null);
                Debug.Assert(First == null);

                First = newNode;
                Last = newNode;

                Debug.Assert(Count == 0);

                Count = 1;
            }
        }

        /// <summary>
        /// Adds a item before a existing item on the list.
        /// </summary>
        /// <param name="node">Item on the list that will succeed the new item</param>
        /// <param name="newNode">Item to be added</param>
        /// <returns>New item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddBefore(T node, T newNode)
        {
            Debug.Assert(newNode.ListPrevious == null);
            Debug.Assert(newNode.ListNext == null);

            newNode.ListPrevious = node.ListPrevious;
            newNode.ListNext = node;

            node.ListPrevious = newNode;

            if (newNode.ListPrevious != null)
            {
                newNode.ListPrevious.ListNext = newNode;
            }

            if (First == node)
            {
                First = newNode;
            }

            Count++;

            return newNode;
        }

        /// <summary>
        /// Adds a item after a existing item on the list.
        /// </summary>
        /// <param name="node">Item on the list that will preceed the new item</param>
        /// <param name="newNode">Item to be added</param>
        /// <returns>New item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AddAfter(T node, T newNode)
        {
            Debug.Assert(newNode.ListPrevious == null);
            Debug.Assert(newNode.ListNext == null);

            newNode.ListPrevious = node;
            newNode.ListNext = node.ListNext;

            node.ListNext = newNode;

            if (newNode.ListNext != null)
            {
                newNode.ListNext.ListPrevious = newNode;
            }

            if (Last == node)
            {
                Last = newNode;
            }

            Count++;

            return newNode;
        }

        /// <summary>
        /// Removes a item from the list.
        /// </summary>
        /// <param name="node">The item to be removed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T node)
        {
            if (node.ListPrevious != null)
            {
                node.ListPrevious.ListNext = node.ListNext;
            }
            else
            {
                Debug.Assert(First == node);

                First = node.ListNext;
            }

            if (node.ListNext != null)
            {
                node.ListNext.ListPrevious = node.ListPrevious;
            }
            else
            {
                Debug.Assert(Last == node);

                Last = node.ListPrevious;
            }

            node.ListPrevious = null;
            node.ListNext = null;

            Count--;
        }
    }
}
