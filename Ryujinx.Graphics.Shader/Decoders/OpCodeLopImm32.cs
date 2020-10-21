using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeLopImm32 : OpCodeAluImm32, IOpCodeLop, IOpCodeImm
    {
        public LogicalOperation LogicalOp { get; }

        public bool InvertA { get; }
        public bool InvertB { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeLopImm32(emitter, address, opCode);

        public OpCodeLopImm32(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            LogicalOp = (LogicalOperation)opCode.Extract(53, 2);

            InvertA = opCode.Extract(55);
            InvertB = opCode.Extract(56);

            Extended = opCode.Extract(57);
        }
    }
}