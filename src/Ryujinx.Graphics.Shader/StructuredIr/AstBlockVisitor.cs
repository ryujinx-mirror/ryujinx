using System;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.StructuredIr.AstHelper;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class AstBlockVisitor
    {
        public AstBlock Block { get; private set; }

        public class BlockVisitationEventArgs : EventArgs
        {
            public AstBlock Block { get; }

            public BlockVisitationEventArgs(AstBlock block)
            {
                Block = block;
            }
        }

        public event EventHandler<BlockVisitationEventArgs> BlockEntered;
        public event EventHandler<BlockVisitationEventArgs> BlockLeft;

        public AstBlockVisitor(AstBlock mainBlock)
        {
            Block = mainBlock;
        }

        public IEnumerable<IAstNode> Visit()
        {
            IAstNode node = Block.First;

            while (node != null)
            {
                // We reached a child block, visit the nodes inside.
                while (node is AstBlock childBlock)
                {
                    Block = childBlock;

                    node = childBlock.First;

                    BlockEntered?.Invoke(this, new BlockVisitationEventArgs(Block));
                }

                // Node may be null, if the block is empty.
                if (node != null)
                {
                    IAstNode next = Next(node);

                    yield return node;

                    node = next;
                }

                // We reached the end of the list, go up on tree to the parent blocks.
                while (node == null && Block.Type != AstBlockType.Main)
                {
                    BlockLeft?.Invoke(this, new BlockVisitationEventArgs(Block));

                    node = Next(Block);

                    Block = Block.Parent;
                }
            }
        }
    }
}
