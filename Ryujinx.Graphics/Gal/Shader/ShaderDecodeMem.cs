using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
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

        private static int[,] MaskLut = new int[,]
        {
            { ____, ____, ____, ____, ____, ____, ____, ____ },
            { R___, _G__, __B_, ___A, RG__, R__A, _G_A, __BA },
            { R___, _G__, __B_, ___A, RG__, ____, ____, ____ },
            { RGB_, RG_A, R_BA, _GBA, RGBA, ____, ____, ____ }
        };

        public static void Ld_A(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode[] Opers = OpCode.Abuf20();

            //Used by GS
            ShaderIrOperGpr Vertex = OpCode.Gpr39();

            int Index = 0;

            foreach (ShaderIrNode OperA in Opers)
            {
                ShaderIrOperGpr OperD = OpCode.Gpr0();

                OperD.Index += Index++;

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperD, OperA)));
            }
        }

        public static void Ld_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            int CbufPos   = OpCode.Read(22, 0x3fff);
            int CbufIndex = OpCode.Read(36, 0x1f);
            int Type      = OpCode.Read(48, 7);

            if (Type > 5)
            {
                throw new InvalidOperationException();
            }

            ShaderIrOperGpr Temp = ShaderIrOperGpr.MakeTemporary();

            Block.AddNode(new ShaderIrAsg(Temp, OpCode.Gpr8()));

            int Count = Type == 5 ? 2 : 1;

            for (int Index = 0; Index < Count; Index++)
            {
                ShaderIrOperCbuf OperA = new ShaderIrOperCbuf(CbufIndex, CbufPos, Temp);

                ShaderIrOperGpr OperD = OpCode.Gpr0();

                OperA.Pos   += Index;
                OperD.Index += Index;

                if (!OperD.IsValidRegister)
                {
                    break;
                }

                ShaderIrNode Node = OperA;

                if (Type < 4)
                {
                    //This is a 8 or 16 bits type.
                    bool Signed = (Type & 1) != 0;

                    int Size = 8 << (Type >> 1);

                    Node = ExtendTo32(Node, Signed, Size);
                }

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperD, Node)));
            }
        }

        public static void St_A(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode[] Opers = OpCode.Abuf20();

            int Index = 0;

            foreach (ShaderIrNode OperA in Opers)
            {
                ShaderIrOperGpr OperD = OpCode.Gpr0();

                OperD.Index += Index++;

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperA, OperD)));
            }
        }

        public static void Texq(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperD = OpCode.Gpr0();
            ShaderIrNode OperA = OpCode.Gpr8();

            ShaderTexqInfo Info = (ShaderTexqInfo)(OpCode.Read(22, 0x1f));

            ShaderIrMetaTexq Meta0 = new ShaderIrMetaTexq(Info, 0);
            ShaderIrMetaTexq Meta1 = new ShaderIrMetaTexq(Info, 1);

            ShaderIrNode OperC = OpCode.Imm13_36();

            ShaderIrOp Op0 = new ShaderIrOp(ShaderIrInst.Texq, OperA, null, OperC, Meta0);
            ShaderIrOp Op1 = new ShaderIrOp(ShaderIrInst.Texq, OperA, null, OperC, Meta1);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperD, Op0)));
            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OperA, Op1))); //Is this right?
        }

        public static void Tex(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitTex(Block, OpCode, GprHandle: false);
        }

        public static void Tex_B(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitTex(Block, OpCode, GprHandle: true);
        }

        private static void EmitTex(ShaderIrBlock Block, long OpCode, bool GprHandle)
        {
            //TODO: Support other formats.
            ShaderIrOperGpr[] Coords = new ShaderIrOperGpr[2];

            for (int Index = 0; Index < Coords.Length; Index++)
            {
                ShaderIrOperGpr CoordReg = OpCode.Gpr8();

                CoordReg.Index += Index;

                if (!CoordReg.IsValidRegister)
                {
                    CoordReg.Index = ShaderIrOperGpr.ZRIndex;
                }

                Coords[Index] = ShaderIrOperGpr.MakeTemporary(Index);

                Block.AddNode(new ShaderIrAsg(Coords[Index], CoordReg));
            }

            int ChMask = OpCode.Read(31, 0xf);

            ShaderIrNode OperC = GprHandle
                ? (ShaderIrNode)OpCode.Gpr20()
                : (ShaderIrNode)OpCode.Imm13_36();

            ShaderIrInst Inst = GprHandle ? ShaderIrInst.Texb : ShaderIrInst.Texs;

            int RegInc = 0;

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrOperGpr Dst = OpCode.Gpr0();

                Dst.Index += RegInc++;

                if (!Dst.IsValidRegister || Dst.IsConst)
                {
                    continue;
                }

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(Inst, Coords[0], Coords[1], OperC, Meta);

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(Dst, Op)));
            }
        }

        public static void Texs(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitTexs(Block, OpCode, ShaderIrInst.Texs);
        }

        public static void Tlds(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitTexs(Block, OpCode, ShaderIrInst.Txlf);
        }

        private static void EmitTexs(ShaderIrBlock Block, long OpCode, ShaderIrInst Inst)
        {
            //TODO: Support other formats.
            int LutIndex;

            LutIndex  = !OpCode.Gpr0().IsConst  ? 1 : 0;
            LutIndex |= !OpCode.Gpr28().IsConst ? 2 : 0;

            if (LutIndex == 0)
            {
                //Both destination registers are RZ, do nothing.
                return;
            }

            bool Fp16 = !OpCode.Read(59);

            int DstIncrement = 0;

            ShaderIrOperGpr GetDst()
            {
                ShaderIrOperGpr Dst;

                if (Fp16)
                {
                    //FP16 mode, two components are packed on the two
                    //halfs of a 32-bits register, as two half-float values.
                    int HalfPart = DstIncrement & 1;

                    switch (LutIndex)
                    {
                        case 1: Dst = OpCode.GprHalf0(HalfPart);  break;
                        case 2: Dst = OpCode.GprHalf28(HalfPart); break;
                        case 3: Dst = (DstIncrement >> 1) != 0
                            ? OpCode.GprHalf28(HalfPart)
                            : OpCode.GprHalf0(HalfPart); break;

                        default: throw new InvalidOperationException();
                    }
                }
                else
                {
                    //32-bits mode, each component uses one register.
                    //Two components uses two consecutive registers.
                    switch (LutIndex)
                    {
                        case 1: Dst = OpCode.Gpr0();  break;
                        case 2: Dst = OpCode.Gpr28(); break;
                        case 3: Dst = (DstIncrement >> 1) != 0
                            ? OpCode.Gpr28()
                            : OpCode.Gpr0(); break;

                        default: throw new InvalidOperationException();
                    }

                    Dst.Index += DstIncrement & 1;
                }

                DstIncrement++;

                return Dst;
            }

            int ChMask = MaskLut[LutIndex, OpCode.Read(50, 7)];

            if (ChMask == 0)
            {
                //All channels are disabled, do nothing.
                return;
            }

            ShaderIrNode OperC = OpCode.Imm13_36();

            ShaderIrOperGpr Coord0 = ShaderIrOperGpr.MakeTemporary(0);
            ShaderIrOperGpr Coord1 = ShaderIrOperGpr.MakeTemporary(1);

            Block.AddNode(new ShaderIrAsg(Coord0, OpCode.Gpr8()));
            Block.AddNode(new ShaderIrAsg(Coord1, OpCode.Gpr20()));

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(Inst, Coord0, Coord1, OperC, Meta);

                ShaderIrOperGpr Dst = GetDst();

                if (Dst.IsValidRegister && !Dst.IsConst)
                {
                    Block.AddNode(OpCode.PredNode(new ShaderIrAsg(Dst, Op)));
                }
            }
        }

        private static bool IsChannelUsed(int ChMask, int Ch)
        {
            return (ChMask & (1 << Ch)) != 0;
        }
    }
}