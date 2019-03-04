namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Out_R(ShaderIrBlock block, long opCode, int position)
        {
            //TODO: Those registers have to be used for something
            ShaderIrOperGpr gpr0  = opCode.Gpr0();
            ShaderIrOperGpr gpr8  = opCode.Gpr8();
            ShaderIrOperGpr gpr20 = opCode.Gpr20();

            int type = opCode.Read(39, 3);

            if ((type & 1) != 0)
            {
                block.AddNode(opCode.PredNode(new ShaderIrOp(ShaderIrInst.Emit)));
            }

            if ((type & 2) != 0)
            {
                block.AddNode(opCode.PredNode(new ShaderIrOp(ShaderIrInst.Cut)));
            }
        }
    }
}