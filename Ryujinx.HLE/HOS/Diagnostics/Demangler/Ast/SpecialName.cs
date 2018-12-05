using System.IO;

namespace Ryujinx.HLE.HOS.Diagnostics.Demangler.Ast
{
    public class SpecialName : ParentNode
    {
        private string SpecialValue;

        public SpecialName(string SpecialValue, BaseNode Type) : base(NodeType.SpecialName, Type)
        {
            this.SpecialValue = SpecialValue;
        }

        public override void PrintLeft(TextWriter Writer)
        {
            Writer.Write(SpecialValue);
            Child.Print(Writer);
        }
    }
}