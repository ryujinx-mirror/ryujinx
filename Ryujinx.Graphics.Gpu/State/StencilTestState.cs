using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Stencil front test state and masks.
    /// </summary>
    struct StencilTestState
    {
        public Boolean32 Enable;
        public StencilOp FrontSFail;
        public StencilOp FrontDpFail;
        public StencilOp FrontDpPass;
        public CompareOp FrontFunc;
        public int       FrontFuncRef;
        public int       FrontFuncMask;
        public int       FrontMask;
    }
}
