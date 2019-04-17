using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFsetImm : OpCodeSet, IOpCodeImmF
    {
        public float Immediate { get; }

        public OpCodeFsetImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeF20Immediate(opCode);
        }
    }
}