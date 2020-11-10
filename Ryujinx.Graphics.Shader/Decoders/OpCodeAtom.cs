using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAtom : OpCode, IOpCodeRd, IOpCodeRa, IOpCodeReg
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rb { get; }

        public bool Extended { get; }

        public AtomicOp AtomicOp { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAtom(emitter, address, opCode);

        public OpCodeAtom(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);

            Extended = opCode.Extract(48);

            AtomicOp = (AtomicOp)opCode.Extract(52, 4);
        }
    }
}