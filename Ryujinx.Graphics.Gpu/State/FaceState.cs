using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Face culling and orientation parameters.
    /// </summary>
    struct FaceState
    {
        public Boolean32 CullEnable;
        public FrontFace FrontFace;
        public Face      CullFace;
    }
}
