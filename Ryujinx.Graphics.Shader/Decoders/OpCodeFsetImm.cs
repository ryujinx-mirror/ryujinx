using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFsetImm : OpCodeSet, IOpCodeImmF
    {
        public float Immediate { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeFsetImm(emitter, address, opCode);

        public OpCodeFsetImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeF20Immediate(opCode);
        }
    }
}