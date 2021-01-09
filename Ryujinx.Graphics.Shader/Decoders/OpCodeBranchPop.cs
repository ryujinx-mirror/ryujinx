using Ryujinx.Graphics.Shader.Instructions;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeBranchPop : OpCodeConditional
    {
        public Dictionary<OpCodePush, int> Targets { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeBranchPop(emitter, address, opCode);

        public OpCodeBranchPop(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Targets = new Dictionary<OpCodePush, int>();
        }
    }
}