using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NoexceptSpec : ParentNode
    {
        public NoexceptSpec(BaseNode child) : base(NodeType.NoexceptSpec, child) { }

        public override void PrintLeft(TextWriter writer)
        {
            writer.Write("noexcept(");
            Child.Print(writer);
            writer.Write(")");
        }
    }
}
