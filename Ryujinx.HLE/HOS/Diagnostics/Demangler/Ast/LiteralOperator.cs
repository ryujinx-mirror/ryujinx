using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class LiteralOperator : ParentNode
    {
        public LiteralOperator(BaseNode Child) : base(NodeType.LiteralOperator, Child) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("operator \"");
            Child.PrintLeft(Writer);
            Writer.Write("\"");
        }
    }
}