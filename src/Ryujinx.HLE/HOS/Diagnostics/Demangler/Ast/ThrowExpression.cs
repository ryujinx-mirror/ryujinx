using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ThrowExpression : BaseNode
    {
        private readonly BaseNode _expression;

        public ThrowExpression(BaseNode expression) : base(NodeType.ThrowExpression)
        {
            _expression = expression;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("throw ");
            _expression.Print(writer);
        }
    }
}
