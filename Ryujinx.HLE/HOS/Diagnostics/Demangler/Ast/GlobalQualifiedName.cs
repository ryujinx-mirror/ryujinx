using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class GlobalQualifiedName : ParentNode
    {
        public GlobalQualifiedName(BaseNode Child) : base(NodeType.GlobalQualifiedName, Child) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("::");
            Child.Print(Writer);
        }
    }
}
