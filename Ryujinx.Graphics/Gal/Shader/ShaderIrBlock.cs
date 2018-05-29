using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrBlock
    {
        public long Position    { get; set; }
        public long EndPosition { get; set; }

        public ShaderIrBlock Next   { get; set; }
        public ShaderIrBlock Branch { get; set; }

        public List<ShaderIrBlock> Sources { get; private set; }

        public List<ShaderIrNode> Nodes { get; private set; }

        public ShaderIrBlock(long Position)
        {
            this.Position = Position;

            Sources = new List<ShaderIrBlock>();

            Nodes = new List<ShaderIrNode>();
        }

        public void AddNode(ShaderIrNode Node)
        {
            Nodes.Add(Node);
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