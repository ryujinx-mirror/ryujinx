using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ConversionExpression : BaseNode
    {
        private BaseNode TypeNode;
        private BaseNode Expressions;

        public ConversionExpression(BaseNode TypeNode, BaseNode Expressions) : base(NodeType.ConversionExpression)
        {
            this.TypeNode    = TypeNode;
            this.Expressions = Expressions;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("(");
            TypeNode.Print(Writer);
            Writer.Write(")(");
            Expressions.Print(Writer);
        }
    }
}