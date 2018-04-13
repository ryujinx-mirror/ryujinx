using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrBlock
    {
        private List<ShaderIrNode> Nodes;

        public ShaderIrBlock()
        {
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