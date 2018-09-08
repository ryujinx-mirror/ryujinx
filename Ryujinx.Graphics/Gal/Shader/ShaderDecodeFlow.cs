using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Bra(ShaderIrBlock Block, long OpCode, long Position)
        {
            if ((OpCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            int Target = OpCode.Branch();

            ShaderIrOperImm Imm = new ShaderIrOperImm(Target);

            Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Bra, Imm)));
        }

        public static void Exit(ShaderIrBlock Block, long OpCode, long Position)
        {
            int CCode = (int)OpCode & 0x1f;

            //TODO: Figure out what the other condition codes mean...
            if (CCode == 0xf)
            {
                Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Exit)));
            }

        }

        public static void Kil(ShaderIrBlock Block, long OpCode, long Position)
        {
            Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Kil)));
        }

        public static void Ssy(ShaderIrBlock Block, long OpCode, long Position)
        {
            if ((OpCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            int Offset = OpCode.Branch();

            int Target = (int)(Position + Offset);

            ShaderIrOperImm Imm = new ShaderIrOperImm(Target);

            Block.AddNode(new ShaderIrOp(ShaderIrInst.Ssy, Imm));
        }

        public static void Sync(ShaderIrBlock Block, long OpCode, long Position)
        {
            //TODO: Implement Sync condition codes

            Block.AddNode(OpCode.PredNode(new ShaderIrOp(ShaderIrInst.Sync)));
        }
    }
}