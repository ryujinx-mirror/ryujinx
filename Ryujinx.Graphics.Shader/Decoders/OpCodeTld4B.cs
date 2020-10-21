using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTld4B : OpCodeTexture, IOpCodeTld4
    {
        public TextureGatherOffset Offset { get; }

        public int GatherCompIndex { get; }

        public bool Bindless => true;

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTld4B(emitter, address, opCode);

        public OpCodeTld4B(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            HasDepthCompare = opCode.Extract(50);

            Offset = (TextureGatherOffset)opCode.Extract(36, 2);

            GatherCompIndex = opCode.Extract(38, 2);
        }
    }
}