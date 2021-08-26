using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAl2p : OpCode, IOpCodeRd, IOpCodeRa
    {
        public Register Rd          { get; }
        public Register Ra          { get; }
        public Register Predicate44 { get; }

        public int Immediate { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAl2p(emitter, address, opCode);

        public OpCodeAl2p(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd          = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra          = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Predicate44 = new Register(opCode.Extract(44, 3), RegisterType.Predicate);

            Immediate = ((int)opCode << 1) >> 21;
        }
    }
}