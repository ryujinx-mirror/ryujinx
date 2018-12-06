using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class TemplateArguments : NodeArray
    {
        public TemplateArguments(List<BaseNode> nodes) : base(nodes, NodeType.TemplateArguments) { }

        public override void PrintLeft(TextWriter writer)
        {
            string Params = string.Join<BaseNode>(", ", Nodes.ToArray());

            writer.Write("<");

            writer.Write(Params);

            if (Params.EndsWith(">"))
            {
                writer.Write(" ");
            }

            writer.Write(">");
        }
    }
}
