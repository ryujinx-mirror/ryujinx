using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class DeleteExpression : ParentNode
    {
        private bool IsGlobal;
        private bool IsArrayExpression;

        public DeleteExpression(BaseNode Child, bool IsGlobal, bool IsArrayExpression) : base(NodeType.DeleteExpression, Child)
        {
            this.IsGlobal          = IsGlobal;
            this.IsArrayExpression = IsArrayExpression;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            if (IsGlobal)
            {
                Writer.Write("::");
            }

            Writer.Write("delete");

            if (IsArrayExpression)
            {
                Writer.Write("[] ");
            }

            Child.Print(Writer);
        }
    }
}