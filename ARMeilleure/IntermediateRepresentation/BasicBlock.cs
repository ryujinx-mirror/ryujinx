using System;
using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class BasicBlock : IIntrusiveListNode<BasicBlock>
    {
        private readonly List<BasicBlock> _successors;

        public int Index { get; set; }

        public BasicBlockFrequency Frequency { get; set; }

        public BasicBlock ListPrevious { get; set; }
        public BasicBlock ListNext { get; set; }

        public IntrusiveList<Node> Operations { get; }

        public List<BasicBlock> Predecessors { get; }

        public HashSet<BasicBlock> DominanceFrontiers { get; }
        public BasicBlock ImmediateDominator { get; set; }

        public int SuccessorCount => _successors.Count;

        public BasicBlock() : this(index: -1) { }

        public BasicBlock(int index)
        {
            _successors = new List<BasicBlock>();

            Operations = new IntrusiveList<Node>();
            Predecessors = new List<BasicBlock>();
            DominanceFrontiers = new HashSet<BasicBlock>();

            Index = index;
        }

        public void AddSuccessor(BasicBlock block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            block.Predecessors.Add(this);

            _successors.Add(block);
        }

        public void RemoveSuccessor(int index)
        {
            BasicBlock oldBlock = _successors[index];

            oldBlock.Predecessors.Remove(this);

            _successors.RemoveAt(index);
        }

        public BasicBlock GetSuccessor(int index)
        {
            return _successors[index];
        }

        public void SetSuccessor(int index, BasicBlock block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            BasicBlock oldBlock = _successors[index];

            oldBlock.Predecessors.Remove(this);
            block.Predecessors.Add(this);

            _successors[index] = block;
        }

        public void Append(Node node)
        {
            var lastOp = Operations.Last as Operation;

            // Append node before terminal or to end if no terminal.
            switch (lastOp?.Instruction)
            {
                case Instruction.Return:
                case Instruction.Tailcall:
                case Instruction.BranchIf:
                    Operations.AddBefore(lastOp, node);
                    break;

                default:
                    Operations.AddLast(node);
                    break;
            }
        }

        public Node GetLastOp()
        {
            return Operations.Last;
        }
    }
}