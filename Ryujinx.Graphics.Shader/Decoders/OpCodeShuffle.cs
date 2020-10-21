using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeShuffle : OpCode, IOpCodeRd, IOpCodeRa
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rb { get; }
        public Register Rc { get; }

        public int ImmediateB { get; }
        public int ImmediateC { get; }

        public bool IsBImmediate { get; }
        public bool IsCImmediate { get; }

        public ShuffleType ShuffleType { get; }

        public Register Predicate48 { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeShuffle(emitter, address, opCode);

        public OpCodeShuffle(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);
            Rc = new Register(opCode.Extract(39, 8), RegisterType.Gpr);

            ImmediateB = opCode.Extract(20, 5);
            ImmediateC = opCode.Extract(34, 13);

            IsBImmediate = opCode.Extract(28);
            IsCImmediate = opCode.Extract(29);

            ShuffleType = (ShuffleType)opCode.Extract(30, 2);

            Predicate48 = new Register(opCode.Extract(48, 3), RegisterType.Predicate);
        }
    }
}