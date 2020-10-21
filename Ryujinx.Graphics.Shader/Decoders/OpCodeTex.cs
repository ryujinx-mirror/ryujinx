using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTex : OpCodeTexture
    {
        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTex(emitter, address, opCode);

        public OpCodeTex(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            HasDepthCompare = opCode.Extract(50);

            HasOffset = opCode.Extract(54);
        }
    }
}