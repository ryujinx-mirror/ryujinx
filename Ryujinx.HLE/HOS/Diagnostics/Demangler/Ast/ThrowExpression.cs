using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ThrowExpression : BaseNode
    {
        private BaseNode Expression;

        public ThrowExpression(BaseNode Expression) : base(NodeType.ThrowExpression)
        {
            this.Expression = Expression;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("throw ");
            Expression.Print(Writer);
        }
    }
}