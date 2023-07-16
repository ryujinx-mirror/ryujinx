using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PrefixExpression : ParentNode
    {
        private readonly string _prefix;

        public PrefixExpression(string prefix, BaseNode child) : base(NodeType.PrefixExpression, child)
        {
            _prefix = prefix;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write(_prefix);
            writer.Write("(");
            Child.Print(writer);
            writer.Write(")");
        }
    }
}
