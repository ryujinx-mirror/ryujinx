using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrBlock
    {
        private List<ShaderIrNode> Nodes;

        private Dictionary<int, ShaderIrLabel> LabelsToInsert;

        public int Position;

        public ShaderIrBlock()
        {
            Nodes = new List<ShaderIrNode>();

            LabelsToInsert = new Dictionary<int, ShaderIrLabel>();
        }

        public void AddNode(ShaderIrNode Node)
        {
            Nodes.Add(Node);
        }

        public ShaderIrLabel GetLabel(int Position)
        {
            if (LabelsToInsert.TryGetValue(Position, out ShaderIrLabel Label))
            {
                return Label;
            }

            Label = new ShaderIrLabel();

            LabelsToInsert.Add(Position, Label);

            return Label;
        }

        public void MarkLabel(int Position)
        {
            if (LabelsToInsert.TryGetValue(Position, out ShaderIrLabel Label))
            {
                Nodes.Add(Label);
            }
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