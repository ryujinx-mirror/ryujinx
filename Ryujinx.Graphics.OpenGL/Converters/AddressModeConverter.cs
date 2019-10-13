using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Sampler;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class AddressModeConverter
    {
        public static TextureWrapMode Convert(this AddressMode mode)
        {
            switch (mode)
            {
                case AddressMode.Clamp               : return TextureWrapMode.Clamp;
                case AddressMode.Repeat              : return TextureWrapMode.Repeat;
                case AddressMode.MirrorClamp         : return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampExt;
                case AddressMode.MirrorClampToEdge   : return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToEdgeExt;
                case AddressMode.MirrorClampToBorder : return (TextureWrapMode)ExtTextureMirrorClamp.MirrorClampToBorderExt;
                case AddressMode.ClampToBorder       : return TextureWrapMode.ClampToBorder;
                case AddressMode.MirroredRepeat      : return TextureWrapMode.MirroredRepeat;
                case AddressMode.ClampToEdge         : return TextureWrapMode.ClampToEdge;
            }

            throw new ArgumentException($"Invalid address mode \"{mode}\".");
        }
    }
}
