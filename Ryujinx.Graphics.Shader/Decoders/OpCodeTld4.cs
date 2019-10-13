using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTld4 : OpCodeTexture
    {
        public TextureGatherOffset Offset { get; }

        public int GatherCompIndex { get; }

        public OpCodeTld4(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            HasDepthCompare = opCode.Extract(50);

            Offset = (TextureGatherOffset)opCode.Extract(54, 2);

            GatherCompIndex = opCode.Extract(56, 2);
        }
    }
}