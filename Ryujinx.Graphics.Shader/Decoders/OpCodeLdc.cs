using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeLdc : OpCode, IOpCodeRd, IOpCodeRa, IOpCodeCbuf
    {
        public Register Rd { get; }
        public Register Ra { get; }

        public int Offset { get; }
        public int Slot   { get; }

        public IntegerSize Size { get; }

        public OpCodeLdc(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0, 8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8, 8), RegisterType.Gpr);

            Offset = opCode.Extract(22, 14);
            Slot   = opCode.Extract(36, 5);

            Size = (IntegerSize)opCode.Extract(48, 3);
        }
    }
}