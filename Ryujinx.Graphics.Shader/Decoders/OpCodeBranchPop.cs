using Ryujinx.Graphics.Shader.Instructions;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeBranchPop : OpCode
    {
        public Dictionary<OpCodePush, int> Targets { get; }

        public OpCodeBranchPop(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Targets = new Dictionary<OpCodePush, int>();
        }
    }
}