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

            int Target = ((int)(OpCode >> 20) << 8) >> 8;

            ShaderIrOperImm Imm = new ShaderIrOperImm(Target);

            Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Bra, Imm), OpCode));
        }

        public static void Exit(ShaderIrBlock Block, long OpCode, long Position)
        {
            int CCode = (int)OpCode & 0x1f;

            //TODO: Figure out what the other condition codes mean...
            if (CCode == 0xf)
            {
                Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Exit), OpCode));
            }

        }

        public static void Kil(ShaderIrBlock Block, long OpCode, long Position)
        {
            Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Kil), OpCode));
        }

        public static void Ssy(ShaderIrBlock Block, long OpCode, long Position)
        {
            if ((OpCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            int Offset = ((int)(OpCode >> 20) << 8) >> 8;

            int Target = (int)(Position + Offset);

            ShaderIrOperImm Imm = new ShaderIrOperImm(Target);

            Block.AddNode(new ShaderIrOp(ShaderIrInst.Ssy, Imm));
        }

        public static void Sync(ShaderIrBlock Block, long OpCode, long Position)
        {
            //TODO: Implement Sync condition codes

            Block.AddNode(GetPredNode(new ShaderIrOp(ShaderIrInst.Sync), OpCode));
        }
    }
}