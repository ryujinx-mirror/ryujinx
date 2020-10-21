using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodePset : OpCodeSet
    {
        public Register Predicate12  { get; }
        public Register Predicate29  { get; }

        public bool InvertA { get; }
        public bool InvertB { get; }

        public LogicalOperation LogicalOpAB { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodePset(emitter, address, opCode);

        public OpCodePset(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Predicate12 = new Register(opCode.Extract(12, 3), RegisterType.Predicate);
            Predicate29 = new Register(opCode.Extract(29, 3), RegisterType.Predicate);

            InvertA = opCode.Extract(15);
            InvertB = opCode.Extract(32);

            LogicalOpAB = (LogicalOperation)opCode.Extract(24, 2);
        }
    }
}