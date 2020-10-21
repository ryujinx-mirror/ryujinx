using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeHfmaReg : OpCodeHfma, IOpCodeHfma, IOpCodeReg
    {
        public Register Rb { get; }

        public bool NegateB  { get; }
        public bool NegateC  { get; }
        public bool Saturate { get; }

        public FPHalfSwizzle SwizzleB { get; }
        public FPHalfSwizzle SwizzleC { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeHfmaReg(emitter, address, opCode);

        public OpCodeHfmaReg(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);

            SwizzleB = (FPHalfSwizzle)opCode.Extract(28, 2);

            NegateC  = opCode.Extract(30);
            NegateB  = opCode.Extract(31);
            Saturate = opCode.Extract(32);

            SwizzleC = (FPHalfSwizzle)opCode.Extract(35, 2);
        }
    }
}