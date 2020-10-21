using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAlu : OpCode, IOpCodeAlu, IOpCodeRc
    {
        public Register Rd          { get; }
        public Register Ra          { get; }
        public Register Rc          { get; }
        public Register Predicate39 { get; }

        public int ByteSelection { get; }

        public bool InvertP     { get; }
        public bool Extended    { get; protected set; }
        public bool SetCondCode { get; protected set; }
        public bool Saturate    { get; protected set; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAlu(emitter, address, opCode);

        public OpCodeAlu(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd          = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra          = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rc          = new Register(opCode.Extract(39, 8), RegisterType.Gpr);
            Predicate39 = new Register(opCode.Extract(39, 3), RegisterType.Predicate);

            ByteSelection = opCode.Extract(41, 2);

            InvertP     = opCode.Extract(42);
            Extended    = opCode.Extract(43);
            SetCondCode = opCode.Extract(47);
            Saturate    = opCode.Extract(50);
        }
    }
}