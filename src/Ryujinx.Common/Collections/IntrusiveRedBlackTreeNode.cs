namespace Ryujinx.Common.Collections
{
    /// <summary>
    /// Represents a node in the Red-Black Tree.
    /// </summary>
    public class IntrusiveRedBlackTreeNode<T> where T : IntrusiveRedBlackTreeNode<T>
    {
        internal bool Color = true;
        internal T Left;
        internal T Right;
        internal T Parent;

        public T Predecessor => IntrusiveRedBlackTreeImpl<T>.PredecessorOf((T)this);
        public T Successor => IntrusiveRedBlackTreeImpl<T>.SuccessorOf((T)this);
    }
}
