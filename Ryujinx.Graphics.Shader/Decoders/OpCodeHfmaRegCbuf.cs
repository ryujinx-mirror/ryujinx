using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeHfmaRegCbuf : OpCodeHfma, IOpCodeHfma, IOpCodeRegCbuf
    {
        public int Offset { get; }
        public int Slot   { get; }

        public bool NegateB  { get; }
        public bool NegateC  { get; }
        public bool Saturate { get; }

        public FPHalfSwizzle SwizzleB { get; }
        public FPHalfSwizzle SwizzleC => FPHalfSwizzle.FP32;

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeHfmaRegCbuf(emitter, address, opCode);

        public OpCodeHfmaRegCbuf(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Offset = opCode.Extract(20, 14);
            Slot   = opCode.Extract(34, 5);

            NegateC  = opCode.Extract(51);
            Saturate = opCode.Extract(52);

            SwizzleB = (FPHalfSwizzle)opCode.Extract(53, 2);

            NegateB = opCode.Extract(56);
        }
    }
}