using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCode
    {
        public InstEmitter Emitter { get; }

        public ulong Address   { get; }
        public long  RawOpCode { get; }

        public Register Predicate { get; protected set; }

        public bool InvertPredicate { get; protected set; }

        // When inverted, the always true predicate == always false.
        public bool NeverExecute => Predicate.Index == RegisterConsts.PredicateTrueIndex && InvertPredicate;

        public static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCode(emitter, address, opCode);

        public OpCode(InstEmitter emitter, ulong address, long opCode)
        {
            Emitter   = emitter;
            Address   = address;
            RawOpCode = opCode;

            Predicate = new Register(opCode.Extract(16, 3), RegisterType.Predicate);

            InvertPredicate = opCode.Extract(19);
        }
    }
}