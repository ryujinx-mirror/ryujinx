using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeConditional : OpCode
    {
        public Condition Condition { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeConditional(emitter, address, opCode);

        public OpCodeConditional(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Condition = (Condition)opCode.Extract(0, 5);
        }
    }
}