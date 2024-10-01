using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class IntegerCastExpression : ParentNode
    {
        private readonly string _number;

        public IntegerCastExpression(BaseNode type, string number) : base(NodeType.IntegerCastExpression, type)
        {
            _number = number;
        }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("(");
            Child.Print(writer);
            writer.Write(")");
            writer.Write(_number);
        }
    }
}
