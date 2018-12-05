using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class EnclosedExpression : BaseNode
    {
        private string   Prefix;
        private BaseNode Expression;
        private string   Postfix;

        public EnclosedExpression(string Prefix, BaseNode Expression, string Postfix) : base(NodeType.EnclosedExpression)
        {
            this.Prefix     = Prefix;
            this.Expression = Expression;
            this.Postfix    = Postfix;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(Prefix);
            Expression.Print(Writer);
            Writer.Write(Postfix);
        }
    }
}