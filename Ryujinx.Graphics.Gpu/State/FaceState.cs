using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Face culling and orientation parameters.
    /// </summary>
    struct FaceState
    {
#pragma warning disable CS0649
        public Boolean32 CullEnable;
        public FrontFace FrontFace;
        public Face      CullFace;
#pragma warning restore CS0649
    }
}
