using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAluRegCbuf : OpCodeAluReg, IOpCodeRegCbuf
    {
        public int Offset { get; }
        public int Slot   { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAluRegCbuf(emitter, address, opCode);

        public OpCodeAluRegCbuf(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Offset = opCode.Extract(20, 14);
            Slot   = opCode.Extract(34, 5);

            Rb = new Register(opCode.Extract(39, 8), RegisterType.Gpr);
        }
    }
}