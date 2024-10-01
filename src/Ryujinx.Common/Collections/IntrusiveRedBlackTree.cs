using System;

namespace Ryujinx.Common.Collections
{
    /// <summary>
    /// Tree that provides the ability for O(logN) lookups for keys that exist in the tree, and O(logN) lookups for keys immediately greater than or less than a specified key.
    /// </summary>
    /// <typeparam name="T">Derived node type</typeparam>
    public class IntrusiveRedBlackTree<T> : IntrusiveRedBlackTreeImpl<T> where T : IntrusiveRedBlackTreeNode<T>, IComparable<T>
    {
        #region Public Methods

        /// <summary>
        /// Adds a new node into the tree.
        /// </summary>
        /// <param name="node">Node to be added</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null</exception>
        public void Add(T node)
        {
            ArgumentNullException.ThrowIfNull(node);

            Insert(node);
        }

        /// <summary>
        /// Removes a node from the tree.
        /// </summary>
        /// <param name="node">Note to be removed</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null</exception>
        public void Remove(T node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (Delete(node) != null)
            {
                Count--;
            }
        }

        /// <summary>
        /// Retrieve the node that is considered equal to the specified node by the comparator.
        /// </summary>
        /// <param name="searchNode">Node to compare with</param>
        /// <returns>Node that is equal to <paramref name="searchNode"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="searchNode"/> is null</exception>
        public T GetNode(T searchNode)
        {
            ArgumentNullException.ThrowIfNull(searchNode);

            T node = Root;
            while (node != null)
            {
                int cmp = searchNode.CompareTo(node);
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

        #endregion

        #region Private Methods (BST)

        /// <summary>
        /// Inserts a new node into the tree.
        /// </summary>
        /// <param name="node">Node to be inserted</param>
        private void Insert(T node)
        {
            T newNode = BSTInsert(node);
            RestoreBalanceAfterInsertion(newNode);
        }

        /// <summary>
        /// Insertion Mechanism for a Binary Search Tree (BST).
        /// <br></br>
        /// Iterates the tree starting from the root and inserts a new node
        /// where all children in the left subtree are less than <paramref name="newNode"/>,
        /// and all children in the right subtree are greater than <paramref name="newNode"/>.
        /// </summary>
        /// <param name="newNode">Node to be inserted</param>
        /// <returns>The inserted Node</returns>
        private T BSTInsert(T newNode)
        {
            T parent = null;
            T node = Root;

            while (node != null)
            {
                parent = node;
                int cmp = newNode.CompareTo(node);
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
            newNode.Parent = parent;
            if (parent == null)
            {
                Root = newNode;
            }
            else if (newNode.CompareTo(parent) < 0)
            {
                parent.Left = newNode;
            }
            else
            {
                parent.Right = newNode;
            }
            Count++;
            return newNode;
        }

        /// <summary>
        /// Removes <paramref name="nodeToDelete"/> from the tree, if it exists.
        /// </summary>
        /// <param name="nodeToDelete">Node to be removed</param>
        /// <returns>The deleted Node</returns>
        private T Delete(T nodeToDelete)
        {
            if (nodeToDelete == null)
            {
                return null;
            }

            T old = nodeToDelete;
            T child;
            T parent;
            bool color;

            if (LeftOf(nodeToDelete) == null)
            {
                child = RightOf(nodeToDelete);
            }
            else if (RightOf(nodeToDelete) == null)
            {
                child = LeftOf(nodeToDelete);
            }
            else
            {
                T element = Minimum(RightOf(nodeToDelete));

                child = RightOf(element);
                parent = ParentOf(element);
                color = ColorOf(element);

                if (child != null)
                {
                    child.Parent = parent;
                }

                if (parent == null)
                {
                    Root = child;
                }
                else if (element == LeftOf(parent))
                {
                    parent.Left = child;
                }
                else
                {
                    parent.Right = child;
                }

                element.Color = old.Color;
                element.Left = old.Left;
                element.Right = old.Right;
                element.Parent = old.Parent;

                if (ParentOf(old) == null)
                {
                    Root = element;
                }
                else if (old == LeftOf(ParentOf(old)))
                {
                    ParentOf(old).Left = element;
                }
                else
                {
                    ParentOf(old).Right = element;
                }

                LeftOf(old).Parent = element;

                if (RightOf(old) != null)
                {
                    RightOf(old).Parent = element;
                }

                if (child != null && color == Black)
                {
                    RestoreBalanceAfterRemoval(child);
                }

                return old;
            }

            parent = ParentOf(nodeToDelete);
            color = ColorOf(nodeToDelete);

            if (child != null)
            {
                child.Parent = parent;
            }

            if (parent == null)
            {
                Root = child;
            }
            else if (nodeToDelete == LeftOf(parent))
            {
                parent.Left = child;
            }
            else
            {
                parent.Right = child;
            }

            if (child != null && color == Black)
            {
                RestoreBalanceAfterRemoval(child);
            }

            return old;
        }

        #endregion
    }

    public static class IntrusiveRedBlackTreeExtensions
    {
        /// <summary>
        /// Retrieve the node that is considered equal to the key by the comparator.
        /// </summary>
        /// <param name="tree">Tree to search at</param>
        /// <param name="key">Key of the node to be found</param>
        /// <returns>Node that is equal to <paramref name="key"/></returns>
        public static TNode GetNodeByKey<TNode, TKey>(this IntrusiveRedBlackTree<TNode> tree, TKey key)
            where TNode : IntrusiveRedBlackTreeNode<TNode>, IComparable<TNode>, IComparable<TKey>
            where TKey : struct
        {
            TNode node = tree.RootNode;
            while (node != null)
            {
                int cmp = node.CompareTo(key);
                if (cmp < 0)
                {
                    node = node.Right;
                }
                else if (cmp > 0)
                {
                    node = node.Left;
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
