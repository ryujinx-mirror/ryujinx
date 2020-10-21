using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeMemory : OpCode, IOpCodeRd, IOpCodeRa
    {
        public Register Rd { get; }
        public Register Ra { get; }

        public int Offset { get; }

        public bool Extended { get; }

        public IntegerSize Size { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeMemory(emitter, address, opCode);

        public OpCodeMemory(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0, 8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8, 8), RegisterType.Gpr);

            Offset = (opCode.Extract(20, 24) << 8) >> 8;

            Extended = opCode.Extract(45);

            Size = (IntegerSize)opCode.Extract(48, 3);
        }
    }
}