using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrBlock
    {
        private List<ShaderIrNode> Nodes;

        private Dictionary<long, ShaderIrLabel> LabelsToInsert;

        public long Position;

        public ShaderIrBlock()
        {
            Nodes = new List<ShaderIrNode>();

            LabelsToInsert = new Dictionary<long, ShaderIrLabel>();
        }

        public void AddNode(ShaderIrNode Node)
        {
            Nodes.Add(Node);
        }

        public ShaderIrLabel GetLabel(long Position)
        {
            if (LabelsToInsert.TryGetValue(Position, out ShaderIrLabel Label))
            {
                return Label;
            }

            Label = new ShaderIrLabel();

            LabelsToInsert.Add(Position, Label);

            return Label;
        }

        public void MarkLabel(long Position)
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