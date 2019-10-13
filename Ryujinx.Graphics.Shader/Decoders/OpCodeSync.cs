using Ryujinx.Graphics.Shader.Instructions;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeSync : OpCode
    {
        public Dictionary<OpCodeSsy, int> Targets { get; }

        public OpCodeSync(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Targets = new Dictionary<OpCodeSsy, int>();
        }
    }
}