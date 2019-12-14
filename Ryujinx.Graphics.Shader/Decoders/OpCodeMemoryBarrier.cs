using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeMemoryBarrier : OpCode
    {
        public BarrierLevel Level { get; }

        public OpCodeMemoryBarrier(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Level = (BarrierLevel)opCode.Extract(8, 2);
        }
    }
}