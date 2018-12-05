using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class DtorName : ParentNode
    {
        public DtorName(BaseNode Name) : base(NodeType.DtOrName, Name) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("~");
            Child.PrintLeft(Writer);
        }
    }
}