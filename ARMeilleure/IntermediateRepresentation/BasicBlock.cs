using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class BasicBlock : IIntrusiveListNode<BasicBlock>
    {
        public int Index { get; set; }

        public BasicBlock ListPrevious { get; set; }
        public BasicBlock ListNext { get; set; }

        public IntrusiveList<Node> Operations { get; }

        private BasicBlock _next;
        private BasicBlock _branch;

        public BasicBlock Next
        {
            get => _next;
            set => _next = AddSuccessor(_next, value);
        }

        public BasicBlock Branch
        {
            get => _branch;
            set => _branch = AddSuccessor(_branch, value);
        }

        public List<BasicBlock> Predecessors { get; }

        public HashSet<BasicBlock> DominanceFrontiers { get; }

        public BasicBlock ImmediateDominator { get; set; }

        public BasicBlock()
        {
            Operations = new IntrusiveList<Node>();

            Predecessors = new List<BasicBlock>();

            DominanceFrontiers = new HashSet<BasicBlock>();

            Index = -1;
        }

        public BasicBlock(int index) : this()
        {
            Index = index;
        }

        private BasicBlock AddSuccessor(BasicBlock oldBlock, BasicBlock newBlock)
        {
            oldBlock?.Predecessors.Remove(this);
            newBlock?.Predecessors.Add(this);

            return newBlock;
        }

        public void Append(Node node)
        {
            // If the branch block is not null, then the list of operations
            // should end with a branch instruction. We insert the new operation
            // before this branch.
            if (_branch != null || (Operations.Last != null && IsLeafBlock()))
            {
                Operations.AddBefore(Operations.Last, node);
            }
            else
            {
                Operations.AddLast(node);
            }
        }

        private bool IsLeafBlock()
        {
            return _branch == null && _next == null;
        }

        public Node GetLastOp()
        {
            return Operations.Last;
        }
    }
}