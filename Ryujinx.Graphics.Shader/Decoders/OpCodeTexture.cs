using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTexture : OpCode, IOpCodeTexture
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rb { get; }

        public bool IsArray { get; }

        public TextureDimensions Dimensions { get; }

        public int ComponentMask { get; }

        public int Immediate { get; }

        public TextureLodMode LodMode { get; protected set; }

        public bool HasOffset       { get; protected set; }
        public bool HasDepthCompare { get; protected set; }
        public bool IsMultisample   { get; protected set; }

        public OpCodeTexture(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);

            IsArray = opCode.Extract(28);

            Dimensions = (TextureDimensions)opCode.Extract(29, 2);

            ComponentMask = opCode.Extract(31, 4);

            Immediate = opCode.Extract(36, 13);

            LodMode = (TextureLodMode)opCode.Extract(55, 3);
        }
    }
}