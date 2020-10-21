using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeHfma : OpCode, IOpCodeRd, IOpCodeRa, IOpCodeRc
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rc { get; protected set; }

        public FPHalfSwizzle SwizzleA { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeHfma(emitter, address, opCode);

        public OpCodeHfma(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rc = new Register(opCode.Extract(39, 8), RegisterType.Gpr);

            SwizzleA = (FPHalfSwizzle)opCode.Extract(47, 2);
        }
    }
}