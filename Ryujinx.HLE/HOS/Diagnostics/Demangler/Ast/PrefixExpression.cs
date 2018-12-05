using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PrefixExpression : ParentNode
    {
        private string Prefix;

        public PrefixExpression(string Prefix, BaseNode Child) : base(NodeType.PrefixExpression, Child)
        {
            this.Prefix = Prefix;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(Prefix);
            Writer.Write("(");
            Child.Print(Writer);
            Writer.Write(")");
        }
    }
}