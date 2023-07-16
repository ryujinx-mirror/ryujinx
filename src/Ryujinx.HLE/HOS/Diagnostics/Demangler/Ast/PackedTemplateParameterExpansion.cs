using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PackedTemplateParameterExpansion : ParentNode
    {
        public PackedTemplateParameterExpansion(BaseNode child) : base(NodeType.PackedTemplateParameterExpansion, child) { }

        public override void PrintLeft(TextWriter writer)
        {
            if (Child is PackedTemplateParameter parameter)
            {
                if (parameter.Nodes.Count != 0)
                {
                    parameter.Print(writer);
                }
            }
            else
            {
                writer.Write("...");
            }
        }
    }
}
