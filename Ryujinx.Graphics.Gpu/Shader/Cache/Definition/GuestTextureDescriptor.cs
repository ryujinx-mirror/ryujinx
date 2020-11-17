using Ryujinx.Graphics.Gpu.Image;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Contains part of TextureDescriptor from <see cref="Image"/> used for shader codegen.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xC, Pack = 1)]
    struct GuestTextureDescriptor : ITextureDescriptor
    {
        public uint Handle;
        public uint Format;
        public TextureTarget Target;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsSrgb;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsTextureCoordNormalized;
        public byte Reserved;

        public uint UnpackFormat()
        {
            return Format;
        }

        public bool UnpackSrgb()
        {
            return IsSrgb;
        }

        public bool UnpackTextureCoordNormalized()
        {
            return IsTextureCoordNormalized;
        }

        public TextureTarget UnpackTextureTarget()
        {
            return Target;
        }
    }
}
