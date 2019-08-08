namespace ARMeilleure.IntermediateRepresentation
{
    class PhiNode : Node
    {
        private BasicBlock[] _blocks;

        public PhiNode(Operand destination, int predecessorsCount) : base(destination, predecessorsCount)
        {
            _blocks = new BasicBlock[predecessorsCount];
        }

        public BasicBlock GetBlock(int index)
        {
            return _blocks[index];
        }

        public void SetBlock(int index, BasicBlock block)
        {
            _blocks[index] = block;
        }
    }
}