using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.Graphics.Shader
{
    public struct ShaderConfig
    {
        public GalShaderType Type { get; }

        public int MaxCBufferSize;

        public ShaderConfig(GalShaderType type, int maxCBufferSize)
        {
            if (maxCBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCBufferSize));
            }

            Type           = type;
            MaxCBufferSize = maxCBufferSize;
        }
    }
}