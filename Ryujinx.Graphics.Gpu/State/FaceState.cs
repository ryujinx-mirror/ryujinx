using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    struct FaceState
    {
        public Boolean32 CullEnable;
        public FrontFace FrontFace;
        public Face      CullFace;
    }
}
