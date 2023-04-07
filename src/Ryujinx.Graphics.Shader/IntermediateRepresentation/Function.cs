namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    class Function
    {
        public BasicBlock[] Blocks { get; }

        public string Name { get; }

        public bool ReturnsValue { get; }

        public int InArgumentsCount { get; }
        public int OutArgumentsCount { get; }

        public Function(BasicBlock[] blocks, string name, bool returnsValue, int inArgumentsCount, int outArgumentsCount)
        {
            Blocks = blocks;
            Name = name;
            ReturnsValue = returnsValue;
            InArgumentsCount = inArgumentsCount;
            OutArgumentsCount = outArgumentsCount;
        }
    }
}