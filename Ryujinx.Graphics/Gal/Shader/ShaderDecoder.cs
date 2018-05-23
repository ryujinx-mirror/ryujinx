namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecoder
    {
        private const bool AddDbgComments = true;

        public static ShaderIrBlock DecodeBasicBlock(IGalMemory Memory, long Position)
        {
            ShaderIrBlock Block = new ShaderIrBlock();

            while (true)
            {
                Block.Position = Position;

                Block.MarkLabel(Position);

                //Ignore scheduling instructions, which are written every 32 bytes.
                if ((Position & 0x1f) == 0)
                {
                    Position += 8;

                    continue;
                }

                uint Word0 = (uint)Memory.ReadInt32(Position + 0);
                uint Word1 = (uint)Memory.ReadInt32(Position + 4);

                Position += 8;

                long OpCode = Word0 | (long)Word1 << 32;

                ShaderDecodeFunc Decode = ShaderOpCodeTable.GetDecoder(OpCode);

                if (AddDbgComments)
                {
                    string DbgOpCode = $"0x{Position:x16}: 0x{OpCode:x16} ";

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