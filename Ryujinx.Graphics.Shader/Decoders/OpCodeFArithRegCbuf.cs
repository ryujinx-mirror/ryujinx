using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFArithRegCbuf : OpCodeFArith, IOpCodeRegCbuf
    {
        public int Offset { get; }
        public int Slot   { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeFArithRegCbuf(emitter, address, opCode);

        public OpCodeFArithRegCbuf(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Offset = opCode.Extract(20, 14);
            Slot   = opCode.Extract(34, 5);
        }
    }
}