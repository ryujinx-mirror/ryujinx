using Ryujinx.Graphics.Shader.Instructions;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeBranchIndir : OpCode
    {
        public HashSet<Block> PossibleTargets { get; }

        public Register Ra { get; }

        public int Offset { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeBranchIndir(emitter, address, opCode);

        public OpCodeBranchIndir(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            PossibleTargets = new HashSet<Block>();

            Ra = new Register(opCode.Extract(8, 8), RegisterType.Gpr);

            Offset = ((int)(opCode >> 20) << 8) >> 8;
        }
    }
}