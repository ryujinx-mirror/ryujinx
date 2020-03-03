using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeDArithImm : OpCodeFArith, IOpCodeImmF
    {
        public float Immediate { get; }

        public OpCodeDArithImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeD20Immediate(opCode);
        }
    }
}