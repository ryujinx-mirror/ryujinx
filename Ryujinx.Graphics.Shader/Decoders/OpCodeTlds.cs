using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTlds : OpCodeTextureScalar
    {
        public TexelLoadTarget Target => (TexelLoadTarget)RawType;

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTlds(emitter, address, opCode);

        public OpCodeTlds(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode) { }
    }
}