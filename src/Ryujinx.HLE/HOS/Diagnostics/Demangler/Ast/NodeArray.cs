using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NodeArray : BaseNode
    {
        public List<BaseNode> Nodes { get; protected set; }

        public NodeArray(List<BaseNode> nodes) : base(NodeType.NodeArray)
        {
            Nodes = nodes;
        }

        public NodeArray(List<BaseNode> nodes, NodeType type) : base(type)
        {
            Nodes = nodes;
        }

        public override bool IsArray()
        {
            return true;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(string.Join<BaseNode>(", ", Nodes.ToArray()));
        }
    }
}
