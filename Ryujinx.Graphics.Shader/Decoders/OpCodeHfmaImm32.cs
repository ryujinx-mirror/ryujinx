using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeHfmaImm32 : OpCodeHfma, IOpCodeHfma, IOpCodeImm
    {
        public int Immediate { get; }

        public bool NegateB => false;
        public bool NegateC { get; }
        public bool Saturate => false;

        public FPHalfSwizzle SwizzleB => FPHalfSwizzle.FP16;
        public FPHalfSwizzle SwizzleC => FPHalfSwizzle.FP16;

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeHfmaImm32(emitter, address, opCode);

        public OpCodeHfmaImm32(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = opCode.Extract(20, 32);

            NegateC = opCode.Extract(52);

            Rc = Rd;
        }
    }
}