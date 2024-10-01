using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class BracedRangeExpression : BaseNode
    {
        private readonly BaseNode _firstNode;
        private readonly BaseNode _lastNode;
        private readonly BaseNode _expression;

        public BracedRangeExpression(BaseNode firstNode, BaseNode lastNode, BaseNode expression) : base(NodeType.BracedRangeExpression)
        {
            _firstNode = firstNode;
            _lastNode = lastNode;
            _expression = expression;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("[");
            _firstNode.Print(writer);
            writer.Write(" ... ");
            _lastNode.Print(writer);
            writer.Write("]");

            if (!_expression.GetType().Equals(NodeType.BracedExpression) || !_expression.GetType().Equals(NodeType.BracedRangeExpression))
            {
                writer.Write(" = ");
            }

            _expression.Print(writer);
        }
    }
}
