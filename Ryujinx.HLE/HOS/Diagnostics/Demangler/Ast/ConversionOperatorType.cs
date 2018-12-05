using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class ConversionOperatorType : ParentNode
    {
        public ConversionOperatorType(BaseNode Child) : base(NodeType.ConversionOperatorType, Child) { }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write("operator ");
            Child.Print(Writer);
        }
    }
}