using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class EnclosedExpression : BaseNode
    {
        private readonly string _prefix;
        private readonly BaseNode _expression;
        private readonly string _postfix;

        public EnclosedExpression(string prefix, BaseNode expression, string postfix) : base(NodeType.EnclosedExpression)
        {
            _prefix = prefix;
            _expression = expression;
            _postfix = postfix;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(_prefix);
            _expression.Print(writer);
            writer.Write(_postfix);
        }
    }
}
