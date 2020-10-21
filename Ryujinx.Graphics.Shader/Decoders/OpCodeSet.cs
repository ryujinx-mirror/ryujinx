using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeSet : OpCodeAlu
    {
        public Register Predicate0 { get; }
        public Register Predicate3 { get; }

        public bool NegateP { get; }

        public LogicalOperation LogicalOp { get; }

        public bool FlushToZero { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeSet(emitter, address, opCode);

        public OpCodeSet(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Predicate0 = new Register(opCode.Extract(0, 3), RegisterType.Predicate);
            Predicate3 = new Register(opCode.Extract(3, 3), RegisterType.Predicate);

            LogicalOp = (LogicalOperation)opCode.Extract(45, 2);

            FlushToZero = opCode.Extract(47);
        }
    }
}