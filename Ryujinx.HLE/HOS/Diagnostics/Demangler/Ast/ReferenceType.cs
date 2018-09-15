using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ReferenceType : BaseNode
    {
        private string   Reference;
        private BaseNode Child;

        public ReferenceType(string Reference, BaseNode Child) : base(NodeType.ReferenceType)
        {
            this.Reference = Reference;
            this.Child     = Child;
        }

        public override bool HasRightPart()
        {
            return Child.HasRightPart();
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Child.PrintLeft(Writer);

            if (Child.IsArray())
            {
                Writer.Write(" ");
            }

            if (Child.IsArray() || Child.HasFunctions())
            {
                Writer.Write("(");
            }

            Writer.Write(Reference);
        }
        public override void PrintRight(TextWriter Writer)
        {
            if (Child.IsArray() || Child.HasFunctions())
            {
                Writer.Write(")");
            }

            Child.PrintRight(Writer);
        }
    }
}