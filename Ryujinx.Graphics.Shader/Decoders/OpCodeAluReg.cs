using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAluReg : OpCodeAlu, IOpCodeReg
    {
        public Register Rb { get; protected set; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAluReg(emitter, address, opCode);

        public OpCodeAluReg(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);
        }
    }
}