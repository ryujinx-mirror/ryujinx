using Ryujinx.Graphics.GAL.Texture;

namespace Ryujinx.Graphics.Gpu.Image
{
    struct TextureBindingInfo
    {
        public Target Target { get; }

        public int Handle { get; }

        public TextureBindingInfo(Target target, int handle)
        {
            Target = target;
            Handle = handle;
        }
    }
}