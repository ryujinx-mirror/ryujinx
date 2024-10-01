namespace Ryujinx.Graphics.GAL
{
    public readonly struct DepthTestDescriptor
    {
        public bool TestEnable { get; }
        public bool WriteEnable { get; }

        public CompareOp Func { get; }

        public DepthTestDescriptor(
            bool testEnable,
            bool writeEnable,
            CompareOp func)
        {
            TestEnable = testEnable;
            WriteEnable = writeEnable;
            Func = func;
        }
    }
}
