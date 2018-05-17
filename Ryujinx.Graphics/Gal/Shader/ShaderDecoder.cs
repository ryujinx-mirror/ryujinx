namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecoder
    {
        private const bool AddDbgComments = true;

        public static ShaderIrBlock DecodeBasicBlock(int[] Code, int Offset)
        {
            ShaderIrBlock Block = new ShaderIrBlock();

            while (Offset + 2 <= Code.Length)
            {
                int InstPos = Offset * 4;

                Block.Position = InstPos;

                Block.MarkLabel(InstPos);

                //Ignore scheduling instructions, which are written every 32 bytes.
                if ((Offset & 7) == 0)
                {
                    Offset += 2;

                    continue;
                }

                uint Word0 = (uint)Code[Offset++];
                uint Word1 = (uint)Code[Offset++];

                long OpCode = Word0 | (long)Word1 << 32;

                ShaderDecodeFunc Decode = ShaderOpCodeTable.GetDecoder(OpCode);

                if (AddDbgComments)
                {
                    string DbgOpCode = $"0x{InstPos:x8}: 0x{OpCode:x16} ";

                    Block.AddNode(new ShaderIrCmnt(DbgOpCode + (Decode?.Method.Name ?? "???")));
                }

                if (Decode == null)
                {
                    continue;
                }

                Decode(Block, OpCode);

                if (Block.GetLastNode() is ShaderIrOp Op && Op.Inst == ShaderIrInst.Exit)
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