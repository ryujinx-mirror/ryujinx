using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeHfmaImm2x10 : OpCodeHfma, IOpCodeHfma, IOpCodeImm
    {
        public int Immediate { get; }

        public bool NegateB => false;
        public bool NegateC  { get; }
        public bool Saturate { get; }

        public FPHalfSwizzle SwizzleB => FPHalfSwizzle.FP16;
        public FPHalfSwizzle SwizzleC { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeHfmaImm2x10(emitter, address, opCode);

        public OpCodeHfmaImm2x10(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Immediate = DecoderHelper.Decode2xF10Immediate(opCode);

            NegateC  = opCode.Extract(51);
            Saturate = opCode.Extract(52);

            SwizzleC = (FPHalfSwizzle)opCode.Extract(53, 2);
        }
    }
}