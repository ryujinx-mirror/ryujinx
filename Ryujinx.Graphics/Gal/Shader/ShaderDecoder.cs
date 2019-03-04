using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecoder
    {
        private const long HeaderSize = 0x50;

        private const bool AddDbgComments = true;

        public static ShaderIrBlock[] Decode(IGalMemory memory, long start)
        {
            Dictionary<int, ShaderIrBlock> visited    = new Dictionary<int, ShaderIrBlock>();
            Dictionary<int, ShaderIrBlock> visitedEnd = new Dictionary<int, ShaderIrBlock>();

            Queue<ShaderIrBlock> blocks = new Queue<ShaderIrBlock>();

            long beginning = start + HeaderSize;

            ShaderIrBlock Enqueue(int position, ShaderIrBlock source = null)
            {
                if (!visited.TryGetValue(position, out ShaderIrBlock output))
                {
                    output = new ShaderIrBlock(position);

                    blocks.Enqueue(output);

                    visited.Add(position, output);
                }

                if (source != null)
                {
                    output.Sources.Add(source);
                }

                return output;
            }

            ShaderIrBlock entry = Enqueue(0);

            while (blocks.Count > 0)
            {
                ShaderIrBlock current = blocks.Dequeue();

                FillBlock(memory, current, beginning);

                //Set child blocks. "Branch" is the block the branch instruction
                //points to (when taken), "Next" is the block at the next address,
                //executed when the branch is not taken. For Unconditional Branches
                //or end of shader, Next is null.
                if (current.Nodes.Count > 0)
                {
                    ShaderIrNode lastNode = current.GetLastNode();

                    ShaderIrOp innerOp = GetInnermostOp(lastNode);

                    if (innerOp?.Inst == ShaderIrInst.Bra)
                    {
                        int target = ((ShaderIrOperImm)innerOp.OperandA).Value;

                        current.Branch = Enqueue(target, current);
                    }

                    foreach (ShaderIrNode node in current.Nodes)
                    {
                        innerOp = GetInnermostOp(node);

                        if (innerOp is ShaderIrOp currOp && currOp.Inst == ShaderIrInst.Ssy)
                        {
                            int target = ((ShaderIrOperImm)currOp.OperandA).Value;

                            Enqueue(target, current);
                        }
                    }

                    if (NodeHasNext(lastNode))
                    {
                        current.Next = Enqueue(current.EndPosition);
                    }
                }

                //If we have on the graph two blocks with the same end position,
                //then we need to split the bigger block and have two small blocks,
                //the end position of the bigger "Current" block should then be == to
                //the position of the "Smaller" block.
                while (visitedEnd.TryGetValue(current.EndPosition, out ShaderIrBlock smaller))
                {
                    if (current.Position > smaller.Position)
                    {
                        ShaderIrBlock temp = smaller;

                        smaller = current;
                        current = temp;
                    }

                    current.EndPosition = smaller.Position;
                    current.Next        = smaller;
                    current.Branch      = null;

                    current.Nodes.RemoveRange(
                        current.Nodes.Count - smaller.Nodes.Count,
                        smaller.Nodes.Count);

                    visitedEnd[smaller.EndPosition] = smaller;
                }

                visitedEnd.Add(current.EndPosition, current);
            }

            //Make and sort Graph blocks array by position.
            ShaderIrBlock[] graph = new ShaderIrBlock[visited.Count];

            while (visited.Count > 0)
            {
                uint firstPos = uint.MaxValue;

                foreach (ShaderIrBlock block in visited.Values)
                {
                    if (firstPos > (uint)block.Position)
                        firstPos = (uint)block.Position;
                }

                ShaderIrBlock current = visited[(int)firstPos];

                do
                {
                    graph[graph.Length - visited.Count] = current;

                    visited.Remove(current.Position);

                    current = current.Next;
                }
                while (current != null);
            }

            return graph;
        }

        private static void FillBlock(IGalMemory memory, ShaderIrBlock block, long beginning)
        {
            int position = block.Position;

            do
            {
                //Ignore scheduling instructions, which are written every 32 bytes.
                if ((position & 0x1f) == 0)
                {
                    position += 8;

                    continue;
                }

                uint word0 = (uint)memory.ReadInt32(position + beginning + 0);
                uint word1 = (uint)memory.ReadInt32(position + beginning + 4);

                position += 8;

                long opCode = word0 | (long)word1 << 32;

                ShaderDecodeFunc decode = ShaderOpCodeTable.GetDecoder(opCode);

                if (AddDbgComments)
                {
                    string dbgOpCode = $"0x{(position - 8):x16}: 0x{opCode:x16} ";

                    dbgOpCode += (decode?.Method.Name ?? "???");

                    if (decode == ShaderDecode.Bra || decode == ShaderDecode.Ssy)
                    {
                        int offset = ((int)(opCode >> 20) << 8) >> 8;

                        long target = position + offset;

                        dbgOpCode += " (0x" + target.ToString("x16") + ")";
                    }

                    block.AddNode(new ShaderIrCmnt(dbgOpCode));
                }

                if (decode == null)
                {
                    continue;
                }

                decode(block, opCode, position);
            }
            while (!IsFlowChange(block.GetLastNode()));

            block.EndPosition = position;
        }

        private static bool IsFlowChange(ShaderIrNode node)
        {
            return !NodeHasNext(GetInnermostOp(node));
        }

        private static ShaderIrOp GetInnermostOp(ShaderIrNode node)
        {
            if (node is ShaderIrCond cond)
            {
                node = cond.Child;
            }

            return node is ShaderIrOp op ? op : null;
        }

        private static bool NodeHasNext(ShaderIrNode node)
        {
            if (!(node is ShaderIrOp op))
            {
                return true;
            }

            return op.Inst != ShaderIrInst.Exit &&
                   op.Inst != ShaderIrInst.Bra;
        }
    }
}