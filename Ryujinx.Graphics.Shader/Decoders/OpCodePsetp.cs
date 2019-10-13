using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodePsetp : OpCodeSet
    {
        public Register Predicate12  { get; }
        public Register Predicate29  { get; }

        public LogicalOperation LogicalOpAB { get; }

        public OpCodePsetp(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Predicate12 = new Register(opCode.Extract(12, 3), RegisterType.Predicate);
            Predicate29 = new Register(opCode.Extract(29, 3), RegisterType.Predicate);

            LogicalOpAB = (LogicalOperation)opCode.Extract(24, 2);
        }
    }
}