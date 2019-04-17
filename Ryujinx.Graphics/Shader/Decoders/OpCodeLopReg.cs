using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeLopReg : OpCodeLop, IOpCodeReg
    {
        public Register Rb { get; }

        public OpCodeLopReg(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);
        }
    }
}