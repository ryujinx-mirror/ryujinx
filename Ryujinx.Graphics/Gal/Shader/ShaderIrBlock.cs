using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrBlock
    {
        public int Position    { get; set; }
        public int EndPosition { get; set; }

        public ShaderIrBlock Next   { get; set; }
        public ShaderIrBlock Branch { get; set; }

        public List<ShaderIrBlock> Sources { get; private set; }

        public List<ShaderIrNode> Nodes { get; private set; }

        public ShaderIrBlock(int position)
        {
            Position = position;

            Sources = new List<ShaderIrBlock>();

            Nodes = new List<ShaderIrNode>();
        }

        public void AddNode(ShaderIrNode node)
        {
            Nodes.Add(node);
        }

        public ShaderIrNode[] GetNodes()
        {
            return Nodes.ToArray();
        }

        public ShaderIrNode GetLastNode()
        {
            if (Nodes.Count > 0)
            {
                return Nodes[Nodes.Count - 1];
            }

            return null;
        }
    }
}