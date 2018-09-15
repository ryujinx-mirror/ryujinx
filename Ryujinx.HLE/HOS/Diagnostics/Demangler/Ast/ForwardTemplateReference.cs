using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ForwardTemplateReference : BaseNode
    {
        // TODO: Compute inside the Demangler
        public BaseNode Reference;
        private int     Index;

        public ForwardTemplateReference(int Index) : base(NodeType.ForwardTemplateReference)
        {
            this.Index = Index;
        }

        public override string GetName()
        {
            return Reference.GetName();
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Reference.PrintLeft(Writer);
        }

        public override void PrintRight(TextWriter Writer)
        {
            Reference.PrintRight(Writer);
        }

        public override bool HasRightPart()
        {
            return Reference.HasRightPart();
        }
    }
}
