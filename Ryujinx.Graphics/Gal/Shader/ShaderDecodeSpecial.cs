using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Out_R(ShaderIrBlock Block, long OpCode, long Position)
        {
            //TODO: Those registers have to be used for something
            ShaderIrOperGpr Gpr0  = OpCode.Gpr0();
            ShaderIrOperGpr Gpr8  = OpCode.Gpr8();
            ShaderIrOperGpr Gpr20 = OpCode.Gpr20();

            int Type = OpCode.Read(39, 3);

            if ((Type & 1) != 0)
            {
                Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Emit)));
            }

            if ((Type & 2) != 0)
            {
                Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Cut)));
            }
        }
    }
}