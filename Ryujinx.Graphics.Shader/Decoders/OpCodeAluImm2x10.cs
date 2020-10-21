using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAluImm2x10 : OpCodeAlu, IOpCodeImm
    {
        public int Immediate { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAluImm2x10(emitter, address, opCode);

        public OpCodeAluImm2x10(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.Decode2xF10Immediate(opCode);
        }
    }
}