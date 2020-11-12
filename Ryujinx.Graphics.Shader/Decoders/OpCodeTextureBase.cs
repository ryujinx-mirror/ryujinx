using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTextureBase : OpCode
    {
        public int HandleOffset { get; }

        public OpCodeTextureBase(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            HandleOffset = opCode.Extract(36, 13);
        }
    }
}
