using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFArith : OpCodeAlu, IOpCodeFArith
    {
        public RoundingMode RoundingMode { get; }

        public FPMultiplyScale Scale { get; }

        public bool FlushToZero { get; }
        public bool AbsoluteA   { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeFArith(emitter, address, opCode);

        public OpCodeFArith(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            RoundingMode = (RoundingMode)opCode.Extract(39, 2);

            Scale = (FPMultiplyScale)opCode.Extract(41, 3);

            FlushToZero = opCode.Extract(44);
            AbsoluteA   = opCode.Extract(46);
        }
    }
}