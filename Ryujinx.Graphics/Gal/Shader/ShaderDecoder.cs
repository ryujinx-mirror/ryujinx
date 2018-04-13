namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecoder
    {
        public static ShaderIrBlock DecodeBasicBlock(int[] Code, int Offset)
        {
            ShaderIrBlock Block = new ShaderIrBlock();

            while (Offset + 2 <= Code.Length)
            {
                //Ignore scheduling instructions, which are
                //written every 32 bytes.
                if ((Offset & 7) == 0)
                {
                    Offset += 2;

                    continue;
                }

                uint Word0 = (uint)Code[Offset++];
                uint Word1 = (uint)Code[Offset++];

                long OpCode = Word0 | (long)Word1 << 32;

                ShaderDecodeFunc Decode = ShaderOpCodeTable.GetDecoder(OpCode);

                if (Decode == null)
                {
                    continue;
                }

                Decode(Block, OpCode);

                if (Block.GetLastNode() is ShaderIrOp Op && IsFlowChange(Op.Inst))
                {
                    break;
                }
            }

            return Block;
        }

        private static bool IsFlowChange(ShaderIrInst Inst)
        {
            return Inst == ShaderIrInst.Exit;
        }
    }
}