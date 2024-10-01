using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CallExpression : NodeArray
    {
        private readonly BaseNode _callee;

        public CallExpression(BaseNode callee, List<BaseNode> nodes) : base(nodes, NodeType.CallExpression)
        {
            _callee = callee;
        }

        public override void PrintLeft(TextWriter writer)
        {
            _callee.Print(writer);

            writer.Write("(");
            writer.Write(string.Join<BaseNode>(", ", Nodes.ToArray()));
            writer.Write(")");
        }
    }
}
