using Ryujinx.Graphics.Shader;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalShader
    {
        void Create(IGalMemory memory, long key, GalShaderType type);

        void Create(IGalMemory memory, long vpAPos, long key, GalShaderType type);

        IEnumerable<CBufferDescriptor> GetConstBufferUsage(long key);
        IEnumerable<TextureDescriptor> GetTextureUsage(long key);

        void Bind(long key);

        void Unbind(GalShaderType type);

        void BindProgram();
    }
}