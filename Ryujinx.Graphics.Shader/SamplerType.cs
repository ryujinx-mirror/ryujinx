using Ryujinx.Graphics.Shader.StructuredIr;
using System;

namespace Ryujinx.Graphics.Shader
{
    [Flags]
    public enum SamplerType
    {
        None = 0,
        Texture1D,
        TextureBuffer,
        Texture2D,
        Texture3D,
        TextureCube,

        Mask = 0xff,

        Array       = 1 << 8,
        Indexed     = 1 << 9,
        Multisample = 1 << 10,
        Shadow      = 1 << 11
    }

    static class SamplerTypeExtensions
    {
        public static int GetDimensions(this SamplerType type)
        {
            switch (type & SamplerType.Mask)
            {
                case SamplerType.Texture1D:     return 1;
                case SamplerType.TextureBuffer: return 1;
                case SamplerType.Texture2D:     return 2;
                case SamplerType.Texture3D:     return 3;
                case SamplerType.TextureCube:   return 3;
            }

            throw new ArgumentException($"Invalid sampler type \"{type}\".");
        }

        public static string ToGlslSamplerType(this SamplerType type)
        {
            string typeName;

            switch (type & SamplerType.Mask)
            {
                case SamplerType.Texture1D:     typeName = "sampler1D";     break;
                case SamplerType.TextureBuffer: typeName = "samplerBuffer"; break;
                case SamplerType.Texture2D:     typeName = "sampler2D";     break;
                case SamplerType.Texture3D:     typeName = "sampler3D";     break;
                case SamplerType.TextureCube:   typeName = "samplerCube";   break;

                default: throw new ArgumentException($"Invalid sampler type \"{type}\".");
            }

            if ((type & SamplerType.Multisample) != 0)
            {
                typeName += "MS";
            }

            if ((type & SamplerType.Array) != 0)
            {
                typeName += "Array";
            }

            if ((type & SamplerType.Shadow) != 0)
            {
                typeName += "Shadow";
            }

            return typeName;
        }

        public static string ToGlslImageType(this SamplerType type, VariableType componentType)
        {
            string typeName;

            switch (type & SamplerType.Mask)
            {
                case SamplerType.Texture1D:     typeName = "image1D";     break;
                case SamplerType.TextureBuffer: typeName = "imageBuffer"; break;
                case SamplerType.Texture2D:     typeName = "image2D";     break;
                case SamplerType.Texture3D:     typeName = "image3D";     break;
                case SamplerType.TextureCube:   typeName = "imageCube";   break;

                default: throw new ArgumentException($"Invalid sampler type \"{type}\".");
            }

            if ((type & SamplerType.Multisample) != 0)
            {
                typeName += "MS";
            }

            if ((type & SamplerType.Array) != 0)
            {
                typeName += "Array";
            }

            switch (componentType)
            {
                case VariableType.U32: typeName = 'u' + typeName; break;
                case VariableType.S32: typeName = 'i' + typeName; break;
            }

            return typeName;
        }
    }
}