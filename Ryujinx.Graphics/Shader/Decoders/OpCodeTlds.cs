using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTlds : OpCodeTextureScalar
    {
        public TexelLoadScalarType Type => (TexelLoadScalarType)RawType;

        public OpCodeTlds(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode) { }
    }
}