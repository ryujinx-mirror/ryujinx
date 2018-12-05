using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class BinaryExpression : BaseNode
    {
        private BaseNode LeftPart;
        private string   Name;
        private BaseNode RightPart;

        public BinaryExpression(BaseNode LeftPart, string Name, BaseNode RightPart) : base(NodeType.BinaryExpression)
        {
            this.LeftPart  = LeftPart;
            this.Name      = Name;
            this.RightPart = RightPart;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (Name.Equals(">"))
            {
                Writer.Write("(");
            }

            Writer.Write("(");
            LeftPart.Print(Writer);
            Writer.Write(") ");

            Writer.Write(Name);

            Writer.Write(" (");
            RightPart.Print(Writer);
            Writer.Write(")");

            if (Name.Equals(">"))
            {
                Writer.Write(")");
            }
        }
    }
}