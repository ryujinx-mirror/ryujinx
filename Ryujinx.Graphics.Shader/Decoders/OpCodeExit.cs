using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeExit : OpCodeConditional
    {
        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeExit(emitter, address, opCode);

        public OpCodeExit(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
        }
    }
}