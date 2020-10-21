using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeIpa : OpCodeAluReg
    {
        public int AttributeOffset { get; }

        public InterpolationMode Mode { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeIpa(emitter, address, opCode);

        public OpCodeIpa(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            AttributeOffset = opCode.Extract(28, 10);

            Saturate = opCode.Extract(51);

            Mode = (InterpolationMode)opCode.Extract(54, 2);
        }
    }
}