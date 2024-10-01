using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class FoldExpression : BaseNode
    {
        private readonly bool _isLeftFold;
        private readonly string _operatorName;
        private readonly BaseNode _expression;
        private readonly BaseNode _initializer;

        public FoldExpression(bool isLeftFold, string operatorName, BaseNode expression, BaseNode initializer) : base(NodeType.FunctionParameter)
        {
            _isLeftFold = isLeftFold;
            _operatorName = operatorName;
            _expression = expression;
            _initializer = initializer;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("(");

            if (_isLeftFold && _initializer != null)
            {
                _initializer.Print(writer);
                writer.Write(" ");
                writer.Write(_operatorName);
                writer.Write(" ");
            }

            writer.Write(_isLeftFold ? "... " : " ");
            writer.Write(_operatorName);
            writer.Write(!_isLeftFold ? " ..." : " ");
            _expression.Print(writer);

            if (!_isLeftFold && _initializer != null)
            {
                _initializer.Print(writer);
                writer.Write(" ");
                writer.Write(_operatorName);
                writer.Write(" ");
            }

            writer.Write(")");
        }
    }
}
