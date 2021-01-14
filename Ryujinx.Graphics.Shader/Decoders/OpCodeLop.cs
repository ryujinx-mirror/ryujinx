using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeLop : OpCodeAlu, IOpCodeLop
    {
        public bool InvertA { get; protected set; }
        public bool InvertB { get; protected set; }

        public LogicalOperation LogicalOp { get; }

        public Register Predicate48 { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeLop(emitter, address, opCode);

        public OpCodeLop(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            InvertA = opCode.Extract(39);
            InvertB = opCode.Extract(40);

            LogicalOp = (LogicalOperation)opCode.Extract(41, 2);

            Predicate48 = new Register(opCode.Extract(48, 3), RegisterType.Predicate);
        }
    }
}