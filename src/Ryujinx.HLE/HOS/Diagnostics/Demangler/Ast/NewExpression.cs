using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NewExpression : BaseNode
    {
        private readonly NodeArray _expressions;
        private readonly BaseNode _typeNode;
        private readonly NodeArray _initializers;

        private readonly bool _isGlobal;
        private readonly bool _isArrayExpression;

        public NewExpression(NodeArray expressions, BaseNode typeNode, NodeArray initializers, bool isGlobal, bool isArrayExpression) : base(NodeType.NewExpression)
        {
            _expressions = expressions;
            _typeNode = typeNode;
            _initializers = initializers;

            _isGlobal = isGlobal;
            _isArrayExpression = isArrayExpression;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_isGlobal)
            {
                writer.Write("::operator ");
            }

            writer.Write("new ");

            if (_isArrayExpression)
            {
                writer.Write("[] ");
            }

            if (_expressions.Nodes.Count != 0)
            {
                writer.Write("(");
                _expressions.Print(writer);
                writer.Write(")");
            }

            _typeNode.Print(writer);

            if (_initializers.Nodes.Count != 0)
            {
                writer.Write("(");
                _initializers.Print(writer);
                writer.Write(")");
            }
        }
    }
}
