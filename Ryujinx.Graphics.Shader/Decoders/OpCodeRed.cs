using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeRed : OpCode, IOpCodeRd, IOpCodeRa
    {
        public Register Rd { get; }
        public Register Ra { get; }

        public AtomicOp AtomicOp { get; }

        public ReductionType Type { get; }

        public int Offset { get; }

        public bool Extended { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeRed(emitter, address, opCode);

        public OpCodeRed(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0, 8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8, 8), RegisterType.Gpr);

            Type = (ReductionType)opCode.Extract(20, 3);

            AtomicOp = (AtomicOp)opCode.Extract(23, 3);

            Offset = (opCode.Extract(28, 20) << 12) >> 12;

            Extended = opCode.Extract(48);
        }
    }
}