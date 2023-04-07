using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class InitListExpression : BaseNode
    {
        private BaseNode       _typeNode;
        private List<BaseNode> _nodes;

        public InitListExpression(BaseNode typeNode, List<BaseNode> nodes) : base(NodeType.InitListExpression)
        {
            _typeNode = typeNode;
            _nodes    = nodes;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_typeNode != null)
            {
                _typeNode.Print(writer);
            }

            writer.Write("{");
            writer.Write(string.Join<BaseNode>(", ", _nodes.ToArray()));
            writer.Write("}");
        }
    }
}