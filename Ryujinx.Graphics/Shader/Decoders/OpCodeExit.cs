using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeExit : OpCode
    {
        public Condition Condition { get; }

        public OpCodeExit(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Condition = (Condition)opCode.Extract(0, 5);
        }
    }
}