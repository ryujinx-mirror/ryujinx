using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ArraySubscriptingExpression : BaseNode
    {
        private readonly BaseNode _leftNode;
        private readonly BaseNode _subscript;

        public ArraySubscriptingExpression(BaseNode leftNode, BaseNode subscript) : base(NodeType.ArraySubscriptingExpression)
        {
            _leftNode = leftNode;
            _subscript = subscript;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("(");
            _leftNode.Print(writer);
            writer.Write(")[");
            _subscript.Print(writer);
            writer.Write("]");
        }
    }
}
