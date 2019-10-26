using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.DepthStencil;

namespace Ryujinx.Graphics.Gpu.State
{
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
