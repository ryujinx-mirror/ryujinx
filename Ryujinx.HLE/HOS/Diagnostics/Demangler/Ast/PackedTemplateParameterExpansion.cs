using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PackedTemplateParameterExpansion : ParentNode
    {
        public PackedTemplateParameterExpansion(BaseNode Child) : base(NodeType.PackedTemplateParameterExpansion, Child) {}

        public override void PrintLeft(TextWriter Writer)
        {
            if (Child is PackedTemplateParameter)
            {
                if (((PackedTemplateParameter)Child).Nodes.Count !=  0)
                {
                    Child.Print(Writer);
                }
            }
            else
            {
                Writer.Write("...");
            }
        }
    }
}