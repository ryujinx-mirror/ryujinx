namespace Ryujinx.Graphics.Gal
{
    public struct ColorMaskState
    {
        private static readonly ColorMaskState _Default = new ColorMaskState()
        {
            Red   = true,
            Green = true,
            Blue  = true,
            Alpha = true
        };

        public static ColorMaskState Default => _Default;

        public bool Red;
        public bool Green;
        public bool Blue;
        public bool Alpha;
    }

    public struct BlendState
    {
        private static readonly BlendState _Default = new BlendState()
        {
            Enabled       = false,
            SeparateAlpha = false,
            EquationRgb   = GalBlendEquation.FuncAdd,
            FuncSrcRgb    = GalBlendFactor.One,
            FuncDstRgb    = GalBlendFactor.Zero,
            EquationAlpha = GalBlendEquation.FuncAdd,
            FuncSrcAlpha  = GalBlendFactor.One,
            FuncDstAlpha  = GalBlendFactor.Zero
        };

        public static BlendState Default => _Default;

        public bool             Enabled;
        public bool             SeparateAlpha;
        public GalBlendEquation EquationRgb;
        public GalBlendFactor   FuncSrcRgb;
        public GalBlendFactor   FuncDstRgb;
        public GalBlendEquation EquationAlpha;
        public GalBlendFactor   FuncSrcAlpha;
        public GalBlendFactor   FuncDstAlpha;
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

        public bool        CullFaceEnabled;
        public GalCullFace CullFace;

        public bool            DepthTestEnabled;
        public bool            DepthWriteEnabled;
        public GalComparisonOp DepthFunc;
        public float DepthRangeNear;
        public float DepthRangeFar;

        public bool StencilTestEnabled;
        public bool StencilTwoSideEnabled;

        public GalComparisonOp StencilBackFuncFunc;
        public int             StencilBackFuncRef;
        public uint            StencilBackFuncMask;
        public GalStencilOp    StencilBackOpFail;
        public GalStencilOp    StencilBackOpZFail;
        public GalStencilOp    StencilBackOpZPass;
        public uint            StencilBackMask;

        public GalComparisonOp StencilFrontFuncFunc;
        public int             StencilFrontFuncRef;
        public uint            StencilFrontFuncMask;
        public GalStencilOp    StencilFrontOpFail;
        public GalStencilOp    StencilFrontOpZFail;
        public GalStencilOp    StencilFrontOpZPass;
        public uint            StencilFrontMask;

        public int             ScissorTestCount;
        public bool[]          ScissorTestEnabled;
        public int[]           ScissorTestX;
        public int[]           ScissorTestY;
        public int[]           ScissorTestWidth;
        public int[]           ScissorTestHeight;

        public bool         BlendIndependent;
        public BlendState[] Blends;

        public bool             ColorMaskCommon;
        public ColorMaskState[] ColorMasks;

        public bool PrimitiveRestartEnabled;
        public uint PrimitiveRestartIndex;

        public GalPipelineState()
        {
            ConstBufferKeys = new long[Stages][];

            for (int Stage = 0; Stage < Stages; Stage++)
            {
                ConstBufferKeys[Stage] = new long[ConstBuffersPerStage];
            }

            Blends = new BlendState[RenderTargetsCount];

            ScissorTestEnabled  = new bool[RenderTargetsCount];
            ScissorTestY        = new int[RenderTargetsCount];
            ScissorTestX        = new int[RenderTargetsCount];
            ScissorTestWidth    = new int[RenderTargetsCount];
            ScissorTestHeight   = new int[RenderTargetsCount];

            ColorMasks = new ColorMaskState[RenderTargetsCount];
        }
    }
}