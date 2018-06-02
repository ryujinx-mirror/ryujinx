using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        private const int TempRegStart = 0x100;

        public static void Ld_A(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrNode[] Opers = GetOperAbuf20(OpCode);

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
            int Type = (int)(OpCode >> 48) & 7;

            if (Type > 5)
            {
                throw new InvalidOperationException();
            }

            int Count = Type == 5 ? 2 : 1;

            for (int Index = 0; Index < Count; Index++)
            {
                ShaderIrOperCbuf OperA = GetOperCbuf36(OpCode);
                ShaderIrOperGpr  OperD = GetOperGpr0  (OpCode);

                OperA.Pos   += Index;
                OperD.Index += Index;

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

            ShaderIrNode OperC = GetOperImm13_36(OpCode);

            for (int Ch = 0; Ch < 4; Ch++)
            {
                ShaderIrOperGpr Dst = new ShaderIrOperGpr(TempRegStart + Ch);

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Texs, Coords[0], Coords[1], OperC, Meta);

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
            EmitTex(Block, OpCode, ShaderIrInst.Texs);
        }

        public static void Tlds(ShaderIrBlock Block, long OpCode)
        {
            EmitTex(Block, OpCode, ShaderIrInst.Txlf);
        }

        private static void EmitTex(ShaderIrBlock Block, long OpCode, ShaderIrInst Inst)
        {
            //TODO: Support other formats.
            ShaderIrNode OperA = GetOperGpr8    (OpCode);
            ShaderIrNode OperB = GetOperGpr20   (OpCode);
            ShaderIrNode OperC = GetOperImm13_36(OpCode);

            bool TwoDests = GetOperGpr28(OpCode).Index != ShaderIrOperGpr.ZRIndex;

            int ChMask;

            switch ((OpCode >> 50) & 7)
            {
                case 0: ChMask = TwoDests ? 0x7 : 0x1; break;
                case 1: ChMask = TwoDests ? 0xb : 0x2; break;
                case 2: ChMask = TwoDests ? 0xd : 0x4; break;
                case 3: ChMask = TwoDests ? 0xe : 0x8; break;
                case 4: ChMask = TwoDests ? 0xf : 0x3; break;

                default: throw new InvalidOperationException();
            }

            for (int Ch = 0; Ch < 4; Ch++)
            {
                ShaderIrOperGpr Dst = new ShaderIrOperGpr(TempRegStart + Ch);

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(Inst, OperA, OperB, OperC, Meta);

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

                ShaderIrOperGpr Dst = (RegInc >> 1) != 0
                    ? GetOperGpr28(OpCode)
                    : GetOperGpr0 (OpCode);

                Dst.Index += RegInc++ & 1;

                if (Dst.Index >= ShaderIrOperGpr.ZRIndex)
                {
                    continue;
                }

                Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Src), OpCode));

                /*if (IsScalar)
                {
                    break;
                }*/
            }
        }

        private static bool IsChannelUsed(int ChMask, int Ch)
        {
            return (ChMask & (1 << Ch)) != 0;
        }
    }
}