using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class BracedRangeExpression : BaseNode
    {
        private BaseNode FirstNode;
        private BaseNode LastNode;
        private BaseNode Expression;

        public BracedRangeExpression(BaseNode FirstNode, BaseNode LastNode, BaseNode Expression) : base(NodeType.BracedRangeExpression)
        {
            this.FirstNode  = FirstNode;
            this.LastNode   = LastNode;
            this.Expression = Expression;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("[");
            FirstNode.Print(Writer);
            Writer.Write(" ... ");
            LastNode.Print(Writer);
            Writer.Write("]");

            if (!Expression.GetType().Equals(NodeType.BracedExpression) || !Expression.GetType().Equals(NodeType.BracedRangeExpression))
            {
                Writer.Write(" = ");
            }

            Expression.Print(Writer);
        }
    }
}
