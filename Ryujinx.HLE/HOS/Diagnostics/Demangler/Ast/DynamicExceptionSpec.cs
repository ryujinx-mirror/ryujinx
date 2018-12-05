using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class DynamicExceptionSpec : ParentNode
    {
        public DynamicExceptionSpec(BaseNode Child) : base(NodeType.DynamicExceptionSpec, Child) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("throw(");
            Child.Print(Writer);
            Writer.Write(")");
        }
    }
}