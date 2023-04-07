using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ConditionalExpression : BaseNode
    {
        private BaseNode _thenNode;
        private BaseNode _elseNode;
        private BaseNode _conditionNode;

        public ConditionalExpression(BaseNode conditionNode, BaseNode thenNode, BaseNode elseNode) : base(NodeType.ConditionalExpression)
        {
            _thenNode      = thenNode;
            _conditionNode = conditionNode;
            _elseNode      = elseNode;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("(");
            _conditionNode.Print(writer);
            writer.Write(") ? (");
            _thenNode.Print(writer);
            writer.Write(") : (");
            _elseNode.Print(writer);
            writer.Write(")");
        }
    }
}