// ReSharper disable InconsistentNaming
using Ryujinx.Graphics.Shader.Instructions;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class OpCodeTextureScalar : OpCodeTextureBase
    {
#region "Component mask LUT"
        private const int ____ = 0x0;
        private const int R___ = 0x1;
        private const int _G__ = 0x2;
        private const int RG__ = 0x3;
        private const int __B_ = 0x4;
        private const int RGB_ = 0x7;
        private const int ___A = 0x8;
        private const int R__A = 0x9;
        private const int _G_A = 0xa;
        private const int RG_A = 0xb;
        private const int __BA = 0xc;
        private const int R_BA = 0xd;
        private const int _GBA = 0xe;
        private const int RGBA = 0xf;

        private static int[,] _maskLut = new int[,]
        {
            { R___, _G__, __B_, ___A, RG__, R__A, _G_A, __BA },
            { RGB_, RG_A, R_BA, _GBA, RGBA, ____, ____, ____ }
        };
#endregion

        public Register Rd0 { get; }
        public Register Ra  { get; }
        public Register Rb  { get; }
        public Register Rd1 { get; }

        public int ComponentMask { get; protected set; }

        protected int RawType;

        public bool IsFp16 { get; protected set; }

        public new static OpCode Create(InstEmitter emitter, ulong address, long opCode) => new OpCodeTextureScalar(emitter, address, opCode);

        public OpCodeTextureScalar(InstEmitter emitter, ulong address, long opCode) : base(emitter, address, opCode)
        {
            Rd0 = new Register(opCode.Extract(0,  8), RegisterType.Gpr);
            Ra  = new Register(opCode.Extract(8,  8), RegisterType.Gpr);
            Rb  = new Register(opCode.Extract(20, 8), RegisterType.Gpr);
            Rd1 = new Register(opCode.Extract(28, 8), RegisterType.Gpr);

            int compSel = opCode.Extract(50, 3);

            RawType = opCode.Extract(53, 4);

            IsFp16 = !opCode.Extract(59);

            ComponentMask = _maskLut[Rd1.IsRZ ? 0 : 1, compSel];
        }
    }
}