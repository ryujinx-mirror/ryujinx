using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Bra(ShaderIrBlock Block, long OpCode, int Position)
        {
            if ((OpCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            ShaderIrOperImm Imm = new ShaderIrOperImm(Position + OpCode.Branch());

            Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Bra, Imm)));
        }

        public static void Exit(ShaderIrBlock Block, long OpCode, int Position)
        {
            int CCode = (int)OpCode & 0x1f;

            //TODO: Figure out what the other condition codes mean...
            if (CCode == 0xf)
            {
                Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Exit)));
            }
        }

        public static void Kil(ShaderIrBlock Block, long OpCode, int Position)
        {
            Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Kil)));
        }

        public static void Ssy(ShaderIrBlock Block, long OpCode, int Position)
        {
            if ((OpCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            ShaderIrOperImm Imm = new ShaderIrOperImm(Position + OpCode.Branch());

            Block.AddNode(new ShaderIrOp(ShaderIrInst.Ssy, Imm));
        }

        public static void Sync(ShaderIrBlock Block, long OpCode, int Position)
        {
            //TODO: Implement Sync condition codes
            Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Sync)));
        }
    }
}