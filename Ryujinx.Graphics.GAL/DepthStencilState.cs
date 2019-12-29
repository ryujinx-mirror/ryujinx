namespace Ryujinx.Graphics.GAL
{
    public struct DepthStencilState
    {
        public bool DepthTestEnable   { get; }
        public bool DepthWriteEnable  { get; }
        public bool StencilTestEnable { get; }

        public CompareOp DepthFunc          { get; }
        public CompareOp StencilFrontFunc   { get; }
        public StencilOp StencilFrontSFail  { get; }
        public StencilOp StencilFrontDpPass { get; }
        public StencilOp StencilFrontDpFail { get; }
        public CompareOp StencilBackFunc    { get; }
        public StencilOp StencilBackSFail   { get; }
        public StencilOp StencilBackDpPass  { get; }
        public StencilOp StencilBackDpFail  { get; }

        public DepthStencilState(
            bool      depthTestEnable,
            bool      depthWriteEnable,
            bool      stencilTestEnable,
            CompareOp depthFunc,
            CompareOp stencilFrontFunc,
            StencilOp stencilFrontSFail,
            StencilOp stencilFrontDpPass,
            StencilOp stencilFrontDpFail,
            CompareOp stencilBackFunc,
            StencilOp stencilBackSFail,
            StencilOp stencilBackDpPass,
            StencilOp stencilBackDpFail)
        {
            DepthTestEnable    = depthTestEnable;
            DepthWriteEnable   = depthWriteEnable;
            StencilTestEnable  = stencilTestEnable;
            DepthFunc          = depthFunc;
            StencilFrontFunc   = stencilFrontFunc;
            StencilFrontSFail  = stencilFrontSFail;
            StencilFrontDpPass = stencilFrontDpPass;
            StencilFrontDpFail = stencilFrontDpFail;
            StencilBackFunc    = stencilBackFunc;
            StencilBackSFail   = stencilBackSFail;
            StencilBackDpPass  = stencilBackDpPass;
            StencilBackDpFail  = stencilBackDpFail;
        }
    }
}
