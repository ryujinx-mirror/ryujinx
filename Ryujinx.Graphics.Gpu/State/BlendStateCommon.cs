using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Color buffer blending parameters, shared by all color buffers.
    /// </summary>
    struct BlendStateCommon
    {
#pragma warning disable CS0649
        public Boolean32   SeparateAlpha;
        public BlendOp     ColorOp;
        public BlendFactor ColorSrcFactor;
        public BlendFactor ColorDstFactor;
        public BlendOp     AlphaOp;
        public BlendFactor AlphaSrcFactor;
        public uint        Unknown0x1354;
        public BlendFactor AlphaDstFactor;
#pragma warning restore CS0649

        public static BlendStateCommon Default = new BlendStateCommon
        {
            ColorOp = BlendOp.Add,
            ColorSrcFactor = BlendFactor.One,
            ColorDstFactor = BlendFactor.Zero,
            AlphaOp = BlendOp.Add,
            AlphaSrcFactor = BlendFactor.One,
            AlphaDstFactor = BlendFactor.Zero
        };
    }
}
