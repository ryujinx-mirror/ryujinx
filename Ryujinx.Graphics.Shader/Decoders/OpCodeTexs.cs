using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTexs : OpCodeTextureScalar
    {
        public TextureTarget Target => (TextureTarget)RawType;

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTexs(emitter, address, opCode);

        public OpCodeTexs(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode) { }
    }
}