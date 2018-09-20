namespace Ryujinx.Graphics.Gal
{
    public struct GalVertexBinding
    {
        //VboKey shouldn't be here, but ARB_vertex_attrib_binding is core since 4.3

        public bool Enabled;
        public int Stride;
        public long VboKey;
        public bool Instanced;
        public int Divisor;
        public GalVertexAttrib[] Attribs;
    }

    public class GalPipelineState
    {
        public const int Stages = 5;
        public const int ConstBuffersPerStage = 18;

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

        public bool StencilTestEnabled;

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

        public bool PrimitiveRestartEnabled;
        public uint PrimitiveRestartIndex;

        public GalPipelineState()
        {
            ConstBufferKeys = new long[Stages][];

            for (int Stage = 0; Stage < Stages; Stage++)
            {
                ConstBufferKeys[Stage] = new long[ConstBuffersPerStage];
            }
        }
    }
}