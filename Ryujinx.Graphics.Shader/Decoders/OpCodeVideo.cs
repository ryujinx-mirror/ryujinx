using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeVideo : OpCode, IOpCodeRd, IOpCodeRa, IOpCodeRc
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rc { get; }

        public bool SetCondCode { get; protected set; }
        public bool Saturate    { get; protected set; }

        public OpCodeVideo(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rc = new Register(opCode.Extract(39, 8), RegisterType.Gpr);

            SetCondCode = opCode.Extract(47);
            Saturate    = opCode.Extract(55);
        }
    }
}