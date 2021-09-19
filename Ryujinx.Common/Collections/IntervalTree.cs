using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Ryujinx.Common.Collections
{
    /// <summary>
    /// An Augmented Interval Tree based off of the "TreeDictionary"'s Red-Black Tree. Allows fast overlap checking of ranges.
    /// </summary>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    public class IntervalTree<K, V> where K : IComparable<K>
    {
        private const int ArrayGrowthSize = 32;

        private const bool Black = true;
        private const bool Red = false;
        private IntervalTreeNode<K, V> _root = null;
        private int _count = 0;

        public int Count => _count;

        public IntervalTree() { }

        #region Public Methods

        /// <summary>
        /// Gets the values of the interval whose key is <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key of the node value to get</param>
        /// <param name="overlaps">Overlaps array to place results in</param>
        /// <returns>Number of values found</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public int Get(K key, ref V[] overlaps)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IntervalTreeNode<K, V> node = GetNode(key);

            if (node == null)
            {
                return 0;
            }

            if (node.Values.Count > overlaps.Length)
            {
                Array.Resize(ref overlaps, node.Values.Count);
            }

            int overlapsCount = 0;
            foreach (RangeNode<K, V> value in node.Values)
            {
                overlaps[overlapsCount++] = value.Value;
            }

            return overlapsCount;
        }

        /// <summary>
        /// Returns the values of the intervals whose start and end keys overlap the given range.
        /// </summary>
        /// <param name="start">Start of the range</param>
        /// <param name="end">End of the range</param>
        /// <param name="overlaps">Overlaps array to place results in</param>
        /// <param name="overlapCount">Index to start writing results into the array. Defaults to 0</param>
        /// <returns>Number of values found</returns>
        /// <exception cref="ArgumentNullException"><paramref name="start"/> or <paramref name="end"/> is null</exception>
        public int Get(K start, K end, ref V[] overlaps, int overlapCount = 0)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            GetValues(_root, start, end, ref overlaps, ref overlapCount);

            return overlapCount;
        }

        /// <summary>
        /// Adds a new interval into the tree whose start is <paramref name="start"/>, end is <paramref name="end"/> and value is <paramref name="value"/>.
        /// </summary>
        /// <param name="start">Start of the range to add</param>
        /// <param name="end">End of the range to insert</param>
        /// <param name="value">Value to add</param>
        /// <exception cref="ArgumentNullException"><paramref name="start"/>, <paramref name="end"/> or <paramref name="value"/> are null</exception>
        public void Add(K start, K end, V value)
        {
            if (start == null)
            {
                throw new ArgumentNullException(nameof(start));
            }

            if (end == null)
            {
                throw new ArgumentNullException(nameof(end));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Insert(start, end, value);
        }

        /// <summary>
        /// Removes the given <paramref name="value"/> from the tree, searching for it with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key of the node to remove</param>
        /// <param name="value">Value to remove</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        /// <returns>Number of deleted values</returns>
        public int Remove(K key, V value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int removed = Delete(key, value);

            _count -= removed;

            return removed;
        }

        /// <summary>
        /// Adds all the nodes in the dictionary into <paramref name="list"/>.
        /// </summary>
        /// <returns>A list of all RangeNodes sorted by Key Order</returns>
        public List<RangeNode<K, V>> AsList()
        {
            List<RangeNode<K, V>> list = new List<RangeNode<K, V>>();

            AddToList(_root, list);

            return list;
        }

        #endregion

        #region Private Methods (BST)

        /// <summary>
        /// Adds all RangeNodes that are children of or contained within <paramref name="node"/> into <paramref name="list"/>, in Key Order.
        /// </summary>
        /// <param name="node">The node to search for RangeNodes within</param>
        /// <param name="list">The list to add RangeNodes to</param>
        private void AddToList(IntervalTreeNode<K, V> node, List<RangeNode<K, V>> list)
        {
            if (node == null)
            {
                return;
            }

            AddToList(node.Left, list);

            list.AddRange(node.Values);

            AddToList(node.Right, list);
        }

        /// <summary>
        /// Retrieve the node reference whose key is <paramref name="key"/>, or null if no such node exists.
        /// </summary>
        /// <param name="key">Key of the node to get</param>
        /// <returns>Node reference in the tree</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        private IntervalTreeNode<K, V> GetNode(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IntervalTreeNode<K, V> node = _root;
            while (node != null)
            {
                int cmp = key.CompareTo(node.Start);
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

        /// <summary>
        /// Retrieve all values that overlap the given start and end keys.
        /// </summary>
        /// <param name="start">Start of the range</param>
        /// <param name="end">End of the range</param>
        /// <param name="overlaps">Overlaps array to place results in</param>
        /// <param name="overlapCount">Overlaps count to update</param>
        private void GetValues(IntervalTreeNode<K, V> node, K start, K end, ref V[] overlaps, ref int overlapCount)
        {   
            if (node == null || start.CompareTo(node.Max) >= 0)
            {
                return;
            }

            GetValues(node.Left, start, end, ref overlaps, ref overlapCount);

            bool endsOnRight = end.CompareTo(node.Start) > 0;
            if (endsOnRight)
            {
                if (start.CompareTo(node.End) < 0)
                {
                    // Contains this node. Add overlaps to list.
                    foreach (RangeNode<K,V> overlap in node.Values)
                    {
                        if (start.CompareTo(overlap.End) < 0)
                        {
                            if (overlaps.Length >= overlapCount)
                            {
                                Array.Resize(ref overlaps, overlapCount + ArrayGrowthSize);
                            }

                            overlaps[overlapCount++] = overlap.Value;
                        }
                    }
                }

                GetValues(node.Right, start, end, ref overlaps, ref overlapCount);
            }
        }

        /// <summary>
        /// Inserts a new node into the tree with a given <paramref name="start"/>, <paramref name="end"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="start">Start of the range to insert</param>
        /// <param name="end">End of the range to insert</param>
        /// <param name="value">Value to insert</param>
        private void Insert(K start, K end, V value)
        {
            IntervalTreeNode<K, V> newNode = BSTInsert(start, end, value);
            RestoreBalanceAfterInsertion(newNode);
        }

        /// <summary>
        /// Propagate an increase in max value starting at the given node, heading up the tree.
        /// This should only be called if the max increases - not for rebalancing or removals.
        /// </summary>
        /// <param name="node">The node to start propagating from</param>
        private void PropagateIncrease(IntervalTreeNode<K, V> node)
        {
            K max = node.Max;
            IntervalTreeNode<K, V> ptr = node;

            while ((ptr = ptr.Parent) != null)
            {
                if (max.CompareTo(ptr.Max) > 0)
                {
                    ptr.Max = max;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Propagate recalculating max value starting at the given node, heading up the tree.
        /// This fully recalculates the max value from all children when there is potential for it to decrease.
        /// </summary>
        /// <param name="node">The node to start propagating from</param>
        private void PropagateFull(IntervalTreeNode<K, V> node)
        {
            IntervalTreeNode<K, V> ptr = node;

            do
            {
                K max = ptr.End;

                if (ptr.Left != null && ptr.Left.Max.CompareTo(max) > 0)
                {
                    max = ptr.Left.Max;
                }

                if (ptr.Right != null && ptr.Right.Max.CompareTo(max) > 0)
                {
                    max = ptr.Right.Max;
                }

                ptr.Max = max;
            } while ((ptr = ptr.Parent) != null);
        }

        /// <summary>
        /// Insertion Mechanism for the interval tree. Similar to a BST insert, with the start of the range as the key.
        /// Iterates the tree starting from the root and inserts a new node where all children in the left subtree are less than <paramref name="start"/>, and all children in the right subtree are greater than <paramref name="start"/>.
        /// Each node can contain multiple values, and has an end address which is the maximum of all those values.
        /// Post insertion, the "max" value of the node and all parents are updated.
        /// </summary>
        /// <param name="start">Start of the range to insert</param>
        /// <param name="end">End of the range to insert</param>
        /// <param name="value">Value to insert</param>
        /// <returns>The inserted Node</returns>
        private IntervalTreeNode<K, V> BSTInsert(K start, K end, V value)
        {
            IntervalTreeNode<K, V> parent = null;
            IntervalTreeNode<K, V> node = _root;

            while (node != null)
            {
                parent = node;
                int cmp = start.CompareTo(node.Start);
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
                    node.Values.Add(new RangeNode<K, V>(start, end, value));

                    if (end.CompareTo(node.End) > 0)
                    {
                        node.End = end;
                        if (end.CompareTo(node.Max) > 0)
                        {
                            node.Max = end;
                            PropagateIncrease(node);
                        }
                    }

                    _count++;
                    return node;
                }
            }
            IntervalTreeNode<K, V> newNode = new IntervalTreeNode<K, V>(start, end, value, parent);
            if (newNode.Parent == null)
            {
                _root = newNode;
            }
            else if (start.CompareTo(parent.Start) < 0)
            {
                parent.Left = newNode;
            }
            else
            {
                parent.Right = newNode;
            }

            PropagateIncrease(newNode);
            _count++;
            return newNode;
        }

        /// <summary>
        /// Removes instances of <paramref name="value"> from the dictionary after searching for it with <paramref name="key">.
        /// </summary>
        /// <param name="key">Key to search for</param>
        /// <param name="value">Value to delete</param>
        /// <returns>Number of deleted values</returns>
        private int Delete(K key, V value)
        {
            IntervalTreeNode<K, V> nodeToDelete = GetNode(key);

            if (nodeToDelete == null)
            {
                return 0;
            }

            int removed = nodeToDelete.Values.RemoveAll(node => node.Value.Equals(value));

            if (nodeToDelete.Values.Count > 0)
            {
                if (removed > 0)
                {
                    nodeToDelete.End = nodeToDelete.Values.Max(node => node.End);

                    // Recalculate max from children and new end.
                    PropagateFull(nodeToDelete);
                }

                return removed;
            }

            IntervalTreeNode<K, V> replacementNode;

            if (LeftOf(nodeToDelete) == null || RightOf(nodeToDelete) == null)
            {
                replacementNode = nodeToDelete;
            }
            else
            {
                replacementNode = PredecessorOf(nodeToDelete);
            }

            IntervalTreeNode<K, V> tmp = LeftOf(replacementNode) ?? RightOf(replacementNode);

            if (tmp != null)
            {
                tmp.Parent = ParentOf(replacementNode);
            }

            if (ParentOf(replacementNode) == null)
            {
                _root = tmp;
            }
            else if (replacementNode == LeftOf(ParentOf(replacementNode)))
            {
                ParentOf(replacementNode).Left = tmp;
            }
            else
            {
                ParentOf(replacementNode).Right = tmp;
            }

            if (replacementNode != nodeToDelete)
            {
                nodeToDelete.Start = replacementNode.Start;
                nodeToDelete.Values = replacementNode.Values;
                nodeToDelete.End = replacementNode.End;
                nodeToDelete.Max = replacementNode.Max;
            }

            PropagateFull(replacementNode);

            if (tmp != null && ColorOf(replacementNode) == Black)
            {
                RestoreBalanceAfterRemoval(tmp);
            }

            return removed;
        }

        /// <summary>
        /// Returns the node with the largest key where <paramref name="node"/> is considered the root node.
        /// </summary>
        /// <param name="node">Root Node</param>
        /// <returns>Node with the maximum key in the tree of <paramref name="node"/></returns>
        private static IntervalTreeNode<K, V> Maximum(IntervalTreeNode<K, V> node)
        {
            IntervalTreeNode<K, V> tmp = node;
            while (tmp.Right != null)
            {
                tmp = tmp.Right;
            }

            return tmp;
        }

        /// <summary>
        /// Finds the node whose key is immediately less than <paramref name="node"/>.
        /// </summary>
        /// <param name="node">Node to find the predecessor of</param>
        /// <returns>Predecessor of <paramref name="node"/></returns>
        private static IntervalTreeNode<K, V> PredecessorOf(IntervalTreeNode<K, V> node)
        {
            if (node.Left != null)
            {
                return Maximum(node.Left);
            }
            IntervalTreeNode<K, V> parent = node.Parent;
            while (parent != null && node == parent.Left)
            {
                node = parent;
                parent = parent.Parent;
            }
            return parent;
        }
        #endregion

        #region Private Methods (RBL)
        private void RestoreBalanceAfterRemoval(IntervalTreeNode<K, V> balanceNode)
        {
            IntervalTreeNode<K, V> ptr = balanceNode;

            while (ptr != _root && ColorOf(ptr) == Black)
            {
                if (ptr == LeftOf(ParentOf(ptr)))
                {
                    IntervalTreeNode<K, V> sibling = RightOf(ParentOf(ptr));

                    if (ColorOf(sibling) == Red)
                    {
                        SetColor(sibling, Black);
                        SetColor(ParentOf(ptr), Red);
                        RotateLeft(ParentOf(ptr));
                        sibling = RightOf(ParentOf(ptr));
                    }
                    if (ColorOf(LeftOf(sibling)) == Black && ColorOf(RightOf(sibling)) == Black)
                    {
                        SetColor(sibling, Red);
                        ptr = ParentOf(ptr);
                    }
                    else
                    {
                        if (ColorOf(RightOf(sibling)) == Black)
                        {
                            SetColor(LeftOf(sibling), Black);
                            SetColor(sibling, Red);
                            RotateRight(sibling);
                            sibling = RightOf(ParentOf(ptr));
                        }
                        SetColor(sibling, ColorOf(ParentOf(ptr)));
                        SetColor(ParentOf(ptr), Black);
                        SetColor(RightOf(sibling), Black);
                        RotateLeft(ParentOf(ptr));
                        ptr = _root;
                    }
                }
                else
                {
                    IntervalTreeNode<K, V> sibling = LeftOf(ParentOf(ptr));

                    if (ColorOf(sibling) == Red)
                    {
                        SetColor(sibling, Black);
                        SetColor(ParentOf(ptr), Red);
                        RotateRight(ParentOf(ptr));
                        sibling = LeftOf(ParentOf(ptr));
                    }
                    if (ColorOf(RightOf(sibling)) == Black && ColorOf(LeftOf(sibling)) == Black)
                    {
                        SetColor(sibling, Red);
                        ptr = ParentOf(ptr);
                    }
                    else
                    {
                        if (ColorOf(LeftOf(sibling)) == Black)
                        {
                            SetColor(RightOf(sibling), Black);
                            SetColor(sibling, Red);
                            RotateLeft(sibling);
                            sibling = LeftOf(ParentOf(ptr));
                        }
                        SetColor(sibling, ColorOf(ParentOf(ptr)));
                        SetColor(ParentOf(ptr), Black);
                        SetColor(LeftOf(sibling), Black);
                        RotateRight(ParentOf(ptr));
                        ptr = _root;
                    }
                }
            }
            SetColor(ptr, Black);
        }

        private void RestoreBalanceAfterInsertion(IntervalTreeNode<K, V> balanceNode)
        {
            SetColor(balanceNode, Red);
            while (balanceNode != null && balanceNode != _root && ColorOf(ParentOf(balanceNode)) == Red)
            {
                if (ParentOf(balanceNode) == LeftOf(ParentOf(ParentOf(balanceNode))))
                {
                    IntervalTreeNode<K, V> sibling = RightOf(ParentOf(ParentOf(balanceNode)));

                    if (ColorOf(sibling) == Red)
                    {
                        SetColor(ParentOf(balanceNode), Black);
                        SetColor(sibling, Black);
                        SetColor(ParentOf(ParentOf(balanceNode)), Red);
                        balanceNode = ParentOf(ParentOf(balanceNode));
                    }
                    else
                    {
                        if (balanceNode == RightOf(ParentOf(balanceNode)))
                        {
                            balanceNode = ParentOf(balanceNode);
                            RotateLeft(balanceNode);
                        }
                        SetColor(ParentOf(balanceNode), Black);
                        SetColor(ParentOf(ParentOf(balanceNode)), Red);
                        RotateRight(ParentOf(ParentOf(balanceNode)));
                    }
                }
                else
                {
                    IntervalTreeNode<K, V> sibling = LeftOf(ParentOf(ParentOf(balanceNode)));

                    if (ColorOf(sibling) == Red)
                    {
                        SetColor(ParentOf(balanceNode), Black);
                        SetColor(sibling, Black);
                        SetColor(ParentOf(ParentOf(balanceNode)), Red);
                        balanceNode = ParentOf(ParentOf(balanceNode));
                    }
                    else
                    {
                        if (balanceNode == LeftOf(ParentOf(balanceNode)))
                        {
                            balanceNode = ParentOf(balanceNode);
                            RotateRight(balanceNode);
                        }
                        SetColor(ParentOf(balanceNode), Black);
                        SetColor(ParentOf(ParentOf(balanceNode)), Red);
                        RotateLeft(ParentOf(ParentOf(balanceNode)));
                    }
                }
            }
            SetColor(_root, Black);
        }

        private void RotateLeft(IntervalTreeNode<K, V> node)
        {
            if (node != null)
            {
                IntervalTreeNode<K, V> right = RightOf(node);
                node.Right = LeftOf(right);
                if (node.Right != null)
                {
                    node.Right.Parent = node; 
                }
                IntervalTreeNode<K, V> nodeParent = ParentOf(node);
                right.Parent = nodeParent;
                if (nodeParent == null)
                {
                    _root = right;
                }
                else if (node == LeftOf(nodeParent))
                {
                    nodeParent.Left = right;
                }
                else
                {
                    nodeParent.Right = right; 
                }
                right.Left = node;
                node.Parent = right;

                PropagateFull(node);
            }
        }

        private void RotateRight(IntervalTreeNode<K, V> node)
        {
            if (node != null)
            {
                IntervalTreeNode<K, V> left = LeftOf(node);
                node.Left = RightOf(left);
                if (node.Left != null)
                {
                    node.Left.Parent = node;
                }
                IntervalTreeNode<K, V> nodeParent = ParentOf(node);
                left.Parent = nodeParent;
                if (nodeParent == null)
                {
                    _root = left;
                }
                else if (node == RightOf(nodeParent))
                {
                    nodeParent.Right = left;
                }
                else
                {
                    nodeParent.Left = left;
                }
                left.Right = node;
                node.Parent = left;

                PropagateFull(node);
            }
        }
        #endregion

        #region Safety-Methods

        // These methods save memory by allowing us to forego sentinel nil nodes, as well as serve as protection against NullReferenceExceptions.

        /// <summary>
        /// Returns the color of <paramref name="node"/>, or Black if it is null.
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>The boolean color of <paramref name="node"/>, or black if null</returns>
        private static bool ColorOf(IntervalTreeNode<K, V> node)
        {
            return node == null || node.Color;
        }

        /// <summary>
        /// Sets the color of <paramref name="node"/> node to <paramref name="color"/>.
        /// <br></br>
        /// This method does nothing if <paramref name="node"/> is null.
        /// </summary>
        /// <param name="node">Node to set the color of</param>
        /// <param name="color">Color (Boolean)</param>
        private static void SetColor(IntervalTreeNode<K, V> node, bool color)
        {
            if (node != null)
            {
                node.Color = color;
            }
        }

        /// <summary>
        /// This method returns the left node of <paramref name="node"/>, or null if <paramref name="node"/> is null.
        /// </summary>
        /// <param name="node">Node to retrieve the left child from</param>
        /// <returns>Left child of <paramref name="node"/></returns>
        private static IntervalTreeNode<K, V> LeftOf(IntervalTreeNode<K, V> node)
        {
            return node?.Left;
        }

        /// <summary>
        /// This method returns the right node of <paramref name="node"/>, or null if <paramref name="node"/> is null.
        /// </summary>
        /// <param name="node">Node to retrieve the right child from</param>
        /// <returns>Right child of <paramref name="node"/></returns>
        private static IntervalTreeNode<K, V> RightOf(IntervalTreeNode<K, V> node)
        {
            return node?.Right;
        }

        /// <summary>
        /// Returns the parent node of <paramref name="node"/>, or null if <paramref name="node"/> is null.
        /// </summary>
        /// <param name="node">Node to retrieve the parent from</param>
        /// <returns>Parent of <paramref name="node"/></returns>
        private static IntervalTreeNode<K, V> ParentOf(IntervalTreeNode<K, V> node)
        {
            return node?.Parent;
        }
        #endregion

        public bool ContainsKey(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return GetNode(key) != null;
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
        }
    }

    /// <summary>
    /// Represents a value and its start and end keys.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public readonly struct RangeNode<K, V>
    {
        public readonly K Start;
        public readonly K End;
        public readonly V Value;

        public RangeNode(K start, K end, V value)
        {
            Start = start;
            End = end;
            Value = value;
        }
    }

    /// <summary>
    /// Represents a node in the IntervalTree which contains start and end keys of type K, and a value of generic type V.
    /// </summary>
    /// <typeparam name="K">Key type of the node</typeparam>
    /// <typeparam name="V">Value type of the node</typeparam>
    internal class IntervalTreeNode<K, V>
    {
        internal bool Color = true;
        internal IntervalTreeNode<K, V> Left = null;
        internal IntervalTreeNode<K, V> Right = null;
        internal IntervalTreeNode<K, V> Parent = null;

        /// <summary>
        /// The start of the range.
        /// </summary>
        internal K Start;

        /// <summary>
        /// The end of the range - maximum of all in the Values list.
        /// </summary>
        internal K End;

        /// <summary>
        /// The maximum end value of this node and all its children.
        /// </summary>
        internal K Max;

        internal List<RangeNode<K, V>> Values;

        public IntervalTreeNode(K start, K end, V value, IntervalTreeNode<K, V> parent)
        {
            this.Start = start;
            this.End = end;
            this.Max = end;
            this.Values = new List<RangeNode<K, V>> { new RangeNode<K, V>(start, end, value) };
            this.Parent = parent;
        }
    }
}
