using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class InitListExpression : BaseNode
    {
        private readonly BaseNode _typeNode;
        private readonly List<BaseNode> _nodes;

        public InitListExpression(BaseNode typeNode, List<BaseNode> nodes) : base(NodeType.InitListExpression)
        {
            _typeNode = typeNode;
            _nodes = nodes;
        }

        public override void PrintLeft(TextWriter writer)
        {
            _typeNode?.Print(writer);

            writer.Write("{");
            writer.Write(string.Join<BaseNode>(", ", _nodes.ToArray()));
            writer.Write("}");
        }
    }
}
