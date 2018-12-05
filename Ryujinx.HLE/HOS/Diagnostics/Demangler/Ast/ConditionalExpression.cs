using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ConditionalExpression : BaseNode
    {
        private BaseNode ThenNode;
        private BaseNode ElseNode;
        private BaseNode ConditionNode;

        public ConditionalExpression(BaseNode ConditionNode, BaseNode ThenNode, BaseNode ElseNode) : base(NodeType.ConditionalExpression)
        {
            this.ThenNode      = ThenNode;
            this.ConditionNode = ConditionNode;
            this.ElseNode      = ElseNode;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("(");
            ConditionNode.Print(Writer);
            Writer.Write(") ? (");
            ThenNode.Print(Writer);
            Writer.Write(") : (");
            ElseNode.Print(Writer);
            Writer.Write(")");
        }
    }
}