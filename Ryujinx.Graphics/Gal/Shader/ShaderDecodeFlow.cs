using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Exit(ShaderIrBlock Block, long OpCode)
        {
            Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Exit), OpCode));
        }

        public static void Kil(ShaderIrBlock Block, long OpCode)
        {
            Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Kil), OpCode));
        }
    }
}