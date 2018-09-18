using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecoder
    {
        private const long HeaderSize = 0x50;

        private const bool AddDbgComments = true;

        public static ShaderIrBlock[] Decode(IGalMemory Memory, long Start)
        {
            Dictionary<int, ShaderIrBlock> Visited    = new Dictionary<int, ShaderIrBlock>();
            Dictionary<int, ShaderIrBlock> VisitedEnd = new Dictionary<int, ShaderIrBlock>();

            Queue<ShaderIrBlock> Blocks = new Queue<ShaderIrBlock>();

            long Beginning = Start + HeaderSize;

            ShaderIrBlock Enqueue(int Position, ShaderIrBlock Source = null)
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

            ShaderIrBlock Entry = Enqueue(0);

            while (Blocks.Count > 0)
            {
                ShaderIrBlock Current = Blocks.Dequeue();

                FillBlock(Memory, Current, Beginning);

                //Set child blocks. "Branch" is the block the branch instruction
                //points to (when taken), "Next" is the block at the next address,
                //executed when the branch is not taken. For Unconditional Branches
                //or end of shader, Next is null.
                if (Current.Nodes.Count > 0)
                {
                    ShaderIrNode LastNode = Current.GetLastNode();

                    ShaderIrOp InnerOp = GetInnermostOp(LastNode);

                    if (InnerOp?.Inst == ShaderIrInst.Bra)
                    {
                        int Target = ((ShaderIrOperImm)InnerOp.OperandA).Value;

                        Current.Branch = Enqueue(Target, Current);
                    }

                    foreach (ShaderIrNode Node in Current.Nodes)
                    {
                        InnerOp = GetInnermostOp(Node);

                        if (InnerOp is ShaderIrOp CurrOp && CurrOp.Inst == ShaderIrInst.Ssy)
                        {
                            int Target = ((ShaderIrOperImm)CurrOp.OperandA).Value;

                            Current.Branch = Enqueue(Target, Current);
                        }
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
                uint FirstPos = uint.MaxValue;

                foreach (ShaderIrBlock Block in Visited.Values)
                {
                    if (FirstPos > (uint)Block.Position)
                        FirstPos = (uint)Block.Position;
                }

                ShaderIrBlock Current = Visited[(int)FirstPos];

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
            int Position = Block.Position;

            do
            {
                //Ignore scheduling instructions, which are written every 32 bytes.
                if ((Position & 0x1f) == 0)
                {
                    Position += 8;

                    continue;
                }

                uint Word0 = (uint)Memory.ReadInt32(Position + Beginning + 0);
                uint Word1 = (uint)Memory.ReadInt32(Position + Beginning + 4);

                Position += 8;

                long OpCode = Word0 | (long)Word1 << 32;

                ShaderDecodeFunc Decode = ShaderOpCodeTable.GetDecoder(OpCode);

                if (AddDbgComments)
                {
                    string DbgOpCode = $"0x{(Position - 8):x16}: 0x{OpCode:x16} ";

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

                Decode(Block, OpCode, Position);
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