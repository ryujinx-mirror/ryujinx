using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeLopImm : OpCodeLop, IOpCodeImm
    {
        public int Immediate { get; }

        public OpCodeLopImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeS20Immediate(opCode);
        }
    }
}