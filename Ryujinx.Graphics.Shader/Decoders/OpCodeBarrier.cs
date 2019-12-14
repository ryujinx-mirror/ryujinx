using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeBarrier : OpCode
    {
        public BarrierMode Mode { get; }

        public OpCodeBarrier(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Mode = (BarrierMode)((opCode >> 32) & 0x9b);
        }
    }
}