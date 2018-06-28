using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalShader
    {
        void Create(IGalMemory Memory, long Key, GalShaderType Type);

        void Create(IGalMemory Memory, long VpAPos, long Key, GalShaderType Type);

        IEnumerable<ShaderDeclInfo> GetTextureUsage(long Key);

        void SetConstBuffer(long Key, int Cbuf, byte[] Data);

        void EnsureTextureBinding(string UniformName, int Value);

        void SetFlip(float X, float Y);

        void Bind(long Key);

        void BindProgram();
    }
}