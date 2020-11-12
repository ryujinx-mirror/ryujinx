using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeImage : OpCodeTextureBase
    {
        public Register Ra { get; }
        public Register Rb { get; }
        public Register Rc { get; }

        public ImageComponents Components { get; }
        public IntegerSize     Size       { get; }

        public bool ByteAddress { get; }

        public ImageDimensions Dimensions { get; }

        public bool UseComponents { get; }
        public bool IsBindless    { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeImage(emitter, address, opCode);

        public OpCodeImage(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Rc = new Register(opCode.Extract(39, 8), RegisterType.Gpr);

            UseComponents = !opCode.Extract(52);

            if (UseComponents)
            {
                Components = (ImageComponents)opCode.Extract(20, 4);
            }
            else
            {
                Size = (IntegerSize)opCode.Extract(20, 4);
            }

            ByteAddress = opCode.Extract(23);

            Dimensions = (ImageDimensions)opCode.Extract(33, 3);

            IsBindless = !opCode.Extract(51);
        }
    }
}