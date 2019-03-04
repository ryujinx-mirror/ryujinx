using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Bra(ShaderIrBlock block, long opCode, int position)
        {
            if ((opCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            ShaderIrOperImm imm = new ShaderIrOperImm(position + opCode.Branch());

            block.AddNode(opCode.PredNode(new ShaderIrOp(ShaderIrInst.Bra, imm)));
        }

        public static void Exit(ShaderIrBlock block, long opCode, int position)
        {
            int cCode = (int)opCode & 0x1f;

            //TODO: Figure out what the other condition codes mean...
            if (cCode == 0xf)
            {
                block.AddNode(opCode.PredNode(new ShaderIrOp(ShaderIrInst.Exit)));
            }
        }

        public static void Kil(ShaderIrBlock block, long opCode, int position)
        {
            block.AddNode(opCode.PredNode(new ShaderIrOp(ShaderIrInst.Kil)));
        }

        public static void Ssy(ShaderIrBlock block, long opCode, int position)
        {
            if ((opCode & 0x20) != 0)
            {
                //This reads the target offset from the constant buffer.
                //Almost impossible to support with GLSL.
                throw new NotImplementedException();
            }

            ShaderIrOperImm imm = new ShaderIrOperImm(position + opCode.Branch());

            block.AddNode(new ShaderIrOp(ShaderIrInst.Ssy, imm));
        }

        public static void Sync(ShaderIrBlock block, long opCode, int position)
        {
            //TODO: Implement Sync condition codes
            block.AddNode(opCode.PredNode(new ShaderIrOp(ShaderIrInst.Sync)));
        }
    }
}