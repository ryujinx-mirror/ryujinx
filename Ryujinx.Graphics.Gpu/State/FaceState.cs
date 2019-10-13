using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    struct FaceState
    {
        public Bool      CullEnable;
        public FrontFace FrontFace;
        public Face      CullFace;
    }
}
