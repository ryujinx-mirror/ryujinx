using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NewExpression : BaseNode
    {
        private NodeArray Expressions;
        private BaseNode  TypeNode;
        private NodeArray Initializers;

        private bool IsGlobal;
        private bool IsArrayExpression;

        public NewExpression(NodeArray Expressions, BaseNode TypeNode, NodeArray Initializers, bool IsGlobal, bool IsArrayExpression) : base(NodeType.NewExpression)
        {
            this.Expressions       = Expressions;
            this.TypeNode          = TypeNode;
            this.Initializers      = Initializers;

            this.IsGlobal          = IsGlobal;
            this.IsArrayExpression = IsArrayExpression;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (IsGlobal)
            {
                Writer.Write("::operator ");
            }

            Writer.Write("new ");

            if (IsArrayExpression)
            {
                Writer.Write("[] ");
            }

            if (Expressions.Nodes.Count != 0)
            {
                Writer.Write("(");
                Expressions.Print(Writer);
                Writer.Write(")");
            }

            TypeNode.Print(Writer);

            if (Initializers.Nodes.Count != 0)
            {
                Writer.Write("(");
                Initializers.Print(Writer);
                Writer.Write(")");
            }
        }
    }
}