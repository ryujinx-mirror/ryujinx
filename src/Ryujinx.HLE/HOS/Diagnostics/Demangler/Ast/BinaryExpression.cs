using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class BinaryExpression : BaseNode
    {
        private readonly BaseNode _leftPart;
        private readonly string _name;
        private readonly BaseNode _rightPart;

        public BinaryExpression(BaseNode leftPart, string name, BaseNode rightPart) : base(NodeType.BinaryExpression)
        {
            _leftPart = leftPart;
            _name = name;
            _rightPart = rightPart;
        }

        public override void PrintLeft(TextWriter writer)
        {
            if (_name.Equals(">"))
            {
                writer.Write("(");
            }

            writer.Write("(");
            _leftPart.Print(writer);
            writer.Write(") ");

            writer.Write(_name);

            writer.Write(" (");
            _rightPart.Print(writer);
            writer.Write(")");

            if (_name.Equals(">"))
            {
                writer.Write(")");
            }
        }
    }
}
