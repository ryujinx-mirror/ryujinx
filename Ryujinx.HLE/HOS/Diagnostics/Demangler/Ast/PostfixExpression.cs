using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PostfixExpression : ParentNode
    {
        private string Operator;

        public PostfixExpression(BaseNode Type, string Operator) : base(NodeType.PostfixExpression, Type)
        {
            this.Operator = Operator;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("(");
            Child.Print(Writer);
            Writer.Write(")");
            Writer.Write(Operator);
        }
    }
}