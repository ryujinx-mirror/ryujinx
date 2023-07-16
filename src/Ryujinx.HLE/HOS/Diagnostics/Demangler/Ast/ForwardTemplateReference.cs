using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ForwardTemplateReference : BaseNode
    {
        // TODO: Compute inside the Demangler
        public BaseNode Reference;
#pragma warning disable IDE0052 // Remove unread private member
        private readonly int _index;
#pragma warning restore IDE0052

        public ForwardTemplateReference(int index) : base(NodeType.ForwardTemplateReference)
        {
            _index = index;
        }

        public override string GetName()
        {
            return Reference.GetName();
        }

        public override void PrintLeft(TextWriter writer)
        {
            Reference.PrintLeft(writer);
        }

        public override void PrintRight(TextWriter writer)
        {
            Reference.PrintRight(writer);
        }

        public override bool HasRightPart()
        {
            return Reference.HasRightPart();
        }
    }
}
