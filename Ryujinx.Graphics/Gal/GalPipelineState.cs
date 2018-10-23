namespace Ryujinx.Graphics.Gal
{
    public struct ColorMaskRgba
    {
        private static readonly ColorMaskRgba _Default = new ColorMaskRgba()
        {
            Red   = true,
            Green = true,
            Blue  = true,
            Alpha = true
        };

        public static ColorMaskRgba Default => _Default;

        public bool Red;
        public bool Green;
        public bool Blue;
        public bool Alpha;
    }

    public class GalPipelineState
    {
        public const int Stages               = 5;
        public const int ConstBuffersPerStage = 18;
        public const int RenderTargetsCount   = 8;

        public long[][] ConstBufferKeys;

        public GalVertexBinding[] VertexBindings;

        public bool FramebufferSrgb;

        public float FlipX;
        public float FlipY;

        public int Instance;

        public GalFrontFace FrontFace;

        public bool CullFaceEnabled;
        public GalCullFace CullFace;

        public bool DepthTestEnabled;
        public bool DepthWriteEnabled;
        public GalComparisonOp DepthFunc;
        public float DepthRangeNear;
        public float DepthRangeFar;

        public bool StencilTestEnabled;
        public bool StencilTwoSideEnabled;

        public GalComparisonOp StencilBackFuncFunc;
        public int StencilBackFuncRef;
        public uint StencilBackFuncMask;
        public GalStencilOp StencilBackOpFail;
        public GalStencilOp StencilBackOpZFail;
        public GalStencilOp StencilBackOpZPass;
        public uint StencilBackMask;

        public GalComparisonOp StencilFrontFuncFunc;
        public int StencilFrontFuncRef;
        public uint StencilFrontFuncMask;
        public GalStencilOp StencilFrontOpFail;
        public GalStencilOp StencilFrontOpZFail;
        public GalStencilOp StencilFrontOpZPass;
        public uint StencilFrontMask;

        public bool BlendEnabled;
        public bool BlendSeparateAlpha;
        public GalBlendEquation BlendEquationRgb;
        public GalBlendFactor BlendFuncSrcRgb;
        public GalBlendFactor BlendFuncDstRgb;
        public GalBlendEquation BlendEquationAlpha;
        public GalBlendFactor BlendFuncSrcAlpha;
        public GalBlendFactor BlendFuncDstAlpha;

        public ColorMaskRgba ColorMask;
        public ColorMaskRgba[] ColorMasks;

        public bool PrimitiveRestartEnabled;
        public uint PrimitiveRestartIndex;

        public GalPipelineState()
        {
            ConstBufferKeys = new long[Stages][];

            for (int Stage = 0; Stage < Stages; Stage++)
            {
                ConstBufferKeys[Stage] = new long[ConstBuffersPerStage];
            }

            ColorMasks = new ColorMaskRgba[RenderTargetsCount];
        }
    }
}