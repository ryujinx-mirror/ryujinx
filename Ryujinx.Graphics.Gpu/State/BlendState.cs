using Ryujinx.Graphics.GAL.Blend;

namespace Ryujinx.Graphics.Gpu.State
{
    struct BlendState
    {
        public Boolean32   SeparateAlpha;
        public BlendOp     ColorOp;
        public BlendFactor ColorSrcFactor;
        public BlendFactor ColorDstFactor;
        public BlendOp     AlphaOp;
        public BlendFactor AlphaSrcFactor;
        public BlendFactor AlphaDstFactor;
        public uint        Padding;
    }
}
