using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ryujinx.Common.Collections
{
    /// <summary>
    /// Dictionary that provides the ability for O(logN) Lookups for keys that exist in the Dictionary, and O(logN) lookups for keys immediately greater than or less than a specified key.
    /// </summary>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    public class TreeDictionary<K, V> : IntrusiveRedBlackTreeImpl<Node<K, V>>, IDictionary<K, V> where K : IComparable<K>
    {
        #region Public Methods

        /// <summary>
        /// Returns the value of the node whose key is <paramref name="key"/>, or the default value if no such node exists.
        /// </summary>
        /// <param name="key">Key of the node value to get</param>
        /// <returns>Value associated w/ <paramref name="key"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public V Get(K key)
        {
            ArgumentNullException.ThrowIfNull(key);

            Node<K, V> node = GetNode(key);

            if (node == null)
            {
                return default;
            }

            return node.Value;
        }

        /// <summary>
        /// Adds a new node into the tree whose key is <paramref name="key"/> key and value is <paramref name="value"/>.
        /// <br></br>
        /// <b>Note:</b> Adding the same key multiple times will cause the value for that key to be overwritten.
        /// </summary>
        /// <param name="key">Key of the node to add</param>
        /// <param name="value">Value of the node to add</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> are null</exception>
        public void Add(K key, V value)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);

            Insert(key, value);
        }

        /// <summary>
        /// Removes the node whose key is <paramref name="key"/> from the tree.
        /// </summary>
        /// <param name="key">Key of the node to remove</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public void Remove(K key)
        {
            ArgumentNullException.ThrowIfNull(key);

            if (Delete(key) != null)
            {
                Count--;
            }
        }

        /// <summary>
        /// Returns the value whose key is equal to or immediately less than <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key for which to find the floor value of</param>
        /// <returns>Key of node immediately less than <paramref name="key"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public K Floor(K key)
        {
            Node<K, V> node = FloorNode(key);
            if (node != null)
            {
                return node.Key;
            }
            return default;
        }

        /// <summary>
        /// Returns the node whose key is equal to or immediately greater than <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key for which to find the ceiling node of</param>
        /// <returns>Key of node immediately greater than <paramref name="key"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public K Ceiling(K key)
        {
            Node<K, V> node = CeilingNode(key);
            if (node != null)
            {
                return node.Key;
            }
            return default;
        }

        /// <summary>
        /// Finds the value whose key is immediately greater than <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to find the successor of</param>
        /// <returns>Value</returns>
        public K SuccessorOf(K key)
        {
            Node<K, V> node = GetNode(key);
            if (node != null)
            {
                Node<K, V> successor =  SuccessorOf(node);

                return successor != null ? successor.Key : default;
            }
            return default;
        }

        /// <summary>
        /// Finds the value whose key is immediately less than <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to find the predecessor of</param>
        /// <returns>Value</returns>
        public K PredecessorOf(K key)
        {
            Node<K, V> node = GetNode(key);
            if (node != null)
            {
                Node<K, V> predecessor = PredecessorOf(node);

                return predecessor != null ? predecessor.Key : default;
            }
            return default;
        }

        /// <summary>
        /// Adds all the nodes in the dictionary as key/value pairs into <paramref name="list"/>.
        /// <br></br>
        /// The key/value pairs will be added in Level Order.
        /// </summary>
        /// <param name="list">List to add the tree pairs into</param>
        public List<KeyValuePair<K, V>> AsLevelOrderList()
        {
            List<KeyValuePair<K, V>> list = new List<KeyValuePair<K, V>>();

            Queue<Node<K, V>> nodes = new Queue<Node<K, V>>();

            if (this.Root != null)
            {
                nodes.Enqueue(this.Root);
            }
            while (nodes.TryDequeue(out Node<K, V> node))
            {
                list.Add(new KeyValuePair<K, V>(node.Key, node.Value));
                if (node.Left != null)
                {
                    nodes.Enqueue(node.Left);
                }
                if (node.Right != null)
                {
                    nodes.Enqueue(node.Right);
                }
            }
            return list;
        }

        /// <summary>
        /// Adds all the nodes in the dictionary into <paramref name="list"/>.
        /// </summary>
        /// <returns>A list of all KeyValuePairs sorted by Key Order</returns>
        public List<KeyValuePair<K, V>> AsList()
        {
            List<KeyValuePair<K, V>> list = new List<KeyValuePair<K, V>>();

            AddToList(Root, list);

            return list;
        }

        #endregion

        #region Private Methods (BST)

        /// <summary>
        /// Adds all nodes that are children of or contained within <paramref name="node"/> into <paramref name="list"/>, in Key Order.
        /// </summary>
        /// <param name="node">The node to search for nodes within</param>
        /// <param name="list">The list to add node to</param>
        private void AddToList(Node<K, V> node, List<KeyValuePair<K, V>> list)
        {
            if (node == null)
            {
                return;
            }

            AddToList(node.Left, list);

            list.Add(new KeyValuePair<K, V>(node.Key, node.Value));

            AddToList(node.Right, list);
        }

        /// <summary>
        /// Retrieve the node reference whose key is <paramref name="key"/>, or null if no such node exists.
        /// </summary>
        /// <param name="key">Key of the node to get</param>
        /// <returns>Node reference in the tree</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        private Node<K, V> GetNode(K key)
        {
            ArgumentNullException.ThrowIfNull(key);

            Node<K, V> node = Root;
            while (node != null)
            {
                int cmp = key.CompareTo(node.Key);
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
        /// Inserts a new node into the tree whose key is <paramref name="key"/> and value is <paramref name="value"/>.
        /// <br></br>
        /// Adding the same key multiple times will overwrite the previous value.
        /// </summary>
        /// <param name="key">Key of the node to insert</param>
        /// <param name="value">Value of the node to insert</param>
        private void Insert(K key, V value)
        {
            Node<K, V> newNode = BSTInsert(key, value);
            RestoreBalanceAfterInsertion(newNode);
        }

        /// <summary>
        /// Insertion Mechanism for a Binary Search Tree (BST).
        /// <br></br>
        /// Iterates the tree starting from the root and inserts a new node where all children in the left subtree are less than <paramref name="key"/>, and all children in the right subtree are greater than <paramref name="key"/>.
        /// <br></br>
        /// <b>Note: </b> If a node whose key is <paramref name="key"/> already exists, it's value will be overwritten.
        /// </summary>
        /// <param name="key">Key of the node to insert</param>
        /// <param name="value">Value of the node to insert</param>
        /// <returns>The inserted Node</returns>
        private Node<K, V> BSTInsert(K key, V value)
        {
            Node<K, V> parent = null;
            Node<K, V> node = Root;

            while (node != null)
            {
                parent = node;
                int cmp = key.CompareTo(node.Key);
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
                    node.Value = value;
                    return node;
                }
            }
            Node<K, V> newNode = new Node<K, V>(key, value, parent);
            if (newNode.Parent == null)
            {
                Root = newNode;
            }
            else if (key.CompareTo(parent.Key) < 0)
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
        /// Removes <paramref name="key"/> from the dictionary, if it exists.
        /// </summary>
        /// <param name="key">Key of the node to delete</param>
        /// <returns>The deleted Node</returns>
        private Node<K, V> Delete(K key)
        {
            // O(1) Retrieval
            Node<K, V> nodeToDelete = GetNode(key);

            if (nodeToDelete == null) return null;

            Node<K, V> replacementNode;

            if (LeftOf(nodeToDelete) == null || RightOf(nodeToDelete) == null)
            {
                replacementNode = nodeToDelete;
            }
            else
            {
                replacementNode = PredecessorOf(nodeToDelete);
            }

            Node<K, V> tmp = LeftOf(replacementNode) ?? RightOf(replacementNode);

            if (tmp != null)
            {
                tmp.Parent = ParentOf(replacementNode);
            }

            if (ParentOf(replacementNode) == null)
            {
                Root = tmp;
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
                nodeToDelete.Key = replacementNode.Key;
                nodeToDelete.Value = replacementNode.Value;
            }

            if (tmp != null && ColorOf(replacementNode) == Black)
            {
                RestoreBalanceAfterRemoval(tmp);
            }

            return replacementNode;
        }

        /// <summary>
        /// Returns the node whose key immediately less than or equal to <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key for which to find the floor node of</param>
        /// <returns>Node whose key is immediately less than or equal to <paramref name="key"/>, or null if no such node is found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        private Node<K, V> FloorNode(K key)
        {
            ArgumentNullException.ThrowIfNull(key);

            Node<K, V> tmp = Root;

            while (tmp != null)
            {
                int cmp = key.CompareTo(tmp.Key);
                if (cmp > 0)
                {
                    if (tmp.Right != null)
                    {
                        tmp = tmp.Right;
                    }
                    else
                    {
                        return tmp;
                    }
                }
                else if (cmp < 0)
                {
                    if (tmp.Left != null)
                    {
                        tmp = tmp.Left;
                    }
                    else
                    {
                        Node<K, V> parent = tmp.Parent;
                        Node<K, V> ptr = tmp;
                        while (parent != null && ptr == parent.Left)
                        {
                            ptr = parent;
                            parent = parent.Parent;
                        }
                        return parent;
                    }
                }
                else
                {
                    return tmp;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the node whose key is immediately greater than or equal to than <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key for which to find the ceiling node of</param>
        /// <returns>Node whose key is immediately greater than or equal to <paramref name="key"/>, or null if no such node is found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        private Node<K, V> CeilingNode(K key)
        {
            ArgumentNullException.ThrowIfNull(key);

            Node<K, V> tmp = Root;

            while (tmp != null)
            {
                int cmp = key.CompareTo(tmp.Key);
                if (cmp < 0)
                {
                    if (tmp.Left != null)
                    {
                        tmp = tmp.Left;
                    }
                    else
                    {
                        return tmp;
                    }
                }
                else if (cmp > 0)
                {
                    if (tmp.Right != null)
                    {
                        tmp = tmp.Right;
                    }
                    else
                    {
                        Node<K, V> parent = tmp.Parent;
                        Node<K, V> ptr = tmp;
                        while (parent != null && ptr == parent.Right)
                        {
                            ptr = parent;
                            parent = parent.Parent;
                        }
                        return parent;
                    }
                }
                else
                {
                    return tmp;
                }
            }
            return null;
        }

        #endregion

        #region Interface Implementations

        // Method descriptions are not provided as they are already included as part of the interface.
        public bool ContainsKey(K key)
        {
            ArgumentNullException.ThrowIfNull(key);

            return GetNode(key) != null;
        }

        bool IDictionary<K, V>.Remove(K key)
        {
            int count = Count;
            Remove(key);
            return count > Count;
        }

        public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
        {
            ArgumentNullException.ThrowIfNull(key);

            Node<K, V> node = GetNode(key);
            value = node != null ? node.Value : default;
            return node != null;
        }

        public void Add(KeyValuePair<K, V> item)
        {
            ArgumentNullException.ThrowIfNull(item.Key);

            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            if (item.Key == null)
            {
                return false;
            }

            Node<K, V> node = GetNode(item.Key);
            if (node != null)
            {
                return node.Key.Equals(item.Key) && node.Value.Equals(item.Value);
            }
            return false;
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            SortedList<K, V> list = GetKeyValues();

            int offset = 0;

            for (int i = arrayIndex; i < array.Length && offset < list.Count; i++)
            {
                array[i] = new KeyValuePair<K, V>(list.Keys[i], list.Values[i]);
                offset++;
            }
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            Node<K, V> node = GetNode(item.Key);

            if (node == null)
            {
                return false;
            }

            if (node.Value.Equals(item.Value))
            {
                int count = Count;
                Remove(item.Key);
                return count > Count;
            }

            return false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        public ICollection<K> Keys => GetKeyValues().Keys;

        public ICollection<V> Values => GetKeyValues().Values;

        public bool IsReadOnly => false;

        public V this[K key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        #endregion

        #region Private Interface Helper Methods

        /// <summary>
        /// Returns a sorted list of all the node keys / values in the tree.
        /// </summary>
        /// <returns>List of node keys</returns>
        private SortedList<K, V> GetKeyValues()
        {
            SortedList<K, V> set = new SortedList<K, V>();
            Queue<Node<K, V>> queue = new Queue<Node<K, V>>();
            if (Root != null)
            {
                queue.Enqueue(Root);
            }

            while (queue.TryDequeue(out Node<K, V> node))
            {
                set.Add(node.Key, node.Value);
                if (null != node.Left)
                {
                    queue.Enqueue(node.Left);
                }
                if (null != node.Right)
                {
                    queue.Enqueue(node.Right);
                }
            }

            return set;
        }

        #endregion
    }

    /// <summary>
    /// Represents a node in the TreeDictionary which contains a key and value of generic type K and V, respectively.
    /// </summary>
    /// <typeparam name="K">Key of the node</typeparam>
    /// <typeparam name="V">Value of the node</typeparam>
    public class Node<K, V> : IntrusiveRedBlackTreeNode<Node<K, V>> where K : IComparable<K>
    {
        internal K Key;
        internal V Value;

        internal Node(K key, V value, Node<K, V> parent)
        {
            Key = key;
            Value = value;
            Parent = parent;
        }
    }
}
