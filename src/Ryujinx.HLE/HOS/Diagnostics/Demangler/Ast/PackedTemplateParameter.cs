using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PackedTemplateParameter : NodeArray
    {
        public PackedTemplateParameter(List<BaseNode> nodes) : base(nodes, NodeType.PackedTemplateParameter) { }

        public override void PrintLeft(TextWriter writer)
        {
            foreach (BaseNode node in Nodes)
            {
                node.PrintLeft(writer);
            }
        }

        public override void PrintRight(TextWriter writer)
        {
            foreach (BaseNode node in Nodes)
            {
                node.PrintLeft(writer);
            }
        }

        public override bool HasRightPart()
        {
            foreach (BaseNode node in Nodes)
            {
                if (node.HasRightPart())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
