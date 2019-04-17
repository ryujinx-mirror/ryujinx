using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTexs : OpCodeTextureScalar
    {
        public TextureScalarType Type => (TextureScalarType)RawType;

        public OpCodeTexs(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode) { }
    }
}