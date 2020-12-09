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
    public class TreeDictionary<K, V> : IDictionary<K, V> where K : IComparable<K>
    {
        private const bool Black = true;
        private const bool Red = false;
        private Node<K, V> _root = null;
        private int _count = 0;
        public TreeDictionary() { }

        #region Public Methods

        /// <summary>
        /// Returns the value of the node whose key is <paramref name="key"/>, or the default value if no such node exists.
        /// </summary>
        /// <param name="key">Key of the node value to get</param>
        /// <returns>Value associated w/ <paramref name="key"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public V Get(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Insert(key, value);
        }

        /// <summary>
        /// Removes the node whose key is <paramref name="key"/> from the tree.
        /// </summary>
        /// <param name="key">Key of the node to remove</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public void Remove(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (Delete(key) != null)
            {
                _count--;
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

            if (this._root != null)
            {
                nodes.Enqueue(this._root);
            }
            while (nodes.Count > 0)
            {
                Node<K, V> node = nodes.Dequeue();
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
        /// <br></br>
        /// The nodes will be added in Sorted by Key Order.
        /// </summary>
        public List<KeyValuePair<K, V>> AsList()
        {
            List<KeyValuePair<K, V>> list = new List<KeyValuePair<K, V>>();

            Queue<Node<K, V>> nodes = new Queue<Node<K, V>>();

            if (this._root != null)
            {
                nodes.Enqueue(this._root);
            }
            while (nodes.Count > 0)
            {
                Node<K, V> node = nodes.Dequeue();
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
        #endregion
        #region Private Methods (BST)

        /// <summary>
        /// Retrieve the node reference whose key is <paramref name="key"/>, or null if no such node exists.
        /// </summary>
        /// <param name="key">Key of the node to get</param>
        /// <returns>Node reference in the tree</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        private Node<K, V> GetNode(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Node<K, V> node = _root;
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
            Node<K, V> node = _root;

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
                _root = newNode;
            }
            else if (key.CompareTo(parent.Key) < 0)
            {
                parent.Left = newNode;
            }
            else
            {
                parent.Right = newNode;
            }
            _count++;
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
        /// Returns the node with the largest key where <paramref name="node"/> is considered the root node.
        /// </summary>
        /// <param name="node">Root Node</param>
        /// <returns>Node with the maximum key in the tree of <paramref name="node"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is null</exception>
        private static Node<K, V> Maximum(Node<K, V> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            Node<K, V> tmp = node;
            while (tmp.Right != null)
            {
                tmp = tmp.Right;
            }

            return tmp;
        }

        /// <summary>
        /// Returns the node with the smallest key where <paramref name="node"/> is considered the root node.
        /// </summary>
        /// <param name="node">Root Node</param>
        /// <returns>Node with the minimum key in the tree of <paramref name="node"/></returns>
        ///<exception cref="ArgumentNullException"><paramref name="node"/> is null</exception>
        private static Node<K, V> Minimum(Node<K, V> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            Node<K, V> tmp = node;
            while (tmp.Left != null)
            {
                tmp = tmp.Left;
            }

            return tmp;
        }

        /// <summary>
        /// Returns the node whose key immediately less than or equal to <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key for which to find the floor node of</param>
        /// <returns>Node whose key is immediately less than or equal to <paramref name="key"/>, or null if no such node is found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        private Node<K, V> FloorNode(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            Node<K, V> tmp = _root;

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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            Node<K, V> tmp = _root;

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

        /// <summary>
        /// Finds the node with the key immediately greater than <paramref name="node"/>.Key.
        /// </summary>
        /// <param name="node">Node to find the successor of</param>
        /// <returns>Successor of <paramref name="node"/></returns>
        private static Node<K, V> SuccessorOf(Node<K, V> node)
        {
            if (node.Right != null)
            {
                return Minimum(node.Right);
            }
            Node<K, V> parent = node.Parent;
            while (parent != null && node == parent.Right)
            {
                node = parent;
                parent = parent.Parent;
            }
            return parent;
        }

        /// <summary>
        /// Finds the node whose key immediately less than <paramref name="node"/>.Key.
        /// </summary>
        /// <param name="node">Node to find the predecessor of</param>
        /// <returns>Predecessor of <paramref name="node"/></returns>
        private static Node<K, V> PredecessorOf(Node<K, V> node)
        {
            if (node.Left != null)
            {
                return Maximum(node.Left);
            }
            Node<K, V> parent = node.Parent;
            while (parent != null && node == parent.Left)
            {
                node = parent;
                parent = parent.Parent;
            }
            return parent;
        }
        #endregion
        #region Private Methods (RBL)

        private void RestoreBalanceAfterRemoval(Node<K, V> balanceNode)
        {
            Node<K, V> ptr = balanceNode;

            while (ptr != _root && ColorOf(ptr) == Black)
            {
                if (ptr == LeftOf(ParentOf(ptr)))
                {
                    Node<K, V> sibling = RightOf(ParentOf(ptr));

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
                    Node<K, V> sibling = LeftOf(ParentOf(ptr));

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

        private void RestoreBalanceAfterInsertion(Node<K, V> balanceNode)
        {
            SetColor(balanceNode, Red);
            while (balanceNode != null && balanceNode != _root && ColorOf(ParentOf(balanceNode)) == Red)
            {
                if (ParentOf(balanceNode) == LeftOf(ParentOf(ParentOf(balanceNode))))
                {
                    Node<K, V> sibling = RightOf(ParentOf(ParentOf(balanceNode)));

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
                    Node<K, V> sibling = LeftOf(ParentOf(ParentOf(balanceNode)));

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

        private void RotateLeft(Node<K, V> node)
        {
            if (node != null)
            {
                Node<K, V> right = RightOf(node);
                node.Right = LeftOf(right);
                if (LeftOf(right) != null)
                {
                    LeftOf(right).Parent = node;
                }
                right.Parent = ParentOf(node);
                if (ParentOf(node) == null)
                {
                    _root = right;
                }
                else if (node == LeftOf(ParentOf(node)))
                {
                    ParentOf(node).Left = right;
                }
                else
                {
                    ParentOf(node).Right = right;
                }
                right.Left = node;
                node.Parent = right;
            }
        }

        private void RotateRight(Node<K, V> node)
        {
            if (node != null)
            {
                Node<K, V> left = LeftOf(node);
                node.Left = RightOf(left);
                if (RightOf(left) != null)
                {
                    RightOf(left).Parent = node;
                }
                left.Parent = node.Parent;
                if (ParentOf(node) == null)
                {
                    _root = left;
                }
                else if (node == RightOf(ParentOf(node)))
                {
                    ParentOf(node).Right = left;
                }
                else
                {
                    ParentOf(node).Left = left;
                }
                left.Right = node;
                node.Parent = left;
            }
        }
        #endregion

        #region Safety-Methods

        // These methods save memory by allowing us to forego sentinel nil nodes, as well as serve as protection against nullpointerexceptions.

        /// <summary>
        /// Returns the color of <paramref name="node"/>, or Black if it is null.
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>The boolean color of <paramref name="node"/>, or black if null</returns>
        private static bool ColorOf(Node<K, V> node)
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
        private static void SetColor(Node<K, V> node, bool color)
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
        private static Node<K, V> LeftOf(Node<K, V> node)
        {
            return node?.Left;
        }

        /// <summary>
        /// This method returns the right node of <paramref name="node"/>, or null if <paramref name="node"/> is null.
        /// </summary>
        /// <param name="node">Node to retrieve the right child from</param>
        /// <returns>Right child of <paramref name="node"/></returns>
        private static Node<K, V> RightOf(Node<K, V> node)
        {
            return node?.Right;
        }

        /// <summary>
        /// Returns the parent node of <paramref name="node"/>, or null if <paramref name="node"/> is null.
        /// </summary>
        /// <param name="node">Node to retrieve the parent from</param>
        /// <returns>Parent of <paramref name="node"/></returns>
        private static Node<K, V> ParentOf(Node<K, V> node)
        {
            return node?.Parent;
        }
        #endregion

        #region Interface Implementations

        // Method descriptions are not provided as they are already included as part of the interface.
        public bool ContainsKey(K key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return GetNode(key) != null;
        }

        bool IDictionary<K, V>.Remove(K key)
        {
            int count = _count;
            Remove(key);
            return count > _count;
        }

        public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
        {
            if (null == key)
            {
                throw new ArgumentNullException(nameof(key));
            }
            Node<K, V> node = GetNode(key);
            value = node != null ? node.Value : default;
            return node != null;
        }

        public void Add(KeyValuePair<K, V> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException(nameof(item.Key));
            }

            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _root = null;
            _count = 0;
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
                int count = _count;
                Remove(item.Key);
                return count > _count;
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

        public int Count => _count;

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
            if (_root != null)
            {
                queue.Enqueue(_root);
            }

            while (queue.Count > 0)
            {
                Node<K, V> node = queue.Dequeue();
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
    internal class Node<K, V>
    {
        internal bool Color = true;
        internal Node<K, V> Left = null;
        internal Node<K, V> Right = null;
        internal Node<K, V> Parent = null;
        internal K Key;
        internal V Value;

        public Node(K key, V value, Node<K, V> parent)
        {
            this.Key = key;
            this.Value = value;
            this.Parent = parent;
        }
    }
}
