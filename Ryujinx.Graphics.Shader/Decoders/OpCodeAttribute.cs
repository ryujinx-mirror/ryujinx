using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAttribute : OpCodeAluReg
    {
        public int AttributeOffset { get; }
        public int Count           { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAttribute(emitter, address, opCode);

        public OpCodeAttribute(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            AttributeOffset = opCode.Extract(20, 10);
            Count           = opCode.Extract(47, 2) + 1;
        }
    }
}