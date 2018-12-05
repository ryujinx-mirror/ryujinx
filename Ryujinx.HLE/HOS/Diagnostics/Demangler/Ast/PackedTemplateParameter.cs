using System.Collections.Generic;
using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PackedTemplateParameter : NodeArray
    {
        public PackedTemplateParameter(List<BaseNode> Nodes) : base(Nodes, NodeType.PackedTemplateParameter) { }

        public override void PrintLeft(TextWriter Writer)
        {
            foreach (BaseNode Node in Nodes)
            {
                Node.PrintLeft(Writer);
            }
        }

        public override void PrintRight(TextWriter Writer)
        {
            foreach (BaseNode Node in Nodes)
            {
                Node.PrintLeft(Writer);
            }
        }

        public override bool HasRightPart()
        {
            foreach (BaseNode Node in Nodes)
            {
                if (Node.HasRightPart())
                {
                    return true;
                }
            }

            return false;
        }
    }
}