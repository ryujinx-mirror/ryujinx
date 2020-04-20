using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Stencil back test state.
    /// </summary>
    struct StencilBackTestState
    {
#pragma warning disable CS0649
        public Boolean32 TwoSided;
        public StencilOp BackSFail;
        public StencilOp BackDpFail;
        public StencilOp BackDpPass;
        public CompareOp BackFunc;
#pragma warning restore CS0649
    }
}
