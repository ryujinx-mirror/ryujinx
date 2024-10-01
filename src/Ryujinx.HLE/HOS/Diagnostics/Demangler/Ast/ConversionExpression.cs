using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ConversionExpression : BaseNode
    {
        private readonly BaseNode _typeNode;
        private readonly BaseNode _expressions;

        public ConversionExpression(BaseNode typeNode, BaseNode expressions) : base(NodeType.ConversionExpression)
        {
            _typeNode = typeNode;
            _expressions = expressions;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("(");
            _typeNode.Print(writer);
            writer.Write(")(");
            _expressions.Print(writer);
        }
    }
}
