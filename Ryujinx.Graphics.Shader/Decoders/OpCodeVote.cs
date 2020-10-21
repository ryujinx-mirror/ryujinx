using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeVote : OpCode, IOpCodeRd, IOpCodePredicate39
    {
        public Register Rd          { get; }
        public Register Predicate39 { get; }
        public Register Predicate45 { get; }

        public VoteOp VoteOp { get; }

        public bool InvertP { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeVote(emitter, address, opCode);

        public OpCodeVote(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd          = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Predicate39 = new Register(opCode.Extract(39, 3), RegisterType.Predicate);
            Predicate45 = new Register(opCode.Extract(45, 3), RegisterType.Predicate);

            InvertP = opCode.Extract(42);

            VoteOp = (VoteOp)opCode.Extract(48, 2);
        }
    }
}