using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Bra(ShaderIrBlock Block, long OpCode)
        {
            if ((OpCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            long Target = ((int)(OpCode >> 20) << 8) >> 8;

            Target += Block.Position + 8;

            Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Bra, Block.GetLabel(Target)), OpCode));
        }

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