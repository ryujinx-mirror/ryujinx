using Ryujinx.Graphics.Shader.Instructions;
using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeFArithImm32 : OpCodeAlu, IOpCodeFArith, IOpCodeImmF
    {
        public RoundingMode RoundingMode => RoundingMode.ToNearest;

        public FPMultiplyScale Scale => FPMultiplyScale.None;

        public bool FlushToZero { get; }
        public bool AbsoluteA   { get; }

        public float Immediate { get; }

        public OpCodeFArithImm32(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            int imm = opCode.Extract(20, 32);

            Immediate = BitConverter.Int32BitsToSingle(imm);

            SetCondCode = opCode.Extract(52);
            AbsoluteA   = opCode.Extract(54);
            FlushToZero = opCode.Extract(55);

            Saturate = false;
        }
    }
}