using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFArithImm : OpCodeFArith, IOpCodeImmF
    {
        public float Immediate { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeFArithImm(emitter, address, opCode);

        public OpCodeFArithImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeF20Immediate(opCode);
        }
    }
}