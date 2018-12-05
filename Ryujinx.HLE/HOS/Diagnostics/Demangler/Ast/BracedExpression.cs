using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class BracedExpression : BaseNode
    {
        private BaseNode Element;
        private BaseNode Expression;
        private bool     IsArrayExpression;

        public BracedExpression(BaseNode Element, BaseNode Expression, bool IsArrayExpression) : base(NodeType.BracedExpression)
        {
            this.Element           = Element;
            this.Expression        = Expression;
            this.IsArrayExpression = IsArrayExpression;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (IsArrayExpression)
            {
                Writer.Write("[");
                Element.Print(Writer);
                Writer.Write("]");
            }
            else
            {
                Writer.Write(".");
                Element.Print(Writer);
            }

            if (!Expression.GetType().Equals(NodeType.BracedExpression) || !Expression.GetType().Equals(NodeType.BracedRangeExpression))
            {
                Writer.Write(" = ");
            }

            Expression.Print(Writer);
        }
    }
}
