using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeSetCbuf : OpCodeSet, IOpCodeCbuf
    {
        public int Offset { get; }
        public int Slot   { get; }

        public OpCodeSetCbuf(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Offset = opCode.Extract(20, 14);
            Slot   = opCode.Extract(34, 5);
        }
    }
}