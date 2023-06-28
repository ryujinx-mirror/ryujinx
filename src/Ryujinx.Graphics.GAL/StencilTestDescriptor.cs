namespace Ryujinx.Graphics.GAL
{
    public readonly struct StencilTestDescriptor
    {
        public bool TestEnable { get; }

        public CompareOp FrontFunc { get; }
        public StencilOp FrontSFail { get; }
        public StencilOp FrontDpPass { get; }
        public StencilOp FrontDpFail { get; }
        public int FrontFuncRef { get; }
        public int FrontFuncMask { get; }
        public int FrontMask { get; }
        public CompareOp BackFunc { get; }
        public StencilOp BackSFail { get; }
        public StencilOp BackDpPass { get; }
        public StencilOp BackDpFail { get; }
        public int BackFuncRef { get; }
        public int BackFuncMask { get; }
        public int BackMask { get; }

        public StencilTestDescriptor(
            bool testEnable,
            CompareOp frontFunc,
            StencilOp frontSFail,
            StencilOp frontDpPass,
            StencilOp frontDpFail,
            int frontFuncRef,
            int frontFuncMask,
            int frontMask,
            CompareOp backFunc,
            StencilOp backSFail,
            StencilOp backDpPass,
            StencilOp backDpFail,
            int backFuncRef,
            int backFuncMask,
            int backMask)
        {
            TestEnable = testEnable;
            FrontFunc = frontFunc;
            FrontSFail = frontSFail;
            FrontDpPass = frontDpPass;
            FrontDpFail = frontDpFail;
            FrontFuncRef = frontFuncRef;
            FrontFuncMask = frontFuncMask;
            FrontMask = frontMask;
            BackFunc = backFunc;
            BackSFail = backSFail;
            BackDpPass = backDpPass;
            BackDpFail = backDpFail;
            BackFuncRef = backFuncRef;
            BackFuncMask = backFuncMask;
            BackMask = backMask;
        }
    }
}
