using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Out_R(ShaderIrBlock Block, long OpCode)
        {
            //TODO: Those registers have to be used for something
            ShaderIrOperGpr Gpr0  = GetOperGpr0(OpCode);
            ShaderIrOperGpr Gpr8  = GetOperGpr8(OpCode);
            ShaderIrOperGpr Gpr20 = GetOperGpr20(OpCode);

            int Type = (int)((OpCode >> 39) & 3);

            if ((Type & 1) != 0)
            {
                Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Emit), OpCode));
            }

            if ((Type & 2) != 0)
            {
                Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Cut), OpCode));
            }
        }
    }
}