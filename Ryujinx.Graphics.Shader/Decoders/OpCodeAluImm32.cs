using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAluImm32 : OpCodeAlu, IOpCodeImm
    {
        public int Immediate { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAluImm32(emitter, address, opCode);

        public OpCodeAluImm32(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = opCode.Extract(20, 32);

            SetCondCode = opCode.Extract(52);
            Extended    = opCode.Extract(53);
            Saturate    = opCode.Extract(54);
        }
    }
}