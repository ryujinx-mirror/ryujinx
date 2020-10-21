using Ryujinx.Graphics.Shader.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodePush : OpCodeBranch
    {
        public Dictionary<OpCodeBranchPop, Operand> PopOps { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodePush(emitter, address, opCode);

        public OpCodePush(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            PopOps = new Dictionary<OpCodeBranchPop, Operand>();

            Predicate = new Register(RegisterConsts.PredicateTrueIndex, RegisterType.Predicate);

            InvertPredicate = false;

            PushTarget = true;
        }
    }
}