using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecoder
    {
        private const long HeaderSize = 0x50;

        private const bool AddDbgComments = true;

        public static ShaderIrBlock[] Decode(IGalMemory Memory, long Start)
        {
            Dictionary<long, ShaderIrBlock> Visited    = new Dictionary<long, ShaderIrBlock>();
            Dictionary<long, ShaderIrBlock> VisitedEnd = new Dictionary<long, ShaderIrBlock>();

            Queue<ShaderIrBlock> Blocks = new Queue<ShaderIrBlock>();

            ShaderIrBlock Enqueue(long Position, ShaderIrBlock Source = null)
            {
                if (!Visited.TryGetValue(Position, out ShaderIrBlock Output))
                {
                    Output = new ShaderIrBlock(Position);

                    Blocks.Enqueue(Output);

                    Visited.Add(Position, Output);
                }

                if (Source != null)
                {
                    Output.Sources.Add(Source);
                }

                return Output;
            }

            ShaderIrBlock Entry = Enqueue(Start + HeaderSize);

            while (Blocks.Count > 0)
            {
                ShaderIrBlock Current = Blocks.Dequeue();

                FillBlock(Memory, Current, Start + HeaderSize);

                //Set child blocks. "Branch" is the block the branch instruction
                //points to (when taken), "Next" is the block at the next address,
                //executed when the branch is not taken. For Unconditional Branches
                //or end of shader, Next is null.
                if (Current.Nodes.Count > 0)
                {
                    ShaderIrNode LastNode = Current.GetLastNode();

                    ShaderIrOp Op = GetInnermostOp(LastNode);

                    if (Op?.Inst == ShaderIrInst.Bra)
                    {
                        int Offset = ((ShaderIrOperImm)Op.OperandA).Value;

                        long Target = Current.EndPosition + Offset;

                        Current.Branch = Enqueue(Target, Current);
                    }

                    if (NodeHasNext(LastNode))
                    {
                        Current.Next = Enqueue(Current.EndPosition);
                    }
                }

                //If we have on the graph two blocks with the same end position,
                //then we need to split the bigger block and have two small blocks,
                //the end position of the bigger "Current" block should then be == to
                //the position of the "Smaller" block.
                while (VisitedEnd.TryGetValue(Current.EndPosition, out ShaderIrBlock Smaller))
                {
                    if (Current.Position > Smaller.Position)
                    {
                        ShaderIrBlock Temp = Smaller;

                        Smaller = Current;
                        Current = Temp;
                    }

                    Current.EndPosition = Smaller.Position;
                    Current.Next        = Smaller;
                    Current.Branch      = null;

                    Current.Nodes.RemoveRange(
                        Current.Nodes.Count - Smaller.Nodes.Count,
                        Smaller.Nodes.Count);

                    VisitedEnd[Smaller.EndPosition] = Smaller;
                }

                VisitedEnd.Add(Current.EndPosition, Current);
            }

            //Make and sort Graph blocks array by position.
            ShaderIrBlock[] Graph = new ShaderIrBlock[Visited.Count];

            while (Visited.Count > 0)
            {
                ulong FirstPos = ulong.MaxValue;

                foreach (ShaderIrBlock Block in Visited.Values)
                {
                    if (FirstPos > (ulong)Block.Position)
                        FirstPos = (ulong)Block.Position;
                }

                ShaderIrBlock Current = Visited[(long)FirstPos];

                do
                {
                    Graph[Graph.Length - Visited.Count] = Current;

                    Visited.Remove(Current.Position);

                    Current = Current.Next;
                }
                while (Current != null);
            }

            return Graph;
        }

        private static void FillBlock(IGalMemory Memory, ShaderIrBlock Block, long Beginning)
        {
            long Position = Block.Position;

            do
            {
                //Ignore scheduling instructions, which are written every 32 bytes.
                if (((Position - Beginning) & 0x1f) == 0)
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
                    string DbgOpCode = $"0x{(Position - Beginning - 8):x16}: 0x{OpCode:x16} ";

                    DbgOpCode += (Decode?.Method.Name ?? "???");

                    if (Decode == ShaderDecode.Bra)
                    {
                        int Offset = ((int)(OpCode >> 20) << 8) >> 8;

                        long Target = Position + Offset;

                        DbgOpCode += " (0x" + Target.ToString("x16") + ")";
                    }

                    Block.AddNode(new ShaderIrCmnt(DbgOpCode));
                }

                if (Decode == null)
                {
                    continue;
                }

                Decode(Block, OpCode);
            }
            while (!IsFlowChange(Block.GetLastNode()));

            Block.EndPosition = Position;
        }

        private static bool IsFlowChange(ShaderIrNode Node)
        {
            return !NodeHasNext(GetInnermostOp(Node));
        }

        private static ShaderIrOp GetInnermostOp(ShaderIrNode Node)
        {
            if (Node is ShaderIrCond Cond)
            {
                Node = Cond.Child;
            }

            return Node is ShaderIrOp Op ? Op : null;
        }

        private static bool NodeHasNext(ShaderIrNode Node)
        {
            if (!(Node is ShaderIrOp Op))
            {
                return true;
            }

            return Op.Inst != ShaderIrInst.Exit &&
                   Op.Inst != ShaderIrInst.Bra;
        }
    }
}