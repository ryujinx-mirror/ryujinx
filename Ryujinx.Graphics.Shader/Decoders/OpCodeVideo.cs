using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeVideo : OpCode, IOpCodeRd, IOpCodeRa, IOpCodeRc
    {
        public Register Rd { get; }
        public Register Ra { get; }
        public Register Rb { get; }
        public Register Rc { get; }

        public int Immediate { get; }

        public int RaSelection { get; }
        public int RbSelection { get; }

        public bool SetCondCode { get; }

        public bool HasRb { get; }

        public VideoType RaType { get; }
        public VideoType RbType { get; }

        public VideoPostOp PostOp { get; }

        public bool DstSigned { get; }
        public bool Saturate  { get; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeVideo(emitter, address, opCode);

        public OpCodeVideo(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb = new Register(opCode.Extract(20, 8), RegisterType.Gpr);
            Rc = new Register(opCode.Extract(39, 8), RegisterType.Gpr);

            RaSelection = opCode.Extract(36, 2);
            RbSelection = opCode.Extract(28, 2);

            RaType = opCode.Extract(37, 2) switch
            {
                2 => VideoType.U16,
                3 => VideoType.U32,
                _ => VideoType.U8
            };

            RbType = opCode.Extract(29, 2) switch
            {
                2 => VideoType.U16,
                3 => VideoType.U32,
                _ => VideoType.U8
            };

            if (opCode.Extract(48))
            {
                RaType |= VideoType.Signed;
            }

            if (!opCode.Extract(50))
            {
                // Immediate variant.
                Immediate = opCode.Extract(16, 20);

                RbType = opCode.Extract(49) ? VideoType.S16 : VideoType.U16;

                if (RbType == VideoType.S16)
                {
                    Immediate = (Immediate << 12) >> 12;
                }
            }
            else if (opCode.Extract(49))
            {
                RbType |= VideoType.Signed;
            }

            if (RaType == VideoType.U16)
            {
                RaSelection &= 1;
            }
            else if (RaType == VideoType.U32)
            {
                RaSelection = 0;
            }

            if (RbType == VideoType.U16)
            {
                RbSelection &= 1;
            }
            else if (RbType == VideoType.U32)
            {
                RbSelection = 0;
            }

            SetCondCode = opCode.Extract(47);

            HasRb = opCode.Extract(50);

            PostOp = (VideoPostOp)opCode.Extract(51, 3);

            DstSigned = opCode.Extract(54);
            Saturate  = opCode.Extract(55);
        }
    }
}