using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeSuatom : OpCodeTextureBase
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rb { get; }
        public Register Rc { get; }

        public ReductionType Type { get; }
        public AtomicOp AtomicOp { get; }
        public ImageDimensions Dimensions { get; }
        public ClampMode ClampMode { get; }

        public bool ByteAddress { get; }
        public bool UseType { get; }
        public bool IsBindless { get; }

        public bool CompareAndSwap { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeSuatom(emitter, address, opCode);

        public OpCodeSuatom(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);
            Rc = new Register(opCode.Extract(39, 8), RegisterType.Gpr);

            bool supportsBindless = opCode.Extract(54);

            Type = (ReductionType)opCode.Extract(supportsBindless ? 36 : 51, 3);
            ByteAddress = opCode.Extract(28);
            AtomicOp = (AtomicOp)opCode.Extract(29, 4); // Only useful if CAS is not true.
            Dimensions = (ImageDimensions)opCode.Extract(33, 3);
            ClampMode = (ClampMode)opCode.Extract(49, 2);

            IsBindless = supportsBindless && !opCode.Extract(51);
            UseType = !supportsBindless || opCode.Extract(52);

            CompareAndSwap = opCode.Extract(55);
        }
    }
}
