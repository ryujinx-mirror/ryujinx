using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PointerType : BaseNode
    {
        private BaseNode Child;

        public PointerType(BaseNode Child) : base(NodeType.PointerType)
        {
            this.Child = Child;
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

            Writer.Write("*");
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