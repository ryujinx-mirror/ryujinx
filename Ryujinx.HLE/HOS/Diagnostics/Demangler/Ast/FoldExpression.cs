using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class FoldExpression : BaseNode
    {
        private bool     IsLeftFold;
        private string   OperatorName;
        private BaseNode Expression;
        private BaseNode Initializer;

        public FoldExpression(bool IsLeftFold, string OperatorName, BaseNode Expression, BaseNode Initializer) : base(NodeType.FunctionParameter)
        {
            this.IsLeftFold   = IsLeftFold;
            this.OperatorName = OperatorName;
            this.Expression   = Expression;
            this.Initializer  = Initializer;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("(");

            if (IsLeftFold && Initializer != null)
            {
                Initializer.Print(Writer);
                Writer.Write(" ");
                Writer.Write(OperatorName);
                Writer.Write(" ");
            }

            Writer.Write(IsLeftFold ? "... " : " ");
            Writer.Write(OperatorName);
            Writer.Write(!IsLeftFold ? " ..." : " ");
            Expression.Print(Writer);

            if (!IsLeftFold && Initializer != null)
            {
                Initializer.Print(Writer);
                Writer.Write(" ");
                Writer.Write(OperatorName);
                Writer.Write(" ");
            }

            Writer.Write(")");
        }
    }
}