using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class BracedExpression : BaseNode
    {
        private BaseNode _element;
        private BaseNode _expression;
        private bool     _isArrayExpression;

        public BracedExpression(BaseNode element, BaseNode expression, bool isArrayExpression) : base(NodeType.BracedExpression)
        {
            _element           = element;
            _expression        = expression;
            _isArrayExpression = isArrayExpression;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_isArrayExpression)
            {
                writer.Write("[");
                _element.Print(writer);
                writer.Write("]");
            }
            else
            {
                writer.Write(".");
                _element.Print(writer);
            }

            if (!_expression.GetType().Equals(NodeType.BracedExpression) || !_expression.GetType().Equals(NodeType.BracedRangeExpression))
            {
                writer.Write(" = ");
            }

            _expression.Print(writer);
        }
    }
}
