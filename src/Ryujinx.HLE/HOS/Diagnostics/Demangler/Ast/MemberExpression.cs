using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class MemberExpression : BaseNode
    {
        private readonly BaseNode _leftNode;
        private readonly string _kind;
        private readonly BaseNode _rightNode;

        public MemberExpression(BaseNode leftNode, string kind, BaseNode rightNode) : base(NodeType.MemberExpression)
        {
            _leftNode = leftNode;
            _kind = kind;
            _rightNode = rightNode;
        }

        public override void PrintLeft(TextWriter writer)
        {
            _leftNode.Print(writer);
            writer.Write(_kind);
            _rightNode.Print(writer);
        }
    }
}
