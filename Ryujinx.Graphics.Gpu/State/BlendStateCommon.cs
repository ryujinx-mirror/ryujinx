using Ryujinx.Graphics.GAL.Blend;

namespace Ryujinx.Graphics.Gpu.State
{
    struct BlendStateCommon
    {
        public Boolean32   SeparateAlpha;
        public BlendOp     ColorOp;
        public BlendFactor ColorSrcFactor;
        public BlendFactor ColorDstFactor;
        public BlendOp     AlphaOp;
        public BlendFactor AlphaSrcFactor;
        public uint        Unknown0x1354;
        public BlendFactor AlphaDstFactor;
    }
}
