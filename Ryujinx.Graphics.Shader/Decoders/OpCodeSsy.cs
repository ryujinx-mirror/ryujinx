using Ryujinx.Graphics.Shader.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeSsy : OpCodeBranch
    {
        public Dictionary<OpCodeSync, Operand> Syncs { get; }

        public OpCodeSsy(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Syncs = new Dictionary<OpCodeSync, Operand>();

            Predicate = new Register(RegisterConsts.PredicateTrueIndex, RegisterType.Predicate);

            InvertPredicate = false;
        }
    }
}