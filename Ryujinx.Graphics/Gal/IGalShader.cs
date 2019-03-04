using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalShader
    {
        void Create(IGalMemory memory, long key, GalShaderType type);

        void Create(IGalMemory memory, long vpAPos, long key, GalShaderType type);

        IEnumerable<ShaderDeclInfo> GetConstBufferUsage(long key);
        IEnumerable<ShaderDeclInfo> GetTextureUsage(long key);

        void Bind(long key);

        void Unbind(GalShaderType type);

        void BindProgram();
    }
}