using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeHsetImm2x10 : OpCodeSet, IOpCodeImm
    {
        public int Immediate { get; }

        public OpCodeHsetImm2x10(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.Decode2xF10Immediate(opCode);
        }
    }
}