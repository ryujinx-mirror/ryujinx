using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        private const int TempRegStart = 0x100;

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

        public static void Ld_A(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrNode[] Opers = GetOperAbuf20(OpCode);

            //Used by GS
            ShaderIrOperGpr Vertex = GetOperGpr39(OpCode);

            int Index = 0;

            foreach (ShaderIrNode OperA in Opers)
            {
                ShaderIrOperGpr OperD = GetOperGpr0(OpCode);

                OperD.Index += Index++;

                Block.AddNode(GetPredNode(new ShaderIrAsg(OperD, OperA), OpCode));
            }
        }

        public static void Ld_C(ShaderIrBlock Block, long OpCode)
        {
            int CbufPos   = (int)(OpCode >> 22) & 0x3fff;
            int CbufIndex = (int)(OpCode >> 36) & 0x1f;
            int Type      = (int)(OpCode >> 48) & 7;

            if (Type > 5)
            {
                throw new InvalidOperationException();
            }

            ShaderIrOperGpr Temp = ShaderIrOperGpr.MakeTemporary();

            Block.AddNode(new ShaderIrAsg(Temp, GetOperGpr8(OpCode)));

            int Count = Type == 5 ? 2 : 1;

            for (int Index = 0; Index < Count; Index++)
            {
                ShaderIrOperCbuf OperA = new ShaderIrOperCbuf(CbufIndex, CbufPos, Temp);

                ShaderIrOperGpr OperD = GetOperGpr0(OpCode);

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

                Block.AddNode(GetPredNode(new ShaderIrAsg(OperD, Node), OpCode));
            }
        }

        public static void St_A(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrNode[] Opers = GetOperAbuf20(OpCode);

            int Index = 0;

            foreach (ShaderIrNode OperA in Opers)
            {
                ShaderIrOperGpr OperD = GetOperGpr0(OpCode);

                OperD.Index += Index++;

                Block.AddNode(GetPredNode(new ShaderIrAsg(OperA, OperD), OpCode));
            }
        }

        public static void Texq(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrNode OperD = GetOperGpr0(OpCode);
            ShaderIrNode OperA = GetOperGpr8(OpCode);

            ShaderTexqInfo Info = (ShaderTexqInfo)((OpCode >> 22) & 0x1f);

            ShaderIrMetaTexq Meta0 = new ShaderIrMetaTexq(Info, 0);
            ShaderIrMetaTexq Meta1 = new ShaderIrMetaTexq(Info, 1);

            ShaderIrNode OperC = GetOperImm13_36(OpCode);

            ShaderIrOp Op0 = new ShaderIrOp(ShaderIrInst.Texq, OperA, null, OperC, Meta0);
            ShaderIrOp Op1 = new ShaderIrOp(ShaderIrInst.Texq, OperA, null, OperC, Meta1);

            Block.AddNode(GetPredNode(new ShaderIrAsg(OperD, Op0), OpCode));
            Block.AddNode(GetPredNode(new ShaderIrAsg(OperA, Op1), OpCode)); //Is this right?
        }

        public static void Tex(ShaderIrBlock Block, long OpCode)
        {
            EmitTex(Block, OpCode, GprHandle: false);
        }

        public static void Tex_B(ShaderIrBlock Block, long OpCode)
        {
            EmitTex(Block, OpCode, GprHandle: true);
        }

        private static void EmitTex(ShaderIrBlock Block, long OpCode, bool GprHandle)
        {
            //TODO: Support other formats.
            ShaderIrOperGpr[] Coords = new ShaderIrOperGpr[2];

            for (int Index = 0; Index < Coords.Length; Index++)
            {
                Coords[Index] = GetOperGpr8(OpCode);

                Coords[Index].Index += Index;

                if (Coords[Index].Index > ShaderIrOperGpr.ZRIndex)
                {
                    Coords[Index].Index = ShaderIrOperGpr.ZRIndex;
                }
            }

            int ChMask = (int)(OpCode >> 31) & 0xf;

            ShaderIrNode OperC = GprHandle
                ? (ShaderIrNode)GetOperGpr20   (OpCode)
                : (ShaderIrNode)GetOperImm13_36(OpCode);

            ShaderIrInst Inst = GprHandle ? ShaderIrInst.Texb : ShaderIrInst.Texs;

            for (int Ch = 0; Ch < 4; Ch++)
            {
                ShaderIrOperGpr Dst = new ShaderIrOperGpr(TempRegStart + Ch);

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(Inst, Coords[0], Coords[1], OperC, Meta);

                Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Op), OpCode));
            }

            int RegInc = 0;

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrOperGpr Src = new ShaderIrOperGpr(TempRegStart + Ch);

                ShaderIrOperGpr Dst = GetOperGpr0(OpCode);

                Dst.Index += RegInc++;

                if (Dst.Index >= ShaderIrOperGpr.ZRIndex)
                {
                    continue;
                }

                Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Src), OpCode));
            }
        }

        public static void Texs(ShaderIrBlock Block, long OpCode)
        {
            EmitTexs(Block, OpCode, ShaderIrInst.Texs);
        }

        public static void Tlds(ShaderIrBlock Block, long OpCode)
        {
            EmitTexs(Block, OpCode, ShaderIrInst.Txlf);
        }

        private static void EmitTexs(ShaderIrBlock Block, long OpCode, ShaderIrInst Inst)
        {
            //TODO: Support other formats.
            ShaderIrNode OperA = GetOperGpr8    (OpCode);
            ShaderIrNode OperB = GetOperGpr20   (OpCode);
            ShaderIrNode OperC = GetOperImm13_36(OpCode);

            int LutIndex;

            LutIndex  = GetOperGpr0 (OpCode).Index != ShaderIrOperGpr.ZRIndex ? 1 : 0;
            LutIndex |= GetOperGpr28(OpCode).Index != ShaderIrOperGpr.ZRIndex ? 2 : 0;

            if (LutIndex == 0)
            {
                //Both registers are RZ, color is not written anywhere.
                //So, the intruction is basically a no-op.
                return;
            }

            int ChMask = MaskLut[LutIndex, (OpCode >> 50) & 7];

            for (int Ch = 0; Ch < 4; Ch++)
            {
                ShaderIrOperGpr Dst = new ShaderIrOperGpr(TempRegStart + Ch);

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(Inst, OperA, OperB, OperC, Meta);

                Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Op), OpCode));
            }

            int RegInc = 0;

            ShaderIrOperGpr GetDst()
            {
                ShaderIrOperGpr Dst;

                switch (LutIndex)
                {
                    case 1: Dst = GetOperGpr0 (OpCode); break;
                    case 2: Dst = GetOperGpr28(OpCode); break;
                    case 3: Dst = (RegInc >> 1) != 0
                        ? GetOperGpr28(OpCode)
                        : GetOperGpr0 (OpCode); break;

                    default: throw new InvalidOperationException();
                }

                Dst.Index += RegInc++ & 1;

                return Dst;
            }

            for (int Ch = 0; Ch < 4; Ch++)
            {
                if (!IsChannelUsed(ChMask, Ch))
                {
                    continue;
                }

                ShaderIrOperGpr Src = new ShaderIrOperGpr(TempRegStart + Ch);

                ShaderIrOperGpr Dst = GetDst();

                if (Dst.Index != ShaderIrOperGpr.ZRIndex)
                {
                    Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Src), OpCode));
                }
            }
        }

        private static bool IsChannelUsed(int ChMask, int Ch)
        {
            return (ChMask & (1 << Ch)) != 0;
        }
    }
}