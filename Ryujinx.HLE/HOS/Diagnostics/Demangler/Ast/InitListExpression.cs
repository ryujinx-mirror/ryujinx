using System;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class InitListExpression : BaseNode
    {
        private BaseNode       TypeNode;
        private List<BaseNode> Nodes;

        public InitListExpression(BaseNode TypeNode, List<BaseNode> Nodes) : base(NodeType.InitListExpression)
        {
            this.TypeNode = TypeNode;
            this.Nodes    = Nodes;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (TypeNode != null)
            {
                TypeNode.Print(Writer);
            }

            Writer.Write("{");
            Writer.Write(string.Join<BaseNode>(", ", Nodes.ToArray()));
            Writer.Write("}");
        }
    }
}