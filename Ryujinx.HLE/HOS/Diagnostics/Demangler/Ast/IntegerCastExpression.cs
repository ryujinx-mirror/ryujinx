using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class IntegerCastExpression : ParentNode
    {
        private string Number;

        public IntegerCastExpression(BaseNode Type, string Number) : base(NodeType.IntegerCastExpression, Type)
        {
            this.Number = Number;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("(");
            Child.Print(Writer);
            Writer.Write(")");
            Writer.Write(Number);
        }
    }
}