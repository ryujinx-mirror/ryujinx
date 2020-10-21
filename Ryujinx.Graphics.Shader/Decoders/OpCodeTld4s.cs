using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTld4s : OpCodeTextureScalar
    {
        public bool HasDepthCompare { get; }
        public bool HasOffset       { get; }

        public int GatherCompIndex { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTld4s(emitter, address, opCode);

        public OpCodeTld4s(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            HasDepthCompare = opCode.Extract(50);
            HasOffset       = opCode.Extract(51);

            GatherCompIndex = opCode.Extract(52, 2);

            IsFp16 = opCode.Extract(55);

            ComponentMask = Rd1.IsRZ ? 3 : 0xf;
        }
    }
}