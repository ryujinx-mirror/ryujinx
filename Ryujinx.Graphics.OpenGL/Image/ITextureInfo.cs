using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.OpenGL.Image
{
    interface ITextureInfo
    {
        int Handle { get; }
        TextureCreateInfo Info { get; }
    }
}
