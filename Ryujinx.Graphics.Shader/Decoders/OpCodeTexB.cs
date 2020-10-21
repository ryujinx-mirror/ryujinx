using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTexB : OpCodeTex
    {
        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTexB(emitter, address, opCode);

        public OpCodeTexB(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            switch (opCode.Extract(37, 3))
            {
                case 0: LodMode = TextureLodMode.None;      break;
                case 1: LodMode = TextureLodMode.LodZero;   break;
                case 2: LodMode = TextureLodMode.LodBias;   break;
                case 3: LodMode = TextureLodMode.LodLevel;  break;
                case 6: LodMode = TextureLodMode.LodBiasA;  break;
                case 7: LodMode = TextureLodMode.LodLevelA; break;
            }
        }
    }
}