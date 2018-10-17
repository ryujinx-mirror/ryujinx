using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class TemplateArguments : NodeArray
    {
        public TemplateArguments(List<BaseNode> Nodes) : base(Nodes, NodeType.TemplateArguments) { }

        public override void PrintLeft(TextWriter Writer)
        {
            string Params = string.Join<BaseNode>(", ", Nodes.ToArray());

            Writer.Write("<");

            Writer.Write(Params);

            if (Params.EndsWith(">"))
            {
                Writer.Write(" ");
            }

            Writer.Write(">");
        }
    }
}
