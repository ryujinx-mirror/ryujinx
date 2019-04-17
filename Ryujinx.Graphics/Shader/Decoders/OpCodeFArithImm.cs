using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFArithImm : OpCodeFArith, IOpCodeImmF
    {
        public float Immediate { get; }

        public OpCodeFArithImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeF20Immediate(opCode);
        }
    }
}