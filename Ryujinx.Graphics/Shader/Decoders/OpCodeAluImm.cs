using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAluImm : OpCodeAlu, IOpCodeImm
    {
        public int Immediate { get; }

        public OpCodeAluImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeS20Immediate(opCode);
        }
    }
}