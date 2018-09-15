using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class NoexceptSpec : ParentNode
    {
        public NoexceptSpec(BaseNode Child) : base(NodeType.NoexceptSpec, Child) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("noexcept(");
            Child.Print(Writer);
            Writer.Write(")");
        }
    }
}
