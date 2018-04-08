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

        public static void Texs(ShaderIrBlock Block, long OpCode)
        {
            //TODO: Support other formats.
            ShaderIrNode OperA = GetOperGpr8    (OpCode);
            ShaderIrNode OperB = GetOperGpr20   (OpCode);
            ShaderIrNode OperC = GetOperGpr28   (OpCode);
            ShaderIrNode OperD = GetOperImm13_36(OpCode);

            for (int Ch = 0; Ch < 4; Ch++)
            {
                ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Texr + Ch, OperA, OperB, OperD);

                ShaderIrOperGpr Dst = GetOperGpr0(OpCode);

                Dst.Index += Ch;

                Block.AddNode(new ShaderIrAsg(Dst, Op));
            }            
        }
    }
}