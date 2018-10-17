using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class CallExpression : NodeArray
    {
        private BaseNode Callee;

        public CallExpression(BaseNode Callee, List<BaseNode> Nodes) : base(Nodes, NodeType.CallExpression)
        {
            this.Callee = Callee;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Callee.Print(Writer);

            Writer.Write("(");
            Writer.Write(string.Join<BaseNode>(", ", Nodes.ToArray()));
            Writer.Write(")");
        }
    }
}