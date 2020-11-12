using Ryujinx.Graphics.Gpu.Image;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Mostly identical to TextureDescriptor from <see cref="Image"/> but we don't store the address of the texture and store its handle instead.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x20, Pack = 1)]
    struct GuestTextureDescriptor
    {
        public uint Handle;
        internal TextureDescriptor Descriptor;
    }
}
