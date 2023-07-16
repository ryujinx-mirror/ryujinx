using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class PointerType : BaseNode
    {
        private readonly BaseNode _child;

        public PointerType(BaseNode child) : base(NodeType.PointerType)
        {
            _child = child;
        }

        public override bool HasRightPart()
        {
            return _child.HasRightPart();
        }

        public override void PrintLeft(TextWriter writer)
        {
            _child.PrintLeft(writer);
            if (_child.IsArray())
            {
                writer.Write(" ");
            }

            if (_child.IsArray() || _child.HasFunctions())
            {
                writer.Write("(");
            }

            writer.Write("*");
        }

        public override void PrintRight(TextWriter writer)
        {
            if (_child.IsArray() || _child.HasFunctions())
            {
                writer.Write(")");
            }

            _child.PrintRight(writer);
        }
    }
}
