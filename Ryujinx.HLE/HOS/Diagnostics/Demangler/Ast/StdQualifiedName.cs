using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class StdQualifiedName : ParentNode
    {
        public StdQualifiedName(BaseNode Child) : base(NodeType.StdQualifiedName, Child) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("std::");
            Child.Print(Writer);
        }
    }
}
