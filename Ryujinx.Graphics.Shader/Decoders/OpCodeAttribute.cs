using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeAttribute : OpCodeAluReg, IOpCodeAttribute
    {
        public int  AttributeOffset { get; }
        public bool Patch           { get; }
        public int  Count           { get; }

        public bool Phys => !Patch && AttributeOffset == 0 && !Ra.IsRZ;
        public bool Indexed => Phys;

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeAttribute(emitter, address, opCode);

        public OpCodeAttribute(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            AttributeOffset = opCode.Extract(20, 10);
            Patch           = opCode.Extract(31);
            Count           = opCode.Extract(47, 2) + 1;
        }
    }
}