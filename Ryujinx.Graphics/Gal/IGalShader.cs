using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalShader
    {
        void Create(IGalMemory Memory, long Key, GalShaderType Type);

        void Create(IGalMemory Memory, long VpAPos, long Key, GalShaderType Type);

        IEnumerable<ShaderDeclInfo> GetTextureUsage(long Key);

        void EnsureTextureBinding(string UniformName, int Value);

        void Bind(long Key);

        void Unbind(GalShaderType Type);

        void BindProgram();
    }
}