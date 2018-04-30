using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
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

            for (int Ch = 0; Ch < 4; Ch++)
            {
                //Assign it to a temp because the destination registers
                //may be used as texture coord input aswell.
                ShaderIrOperGpr Dst = new ShaderIrOperGpr(0x100 + Ch);

                ShaderIrMetaTex Meta = new ShaderIrMetaTex(Ch);

                ShaderIrOp Op = new ShaderIrOp(Inst, OperA, OperB, OperC, Meta);

                Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Op), OpCode));
            }

            for (int Ch = 0; Ch < 4; Ch++)
            {
                ShaderIrOperGpr Src = new ShaderIrOperGpr(0x100 + Ch);

                ShaderIrOperGpr Dst = (Ch >> 1) != 0
                    ? GetOperGpr28(OpCode)
                    : GetOperGpr0 (OpCode);

                Dst.Index += Ch & 1;

                if (Dst.Index >= ShaderIrOperGpr.ZRIndex)
                {
                    continue;
                }

                Block.AddNode(GetPredNode(new ShaderIrAsg(Dst, Src), OpCode));
            }
        }
    }
}