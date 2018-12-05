using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NodeArray : BaseNode
    {
        public List<BaseNode> Nodes { get; protected set; }

        public NodeArray(List<BaseNode> Nodes) : base(NodeType.NodeArray)
        {
            this.Nodes = Nodes;
        }

        public NodeArray(List<BaseNode> Nodes, NodeType Type) : base(Type)
        {
            this.Nodes = Nodes;
        }

        public override bool IsArray()
        {
            return true;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(string.Join<BaseNode>(", ", Nodes.ToArray()));
        }
    }
}