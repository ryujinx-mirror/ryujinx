using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeSetImm : OpCodeSet, IOpCodeImm
    {
        public int Immediate { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeSetImm(emitter, address, opCode);

        public OpCodeSetImm(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.DecodeS20Immediate(opCode);
        }
    }
}