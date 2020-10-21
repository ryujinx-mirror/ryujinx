using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTld : OpCodeTexture
    {
        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTld(emitter, address, opCode);

        public OpCodeTld(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            HasOffset = opCode.Extract(35);

            IsMultisample = opCode.Extract(50);

            bool isLL = opCode.Extract(55);

            LodMode = isLL
                ? TextureLodMode.LodLevel
                : TextureLodMode.LodZero;
        }
    }
}